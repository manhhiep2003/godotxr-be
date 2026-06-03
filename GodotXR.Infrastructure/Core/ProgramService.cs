using AutoMapper;
using GodotXR.Application.DTOs.Request.Program;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Program;
using GodotXR.Application.Services;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Infrastructure.Core
{
    public class ProgramService : IProgramService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProgramService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<ProgramResponse>>> GetAllAsync()
        {
            var programs = await _unitOfWork.Repository<Program>()
                .FindAsync(
                    filter: p => !p.IsDeleted,
                    tracked: false
                );

            var result = _mapper.Map<IEnumerable<ProgramResponse>>(programs);
            return ApiResponse<IEnumerable<ProgramResponse>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<ProgramResponse>> GetByIdAsync(int id)
        {
            var program = await _unitOfWork.Repository<Program>()
                .GetFirstOrDefaultAsync(
                    filter: p => p.Id == id && !p.IsDeleted,
                    includeProperties: "Lessons",
                    tracked: false
                );

            if (program is null)
                return ApiResponse<ProgramResponse>.FailureResponse("Program not found.");

            var result = _mapper.Map<ProgramResponse>(program);
            return ApiResponse<ProgramResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<ProgramResponse>> CreateAsync(CreateProgramRequest request)
        {
            var program = new Program
            {
                ProgramName = request.ProgramName,
                Description = request.Description,
                TargetAgeFrom = request.TargetAgeFrom,
                TargetAgeTo = request.TargetAgeTo,
                Language = request.Language,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Program>().AddAsync(program);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramResponse>(program);
            return ApiResponse<ProgramResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<ProgramResponse>> UpdateAsync(int id, UpdateProgramRequest request)
        {
            var program = await _unitOfWork.Repository<Program>()
                .GetFirstOrDefaultAsync(
                    filter: p => p.Id == id && !p.IsDeleted
                );

            if (program is null)
                return ApiResponse<ProgramResponse>.FailureResponse("Program not found.");

            program.ProgramName = request.ProgramName;
            program.Description = request.Description;
            program.TargetAgeFrom = request.TargetAgeFrom;
            program.TargetAgeTo = request.TargetAgeTo;
            program.Language = request.Language;
            program.Status = request.Status;
            program.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramResponse>(program);
            return ApiResponse<ProgramResponse>.SuccessResponse(result);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var program = await _unitOfWork.Repository<Program>()
                .GetFirstOrDefaultAsync(
                    filter: p => p.Id == id && !p.IsDeleted
                );

            if (program is null)
                return ApiResponse<bool>.FailureResponse("Program not found.");

            program.IsDeleted = true;
            program.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true);
        }
    }
}