using GodotXR.Application.DTOs.Request.User;
using GodotXR.Application.DTOs.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotXR.Application.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllAsync();
        Task<UserResponse?> GetByIdAsync(int id);
        Task<UserResponse> CreateAsync(CreateUserRequest request);
        Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
