using GodotXR.Application.DTOs.Request.User;
using GodotXR.Application.DTOs.Response.User;
using GodotXR.Application.Services;
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

        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            var users = await _unitOfWork.UserRepository.FindAsync(
                filter: u => !u.IsDeleted,
                includeProperties: "Role",
                tracked: false);

            return users.Select(MapToResponse);
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                filter: u => u.Id == id && !u.IsDeleted,
                includeProperties: "Role",
                tracked: false);

            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            var exists = await _unitOfWork.UserRepository.ExistsAsync(
                u => u.Username == request.Username && !u.IsDeleted);
            if (exists)
                throw new InvalidOperationException($"Username '{request.Username}' đã tồn tại.");
            var emailExists = await _unitOfWork.UserRepository.ExistsAsync(
                u => u.Email == request.Email && !u.IsDeleted);
            if (emailExists)
                throw new InvalidOperationException($"Email '{request.Email}' đã tồn tại.");
            var role = await _unitOfWork.Repository<Role>().GetFirstOrDefaultAsync(
                r => r.RoleName == request.RoleName && r.IsActive);
            if (role == null)
                throw new InvalidOperationException($"Role '{request.RoleName}' không tồn tại hoặc không hoạt động.");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                RoleId = role.Id,
                IsActive = true
            };
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var created = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == user.Id,
                includeProperties: "Role",
                tracked: false);

            return MapToResponse(created!);
        }

        public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                filter: u => u.Id == id && !u.IsDeleted,
                includeProperties: "Role");

            if (user == null) return null;
            if (request.Email != null && request.Email != user.Email)
            {
                var emailExists = await _unitOfWork.UserRepository.ExistsAsync(
                    u => u.Email == request.Email && u.Id != id && !u.IsDeleted);
                if (emailExists)
                    throw new InvalidOperationException($"Email '{request.Email}' đã được dùng bởi tài khoản khác.");
            }

            if (request.FullName != null) user.FullName = request.FullName;
            if (request.Email != null) user.Email = request.Email;
            if (request.Phone != null) user.Phone = request.Phone;
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

            if (request.RoleName.HasValue)
            {
                var role = await _unitOfWork.Repository<Role>().GetFirstOrDefaultAsync(
                    r => r.RoleName == request.RoleName.Value && r.IsActive);
                if (role == null)
                    throw new InvalidOperationException($"Role '{request.RoleName}' không tồn tại hoặc không hoạt động.");

                user.RoleId = role.Id;
                user.Role = role;
            }
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted);
            if (user == null) return false;
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
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