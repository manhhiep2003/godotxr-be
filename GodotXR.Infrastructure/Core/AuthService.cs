using AutoMapper;
using GodotXR.Application.DTOs.Request.Auth;
using GodotXR.Application.DTOs.Response.Auth;
using GodotXR.Application.Services;
using GodotXR.Domain.IUnitOfWork;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;


namespace GodotXR.Infrastructure.Core
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IDistributedCache _cache;

        public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IConfiguration configuration, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _cache = cache;
        }

        public async Task<TokenModel?> Login(LoginRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Username == request.Username,
                includeProperties: "Role");

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();


            var cacheKey = $"refreshToken:{user.Email}";
            await _cache.SetStringAsync(cacheKey, refreshToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });

            await _unitOfWork.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserAuthInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Username = user.Username,
                    Phone = user.Phone ?? string.Empty,
                    RoleName = user.Role.RoleName.ToString(),
                    IsActive = user.IsActive
                }
            };
        }
    }
}
