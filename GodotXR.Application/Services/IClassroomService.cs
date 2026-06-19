using GodotXR.Application.DTOs.Request.Classroom;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Classroom;

namespace GodotXR.Application.Services
{
    public interface IClassroomService
    {
        Task<PagedResponse<ClassroomResponse>> GetListClassroomAsync(
            int pageNumber,
            int pageSize,
            int? semesterId = null,
            int? programId = null,
            int? userId = null,
            string? status = null);

        Task<ClassroomResponse?> GetClassroomByIdAsync(int id);

        Task<(bool Succeeded, IEnumerable<string> Errors, ClassroomResponse? Data)>
            CreateClassroomAsync(CreateClassroomRequest request);

        Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors, ClassroomResponse? Data)>
            UpdateClassroomAsync(int id, UpdateClassroomRequest request);

        Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)>
            DeleteClassroomAsync(int id);
    }
}