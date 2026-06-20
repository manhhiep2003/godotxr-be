using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.ChildProfile;
using GodotXR.Domain.Entities;

namespace GodotXR.Application.Services
{
    public interface IChildProfileService
    {
        Task<PagedResponse<ChildProfileResponse>> GetListChildProfileAsync(int pageNumber, int pageSize);
    
        Task<ChildProfileResponse?> GetChildProfileByIdAsync(int id);
    }
}
