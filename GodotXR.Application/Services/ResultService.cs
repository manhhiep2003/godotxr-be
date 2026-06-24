using AutoMapper;
using GodotXR.Application.DTOs.Request.Result;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Result;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;
using GodotXR.Application.DTOs.Response.PronunciationDetail;
using GodotXR.Application.DTOs.Response.EventLog;
namespace GodotXR.Application.Services
{
    public class ResultService : IResultService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ResultService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors, ResultResponse? Data)> SubmitAsync(SubmitResultRequest request)
        {
            var errors = new List<string>();

            // BR-80: idempotent — cùng SessionId không tạo duplicate
            var existing = await _uow.ResultRepository.GetBySessionIdAsync(request.SessionId);
            if (existing != null)
                return (true, false, Array.Empty<string>(), _mapper.Map<ResultResponse>(existing));

            // BR-75: Child phải active
            var child = await _uow.ChildProfileRepository.GetByIdAsync(request.ChildId);
            if (child == null)
                return (false, true, new[] { "Child not found." }, null);
            if (child.Status != "Active")
                errors.Add("Child profile is not active.");

            // BR-76: phải có active Enrollment
            var enrollments = await _uow.EnrollmentRepository.GetByChildIdAsync(request.ChildId);
            if (!enrollments.Any(e => e.Status == "Active" && !e.IsDeleted))
                errors.Add("Child does not have an active enrollment.");

            // BR-84: Exercise phải tồn tại
            var exercise = await _uow.ExerciseRepository.GetByIdAsync(request.ExerciseId);
            if (exercise == null)
                return (false, true, new[] { "Exercise not found." }, null);

            // BR-74: Exercise phải active
            if (exercise.Status != "Active")
                errors.Add("Exercise is not active.");

            // BR-86: Score range
            if (request.Score < 0 || request.Score > 100)
                errors.Add("Score must be between 0 and 100.");

            // BR-87: Duration không âm
            if (request.DurationSeconds < 0)
                errors.Add("Duration must not be negative.");

            // BR-85: CompletionStatus hợp lệ
            var validStatuses = new[] { "Completed", "Incomplete" };
            if (!validStatuses.Contains(request.CompletionStatus))
                errors.Add("Invalid completion status. Must be 'Completed' or 'Incomplete'.");

            // BR-97: AccuracyScore range
            foreach (var pd in request.PronunciationDetails)
            {
                if (pd.AccuracyScore < 0 || pd.AccuracyScore > 100)
                    errors.Add("AccuracyScore must be between 0 and 100.");
            }

            if (errors.Any())
                return (false, false, errors, null);

            // BR-78: AttemptNumber tự tăng
            var attemptCount = await _uow.ResultRepository.GetAttemptCountAsync(request.ChildId, request.ExerciseId);

            var result = new Result
            {
                SessionId = request.SessionId,
                ChildId = request.ChildId,
                ExerciseId = request.ExerciseId,
                AttemptNumber = attemptCount + 1,
                CompletionStatus = request.CompletionStatus,
                Score = request.Score,
                StartedAt = request.StartedAt,
                CompletedAt = request.CompletedAt,
                DurationSeconds = request.DurationSeconds,
                AudioRecordUrl = request.AudioRecordUrl,
                ReplayDataUrl = request.ReplayDataUrl,
                InteractionLog = request.InteractionLog,
                FeedbackText = request.FeedbackText,
                IsFinalized = request.CompletionStatus == "Completed", // BR-89
            };

            await _uow.ResultRepository.AddAsync(result);
            await _uow.SaveChangesAsync(); // Save trước để có ResultId cho FK

            // BR-95, 98: PronunciationDetails phải có ResultId hợp lệ
            foreach (var pdReq in request.PronunciationDetails)
            {
                var pd = new PronunciationDetail
                {
                    ResultId = result.Id,
                    ExpectedPhoneme = pdReq.ExpectedPhoneme,
                    ActualPhoneme = pdReq.ActualPhoneme,
                    AccuracyScore = pdReq.AccuracyScore,
                    IssueType = pdReq.IssueType,
                    ReplayDataUrl = pdReq.ReplayDataUrl,
                };
                await _uow.PronunciationDetailRepository.AddAsync(pd);
            }

            // BR-175: EventLogs
            foreach (var evReq in request.EventLogs)
            {
                var ev = new EventLog
                {
                    ResultId = result.Id,
                    ChildId = request.ChildId,
                    EventType = evReq.EventType,
                    EventTime = evReq.EventTime,
                    Description = evReq.Description,
                };
                await _uow.EventLogRepository.AddAsync(ev);
            }

            await _uow.SaveChangesAsync();

            var fullResult = await _uow.ResultRepository.GetWithDetailsAsync(result.Id);
            return (true, false, Array.Empty<string>(), _mapper.Map<ResultResponse>(fullResult));
        }

        public async Task<ApiResponse<ResultResponse>> GetByIdAsync(int id)
        {
            var result = await _uow.ResultRepository.GetWithDetailsAsync(id);
            if (result == null)
                return new ApiResponse<ResultResponse> { Success = false, Message = "Result not found." };

            return new ApiResponse<ResultResponse>
            {
                Success = true,
                Message = "Success.",
                Data = _mapper.Map<ResultResponse>(result)
            };
        }

        public async Task<ApiResponse<IEnumerable<ResultResponse>>> GetByChildIdAsync(int childId)
        {
            var child = await _uow.ChildProfileRepository.GetByIdAsync(childId);
            if (child == null)
                return new ApiResponse<IEnumerable<ResultResponse>> { Success = false, Message = "Child not found." };

            var results = await _uow.ResultRepository.GetByChildIdAsync(childId);
            return new ApiResponse<IEnumerable<ResultResponse>>
            {
                Success = true,
                Message = "Success.",
                Data = _mapper.Map<IEnumerable<ResultResponse>>(results)
            };
        }

        public async Task<ApiResponse<IEnumerable<ResultResponse>>> GetByExerciseIdAsync(int exerciseId)
        {
            var exercise = await _uow.ExerciseRepository.GetByIdAsync(exerciseId);
            if (exercise == null)
                return new ApiResponse<IEnumerable<ResultResponse>> { Success = false, Message = "Exercise not found." };

            var results = await _uow.ResultRepository.GetByExerciseIdAsync(exerciseId);
            return new ApiResponse<IEnumerable<ResultResponse>>
            {
                Success = true,
                Message = "Success.",
                Data = _mapper.Map<IEnumerable<ResultResponse>>(results)
            };
        }
    }
}