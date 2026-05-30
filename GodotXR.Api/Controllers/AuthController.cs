using GodotXR.Application.DTOs.Response.Auth;
using GodotXR.Application.DTOs.Request.Auth;
using GodotXR.Application.DTOs.Response;
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
        public async Task<ActionResult<ApiResponse<TokenModel>>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<TokenModel>.FailureResponse("Validation failed", errors));
            }

            var result = await _authService.Login(request);

            if (result == null)
            {
                return Unauthorized(ApiResponse<TokenModel>.FailureResponse("Invalid email or password."));
            }

            return Ok(ApiResponse<TokenModel>.SuccessResponse(result, "Login successful"));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<TokenModel>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request is null)
            {
                return BadRequest(ApiResponse<TokenModel>.FailureResponse("Invalid client request"));
            }

            var result = await _authService.RefreshToken(request);

            if (result == null)
            {
                return BadRequest(ApiResponse<TokenModel>.FailureResponse("Invalid token or refresh token"));
            }

            return Ok(ApiResponse<TokenModel>.SuccessResponse(result, "Token refreshed successfully"));
        }
    }
}
