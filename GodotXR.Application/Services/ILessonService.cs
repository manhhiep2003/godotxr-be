using GodotXR.Application.DTOs.Request.Lesson;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Lesson;

namespace GodotXR.Application.Services
{
    public interface ILessonService
    {
        Task<ApiResponse<IEnumerable<LessonResponse>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<LessonResponse>>> GetByProgramIdAsync(int programId);
        Task<ApiResponse<LessonResponse>> GetByIdAsync(int id);
        Task<ApiResponse<LessonResponse>> CreateAsync(CreateLessonRequest request);
        Task<ApiResponse<LessonResponse>> UpdateAsync(int id, UpdateLessonRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}