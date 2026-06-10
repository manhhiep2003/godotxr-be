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
        private readonly IMailService _mailService;
        private readonly IPasswordHasherService _passwordHasherService;

        public AuthService(
            IUnitOfWork unitOfWork, 
            ITokenService tokenService, 
            IConfiguration configuration, 
            IDistributedCache cache,
            IMailService mailService,
            IPasswordHasherService passwordHasherService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _cache = cache;
            _mailService = mailService;
            _passwordHasherService = passwordHasherService;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors, TokenModel? Data)> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Email == request.Email,
                includeProperties: "Role");

            if (user == null)       
                return (false, new[] { "Invalid email or password." }, null );
            
            if (!BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash)) 
                return (false, new[] { "Invalid email or password." }, null);        

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var cacheKey = $"refreshToken:{user.Email}";

            await _cache.SetStringAsync(cacheKey, refreshToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
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

        public async Task<(bool Succeeded, IEnumerable<string> Errors, TokenModel? Data)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal == null)
                return (false, new[] { "Invalid access token." }, null);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return (false, new[] { "Invalid token." }, null);

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == userId,
                includeProperties: "Role");

            if (user == null)
                return (false, new[] { "User not found." }, null);

            var cacheKey = $"refreshToken:{user.Email}";

            var savedRefreshToken = await _cache.GetStringAsync(cacheKey);

            if (savedRefreshToken != request.RefreshToken)
                return (false, new[] { "Invalid refresh token." }, null);

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

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)> ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.UserRepository
                .GetFirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return (false, true, Enumerable.Empty<string>());

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            var cacheKey = $"otp:{email}";

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    otpCode,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                var savedOtp = await _cache.GetStringAsync(cacheKey);

                if (savedOtp != otpCode)
                    return (false, false, new[] { "Failed to save OTP to cache." });
            }
            catch (Exception ex)
            {
                return (false, false, new[] { $"Redis error: {ex.Message}" });
            }

            var subject = "Mã OTP Đặt Lại Mật Khẩu - GodotXR";
            var body = $@"
                <div style='font-family: Arial, Helvetica, sans-serif; max-width: 600px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e5e5e5; border-radius: 12px; overflow: hidden;'>

                    <div style='background: linear-gradient(135deg, #4CAF50, #2E7D32); padding: 24px; text-align: center; color: white;'>
                        <h1 style='margin: 0;'>GodotXR</h1>
                        <p style='margin-top: 8px;'>Xác thực yêu cầu đặt lại mật khẩu</p>
                    </div>

                    <div style='padding: 32px;'>

                        <h2 style='color: #333;'>Xin chào {user.FullName},</h2>

                        <p style='color: #555; line-height: 1.8;'>
                            Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản GodotXR của bạn.
                            Để tiếp tục quá trình xác thực, vui lòng sử dụng mã OTP bên dưới:
                        </p>

                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='display: inline-block;
                                        background-color: #f5f5f5;
                                        border: 2px dashed #4CAF50;
                                        border-radius: 10px;
                                        padding: 16px 32px;
                                        font-size: 32px;
                                        font-weight: bold;
                                        letter-spacing: 8px;
                                        color: #2E7D32;'>
                                {otpCode}
                            </div>
                        </div>

                        <p style='color: #555; line-height: 1.8;'>
                            Mã xác thực này sẽ có hiệu lực trong vòng
                            <strong>5 phút</strong>.
                            Vì lý do bảo mật, vui lòng không chia sẻ mã này với bất kỳ ai.
                        </p>

                        <div style='background-color: #FFF8E1;
                                    border-left: 4px solid #FFC107;
                                    padding: 16px;
                                    margin: 24px 0;
                                    color: #666;'>
                            <strong>Lưu ý:</strong><br/>
                            Nếu bạn không thực hiện yêu cầu đặt lại mật khẩu này,
                            bạn có thể bỏ qua email này. Tài khoản của bạn sẽ không bị ảnh hưởng.
                        </div>

                        <p style='color: #555; line-height: 1.8;'>
                            Nếu gặp bất kỳ vấn đề nào trong quá trình sử dụng dịch vụ,
                            vui lòng liên hệ đội ngũ hỗ trợ của GodotXR để được trợ giúp.
                        </p>

                        <p style='margin-top: 32px; color: #555;'>
                            Trân trọng,<br/>
                            <strong>Đội ngũ GodotXR</strong>
                        </p>

                    </div>

                    <div style='background-color: #f8f8f8;
                                text-align: center;
                                padding: 16px;
                                font-size: 12px;
                                color: #888;'>
                        © {DateTime.Now.Year} GodotXR.
                    </div>

                </div>";
            try
            {
                await _mailService.SendEmailAsync(user.Email, subject, body);
                return (true, false, Enumerable.Empty<string>());
            }
            catch (Exception ex)
            {
                return (false, false, new[] { $"Failed to send email: {ex.Message}" });
            }
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _unitOfWork.UserRepository
                .GetFirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return (false, true, Enumerable.Empty<string>());

            var cacheKey = $"otp:{request.Email}";
            var savedOtp = await _cache.GetStringAsync(cacheKey);

            var trimmedSavedOtp = savedOtp?.Trim();
            var trimmedRequestOtp = request.Otp?.Trim();

            if (string.IsNullOrEmpty(trimmedSavedOtp) ||
                trimmedSavedOtp != trimmedRequestOtp)
            {
                return (false, false, new[] { "Invalid or expired OTP code." });
            }

            await _cache.RemoveAsync(cacheKey);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var affectedRows = await _unitOfWork.SaveChangesAsync();

            if (affectedRows <= 0)
                return (false, false, new[] { "Failed to reset password." });

            return (true, false, Enumerable.Empty<string>());
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _unitOfWork.UserRepository
                .GetFirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return (false, true, Enumerable.Empty<string>());

            if (string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                return (false, false, new[] { "All password fields are required." });
            }

            if (request.NewPassword != request.ConfirmPassword)
                return (false, false, new[] { "New password and confirmation password do not match." });

            if (request.Password == request.NewPassword)
                return (false, false, new[] { "New password must be different from the current password." });

            if (!_passwordHasherService.Verify(request.Password, user.PasswordHash))
                return (false, false, new[] { "Current password is incorrect." });

            user.PasswordHash = _passwordHasherService.Hash(request.NewPassword);

            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>());
        }
    }
}
