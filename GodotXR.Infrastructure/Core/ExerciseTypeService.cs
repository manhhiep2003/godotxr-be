using AutoMapper;
using GodotXR.Application.DTOs.Request.ExerciseType;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.ExerciseType;
using GodotXR.Application.Services;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Infrastructure.Core
{
    public class ExerciseTypeService : IExerciseTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExerciseTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<ExerciseTypeResponse>>> GetAllAsync()
        {
            var exerciseTypes = await _unitOfWork.ExerciseTypeRepository
                .FindAsync(
                    filter: et => !et.IsDeleted,
                    tracked: false
                );

            var result = _mapper.Map<IEnumerable<ExerciseTypeResponse>>(exerciseTypes);
            return ApiResponse<IEnumerable<ExerciseTypeResponse>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<ExerciseTypeResponse>> GetByIdAsync(int id)
        {
            var exerciseType = await _unitOfWork.ExerciseTypeRepository
                .GetFirstOrDefaultAsync(
                    filter: et => et.Id == id && !et.IsDeleted,
                    tracked: false
                );

            if (exerciseType is null)
                return ApiResponse<ExerciseTypeResponse>.FailureResponse("Exercise type not found.");

            var result = _mapper.Map<ExerciseTypeResponse>(exerciseType);
            return ApiResponse<ExerciseTypeResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<ExerciseTypeResponse>> CreateAsync(CreateExerciseTypeRequest request)
        {
            var exerciseType = new ExerciseType
            {
                TypeName = request.TypeName,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ExerciseTypeRepository.AddAsync(exerciseType);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ExerciseTypeResponse>(exerciseType);
            return ApiResponse<ExerciseTypeResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<ExerciseTypeResponse>> UpdateAsync(int id, UpdateExerciseTypeRequest request)
        {
            var exerciseType = await _unitOfWork.ExerciseTypeRepository
                .GetFirstOrDefaultAsync(
                    filter: et => et.Id == id && !et.IsDeleted
                );

            if (exerciseType is null)
                return ApiResponse<ExerciseTypeResponse>.FailureResponse("Exercise type not found.");

            exerciseType.TypeName = request.TypeName;
            exerciseType.Description = request.Description;
            exerciseType.IsActive = request.IsActive;
            exerciseType.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ExerciseTypeResponse>(exerciseType);
            return ApiResponse<ExerciseTypeResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var exerciseType = await _unitOfWork.ExerciseTypeRepository
                .GetFirstOrDefaultAsync(
                    filter: et => et.Id == id && !et.IsDeleted
                );

            if (exerciseType is null)
                return ApiResponse<bool>.FailureResponse("Exercise type not found.");

            exerciseType.IsDeleted = true;
            exerciseType.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true);
        }
    }
}