using AutoMapper;
using GodotXR.Application.DTOs.Request.Lesson;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Lesson;
using GodotXR.Application.Services;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Infrastructure.Core
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<LessonResponse>>> GetAllAsync()
        {
            var lessons = await _unitOfWork.LessonRepository
                .FindAsync(
                    filter: l => !l.IsDeleted,
                    tracked: false
                );

            var result = _mapper.Map<IEnumerable<LessonResponse>>(lessons);
            return ApiResponse<IEnumerable<LessonResponse>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<IEnumerable<LessonResponse>>> GetByProgramIdAsync(int programId)
        {
            var lessons = await _unitOfWork.LessonRepository
                .FindAsync(
                    filter: l => l.ProgramId == programId && !l.IsDeleted,
                    tracked: false
                );

            var result = _mapper.Map<IEnumerable<LessonResponse>>(lessons);
            return ApiResponse<IEnumerable<LessonResponse>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<LessonResponse>> GetByIdAsync(int id)
        {
            var lesson = await _unitOfWork.LessonRepository
                .GetFirstOrDefaultAsync(
                    filter: l => l.Id == id && !l.IsDeleted,
                    tracked: false
                );

            if (lesson is null)
                return ApiResponse<LessonResponse>.FailureResponse("Lesson not found.");

            var result = _mapper.Map<LessonResponse>(lesson);
            return ApiResponse<LessonResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<LessonResponse>> CreateAsync(CreateLessonRequest request)
        {
            // Kiểm tra Program tồn tại
            var program = await _unitOfWork.Repository<Program>()
                .GetFirstOrDefaultAsync(
                    filter: p => p.Id == request.ProgramId && !p.IsDeleted,
                    tracked: false
                );

            if (program is null)
                return ApiResponse<LessonResponse>.FailureResponse("Program not found.");

            var lesson = new Lesson
            {
                ProgramId = request.ProgramId,
                LessonName = request.LessonName,
                LessonOrder = request.LessonOrder,
                Description = request.Description,
                TargetSkill = request.TargetSkill,
                EstimatedDuration = request.EstimatedDuration,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LessonRepository.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<LessonResponse>(lesson);
            return ApiResponse<LessonResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<LessonResponse>> UpdateAsync(int id, UpdateLessonRequest request)
        {
            var lesson = await _unitOfWork.LessonRepository
                .GetFirstOrDefaultAsync(
                    filter: l => l.Id == id && !l.IsDeleted
                );

            if (lesson is null)
                return ApiResponse<LessonResponse>.FailureResponse("Lesson not found.");

            lesson.LessonName = request.LessonName;
            lesson.LessonOrder = request.LessonOrder;
            lesson.Description = request.Description;
            lesson.TargetSkill = request.TargetSkill;
            lesson.EstimatedDuration = request.EstimatedDuration;
            lesson.Status = request.Status;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<LessonResponse>(lesson);
            return ApiResponse<LessonResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var lesson = await _unitOfWork.LessonRepository
                .GetFirstOrDefaultAsync(
                    filter: l => l.Id == id && !l.IsDeleted
                );

            if (lesson is null)
                return ApiResponse<bool>.FailureResponse("Lesson not found.");

            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true);
        }
    }
}