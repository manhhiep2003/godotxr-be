using GodotXR.Application.DTOs.Request.User;
using GodotXR.Application.DTOs.Response.User;
using GodotXR.Application.Services;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IRepositories;
using GodotXR.Domain.IUnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace GodotXR.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private IGenericRepository<User> Users => _unitOfWork.Repository<User>();
        private IGenericRepository<Role> Roles => _unitOfWork.Repository<Role>();

        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            var users = await Users.FindAsync(
                filter: u => u.IsActive && !u.IsDeleted,
                orderBy: q => q.OrderByDescending(x => x.Id),
                includeProperties: "Role",
                tracked: false);

            return users.Select(MapToResponse);
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await Users.GetFirstOrDefaultAsync(
                filter: u => u.Id == id && u.IsActive && !u.IsDeleted,
                includeProperties: "Role",
                tracked: false);

            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            var usernameExists = await Users.ExistsAsync(u => u.Username == request.Username && !u.IsDeleted);
            if (usernameExists)
                throw new InvalidOperationException("Username đã tồn tại.");

            var emailExists = await Users.ExistsAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (emailExists)
                throw new InvalidOperationException("Email đã tồn tại.");

            var role = await Roles.GetFirstOrDefaultAsync(
                r => r.RoleName == request.RoleName && r.IsActive && !r.IsDeleted,
                tracked: false);

            if (role == null)
                throw new InvalidOperationException("Role không tồn tại hoặc đang bị vô hiệu hóa.");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Gender = request.Gender,
                Specialty = request.Specialty,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var created = await Users.GetFirstOrDefaultAsync(
                u => u.Id == user.Id,
                includeProperties: "Role",
                tracked: false);

            return MapToResponse(created!);
        }

        public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
        {
            var user = await Users.GetFirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                includeProperties: "Role",
                tracked: true);

            if (user == null)
                return null;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailExists = await Users.ExistsAsync(u => u.Email == request.Email && u.Id != id && !u.IsDeleted);
                if (emailExists)
                    throw new InvalidOperationException("Email đã tồn tại.");

                user.Email = request.Email;
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone;

            if (!string.IsNullOrWhiteSpace(request.Gender))
                user.Gender = request.Gender;

            if (!string.IsNullOrWhiteSpace(request.Specialty))
                user.Specialty = request.Specialty;

            if (request.RoleName.HasValue)
            {
                var role = await Roles.GetFirstOrDefaultAsync(
                    r => r.RoleName == request.RoleName.Value && r.IsActive && !r.IsDeleted,
                    tracked: false);

                if (role == null)
                    throw new InvalidOperationException("Role không tồn tại hoặc đang bị vô hiệu hóa.");

                user.RoleId = role.Id;
            }

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var updated = await Users.GetFirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                includeProperties: "Role",
                tracked: false);

            return updated == null ? null : MapToResponse(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await Users.GetFirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                tracked: true);

            if (user == null)
                return false;

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Gender = user.Gender,
                Specialty = user.Specialty,
                RoleName = user.Role?.RoleName.ToString() ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}