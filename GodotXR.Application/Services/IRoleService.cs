using GodotXR.Application.DTOs.Request.Role;
using GodotXR.Application.DTOs.Response.Role;

namespace GodotXR.Application.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetAllAsync();
        Task<RoleResponse?> GetByIdAsync(int id);
        Task<RoleResponse> CreateAsync(CreateRoleRequest request);
        Task<RoleResponse?> UpdateAsync(int id, UpdateRoleRequest request);
        Task<bool> DeleteAsync(int id);
    }
}