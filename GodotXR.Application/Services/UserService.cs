using GodotXR.Application.DTOs.Request.User;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.User;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<UserResponse>> GetListUserAsync(int pageNumber, int pageSize)
        {
            var paged = await _unitOfWork.UserRepository.GetPagedAsync(pageNumber, pageSize);

            return new PagedResponse<UserResponse>
            {
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToResponse).ToList(),
            };
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                filter: u => u.Id == id && !u.IsDeleted,
                includeProperties: "Role",
                tracked: false);

            return user == null ? null : MapToResponse(user);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors, UserResponse? Data)> CreateUserAsync(CreateUserRequest request)
        {
            var errors = new List<string>();

            var usernameExists = await _unitOfWork.UserRepository.ExistsAsync(
                u => u.Username == request.Username && !u.IsDeleted);

            if (usernameExists)
                errors.Add($"Username '{request.Username}' đã tồn tại.");

            var emailExists = await _unitOfWork.UserRepository.ExistsAsync(
                u => u.Email == request.Email && !u.IsDeleted);

            if (emailExists)
                errors.Add($"Email '{request.Email}' đã tồn tại.");

            var role = await _unitOfWork.RoleRepository.GetFirstOrDefaultAsync(
                r => r.RoleName == request.RoleName && r.IsActive);

            if (role == null)
                errors.Add($"Role '{request.RoleName}' không tồn tại hoặc không hoạt động.");

            if (errors.Any())
                return (false, errors, null);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                RoleId = role!.Id,
                IsActive = true
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var createdUser = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == user.Id,
                includeProperties: "Role",
                tracked: false);

            return (true, Enumerable.Empty<string>(), MapToResponse(createdUser!));
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors, UserResponse? Data)> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                filter: u => u.Id == id && !u.IsDeleted,
                includeProperties: "Role");

            if (user == null)
                return (false, true, Enumerable.Empty<string>(), null);

            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Email) &&
                request.Email != user.Email)
            {
                var emailExists = await _unitOfWork.UserRepository.ExistsAsync(
                    u => u.Email == request.Email &&
                         u.Id != id &&
                         !u.IsDeleted);

                if (emailExists)
                    errors.Add($"Email '{request.Email}' đã được dùng bởi tài khoản khác.");
            }

            if (request.RoleName.HasValue)
            {
                var role = await _unitOfWork.RoleRepository.GetFirstOrDefaultAsync(
                    r => r.RoleName == request.RoleName.Value && r.IsActive);

                if (role == null)
                {
                    errors.Add(
                        $"Role '{request.RoleName}' không tồn tại hoặc không hoạt động.");
                }
                else
                {
                    user.RoleId = role.Id;
                    user.Role = role;
                }
            }

            if (errors.Any())
                return (false, false, errors, null);

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone;

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (
                true,
                false,
                Enumerable.Empty<string>(),
                MapToResponse(user)
            );
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)> DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return (false, true, Enumerable.Empty<string>());

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>());
        }

        private static UserResponse MapToResponse(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone ?? string.Empty,
            RoleName = user.Role?.RoleName.ToString() ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}