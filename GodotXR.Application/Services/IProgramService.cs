using GodotXR.Application.DTOs.Request.Program;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Program;

namespace GodotXR.Application.Services
{
    public interface IProgramService
    {
        Task<ApiResponse<IEnumerable<ProgramResponse>>> GetAllAsync();
        Task<ApiResponse<ProgramResponse>> GetByIdAsync(int id);
        Task<ApiResponse<ProgramResponse>> CreateAsync(CreateProgramRequest request);
        Task<ApiResponse<ProgramResponse>> UpdateAsync(int id, UpdateProgramRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}