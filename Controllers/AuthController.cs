// Controllers/AuthController.cs
using AssetManagement.API.Models;
using AssetManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Register(MstUser user, string password)
        {
            try
            {
                var createdUser = await _authService.RegisterAsync(user, password);
                return Ok(createdUser);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var token = await _authService.LoginAsync(username, password);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(new { token });
        }
    }
}