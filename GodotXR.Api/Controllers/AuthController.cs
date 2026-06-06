using GodotXR.Application.DTOs.Request.Auth;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Auth;
using GodotXR.Application.DTOs.Response.User;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace GodotXR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IDistributedCache _cache;

        public AuthController(IAuthService authService, IDistributedCache cache)
        {
            _authService = authService;
            _cache = cache;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<TokenModel>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<TokenModel>
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = validErrors
                });
            }

            var (ok, errors, data) = await _authService.LoginAsync(request);

            if (!ok || data == null)
            {
                return Unauthorized(new ApiResponse<TokenModel>
                {
                    Success = false,
                    Message = "Login failed.",
                    Errors = errors.ToList()
                });
            }

            return Ok(new ApiResponse<TokenModel>
            {
                Success = true,
                Message = "Login successful.",
                Data = data
            });
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<TokenModel>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<TokenModel>
                {
                    Success = false,
                    Message = "Invalid request."
                });
            }

            var (ok, errors, data)
                = await _authService.RefreshTokenAsync(request);

            if (!ok || data == null)
            {
                return BadRequest(new ApiResponse<TokenModel>
                {
                    Success = false,
                    Message = "Refresh token failed.",
                    Errors = errors.ToList()
                });
            }

            return Ok(new ApiResponse<TokenModel>
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Data = data
            });
        }
    }
}
