using GodotXR.Application.DTOs.Request.Role;
using GodotXR.Application.DTOs.Response.Role;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            var roles = await _unitOfWork.Repository<Role>().FindAsync(
                filter: r => !r.IsDeleted,
                tracked: false);

            return roles.Select(MapToResponse);
        }

        public async Task<RoleResponse?> GetByIdAsync(int id)
        {
            var role = await _unitOfWork.Repository<Role>().GetFirstOrDefaultAsync(
                filter: r => r.Id == id && !r.IsDeleted,
                tracked: false);

            return role == null ? null : MapToResponse(role);
        }

        public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
        {
            var exists = await _unitOfWork.Repository<Role>().ExistsAsync(
                r => r.RoleName == request.RoleName && !r.IsDeleted);
            if (exists)
                throw new InvalidOperationException($"Role '{request.RoleName}' đã tồn tại.");

            var role = new Role
            {
                RoleName = request.RoleName,
                Description = request.Description,
                IsActive = true
            };

            await _unitOfWork.Repository<Role>().AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(role);
        }

        public async Task<RoleResponse?> UpdateAsync(int id, UpdateRoleRequest request)
        {
            var role = await _unitOfWork.Repository<Role>().GetFirstOrDefaultAsync(
                filter: r => r.Id == id && !r.IsDeleted);

            if (role == null) return null;

            if (request.RoleName.HasValue && request.RoleName.Value != role.RoleName)
            {
                var exists = await _unitOfWork.Repository<Role>().ExistsAsync(
                    r => r.RoleName == request.RoleName.Value && r.Id != id && !r.IsDeleted);
                if (exists)
                    throw new InvalidOperationException($"Role '{request.RoleName}' đã tồn tại.");

                role.RoleName = request.RoleName.Value;
            }

            if (request.Description != null) role.Description = request.Description;
            if (request.IsActive.HasValue) role.IsActive = request.IsActive.Value;

            role.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Role>().Update(role);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(role);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _unitOfWork.Repository<Role>().GetFirstOrDefaultAsync(
                r => r.Id == id && !r.IsDeleted);
            if (role == null) return false;

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Role>().Update(role);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static RoleResponse MapToResponse(Role role) => new()
        {
            Id = role.Id,
            RoleName = role.RoleName.ToString(),
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }
}