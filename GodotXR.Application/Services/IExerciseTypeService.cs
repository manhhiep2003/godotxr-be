using GodotXR.Application.DTOs.Request.ExerciseType;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.ExerciseType;

namespace GodotXR.Application.Services
{
    public interface IExerciseTypeService
    {
        Task<ApiResponse<IEnumerable<ExerciseTypeResponse>>> GetAllAsync();
        Task<ApiResponse<ExerciseTypeResponse>> GetByIdAsync(int id);
        Task<ApiResponse<ExerciseTypeResponse>> CreateAsync(CreateExerciseTypeRequest request);
        Task<ApiResponse<ExerciseTypeResponse>> UpdateAsync(int id, UpdateExerciseTypeRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}