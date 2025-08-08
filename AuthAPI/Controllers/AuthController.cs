using AuthAPI.DTOs;
using AuthAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ApiResponse<object>> Register([FromBody] RegisterDto dto)
        {
            return await _authService.RegisterAsync(dto);
        }

        [HttpPost("login")]
        public async Task<ApiResponse<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            return await _authService.LoginAsync(dto);
        }


        [HttpPost("refresh-token")]
        public async Task<ApiResponse<AuthResponseDto>> RefreshToken([FromBody] TokenRefreshDto dto)
        {
            return await _authService.RefreshTokenAsync(dto.RefreshToken);
        }
       
    }

  
}
