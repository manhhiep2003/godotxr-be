using AutoMapper;
using GodotXR.Application.DTOs.Request.Auth;
using GodotXR.Application.DTOs.Response.Auth;
using GodotXR.Application.Services;
using GodotXR.Domain.IUnitOfWork;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;


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

        public async Task<(bool Succeeded,
                   IEnumerable<string> Errors,
                   TokenModel? Data)> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Email == request.Email,
                includeProperties: "Role");

            if (user == null)
            {
                return (
                    false,
                    new[] { "Invalid email or password." },
                    null
                );
            }

            if (!BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash))
            {
                return (
                    false,
                    new[] { "Invalid email or password." },
                    null
                );
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var cacheKey = $"refreshToken:{user.Email}";

            await _cache.SetStringAsync(
                cacheKey,
                refreshToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromDays(7)
                });

            var token = new TokenModel
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

            return (true, Enumerable.Empty<string>(), token);
        }

        public async Task<(bool Succeeded,
                   IEnumerable<string> Errors,
                   TokenModel? Data)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal =
                _tokenService.GetPrincipalFromExpiredToken(
                    request.AccessToken);

            if (principal == null)
            {
                return (
                    false,
                    new[] { "Invalid access token." },
                    null
                );
            }

            var email = principal.Claims
                .FirstOrDefault(c =>
                    c.Type == ClaimTypes.Email)
                ?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return (
                    false,
                    new[] { "Invalid token." },
                    null
                );
            }

            var user = await _unitOfWork.UserRepository
                .GetFirstOrDefaultAsync(
                    u => u.Email == email,
                    includeProperties: "Role");

            var cacheKey = $"refreshToken:{email}";

            var savedRefreshToken =
                await _cache.GetStringAsync(cacheKey);

            if (user == null ||
                savedRefreshToken != request.RefreshToken)
            {
                return (
                    false,
                    new[] { "Invalid refresh token." },
                    null
                );
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _cache.SetStringAsync(
                cacheKey,
                newRefreshToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                });

            return (
                true,
                Enumerable.Empty<string>(),
                new TokenModel
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
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
                }
            );
        }
    }
}
