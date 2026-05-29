using GodotXR.Application.DTOs.Request.Auth;
using GodotXR.Application.DTOs.Response.Auth;

namespace GodotXR.Application.Services
{
    public interface IAuthService
    {
        Task<TokenModel?> Login(LoginRequest request);
    }
}
