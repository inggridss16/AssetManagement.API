// Controllers/AuthController.cs
using AssetManagement.API.Models;
using AssetManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Map DTO to the MstUser entity
                var user = new MstUser
                {
                    Name = registerDto.Name,
                    DepartmentId = registerDto.DepartmentId,
                    Title = registerDto.Title,
                    SupervisorId = registerDto.SupervisorId,
                    IsAssetManager = registerDto.IsAssetManager,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 0 // Represents self-registration or a system default
                };

                var createdUser = await _authService.RegisterAsync(user, registerDto.Password);

                // Avoid returning the createdUser object which contains the password hash
                return Ok(new { Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _authService.LoginAsync(loginDto.Username, loginDto.Password);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(new { token });
        }
    }
}