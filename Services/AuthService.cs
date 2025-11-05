// Services/AuthService.cs
using AssetManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AssetManagementDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AssetManagementDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _context.MstUsers.SingleOrDefaultAsync(u => u.Name == username);

            if (user == null || !VerifyPasswordHash(password, user.Password))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }

        public async Task<MstUser> RegisterAsync(MstUser user, string password)
        {
            if (await _context.MstUsers.AnyAsync(u => u.Name == user.Name))
            {
                throw new Exception("Username \"" + user.Name + "\" is already taken");
            }

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(new string(password.Reverse().ToArray()));
            user.Password = plainTextBytes;

            _context.MstUsers.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private string GenerateJwtToken(MstUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    // Add roles as claims
                    new Claim(ClaimTypes.Role, user.Title)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash)
        {
            // Simple verification: reverse the input password, encode it, and compare
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(new string(password.Reverse().ToArray()));
            return plainTextBytes.SequenceEqual(storedHash);
        }
    }
}