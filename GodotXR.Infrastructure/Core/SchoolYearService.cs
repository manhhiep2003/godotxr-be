using AutoMapper;
using GodotXR.Application.DTOs.Request.SchoolYear;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.SchoolYear;
using GodotXR.Application.Services;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Application.Services
{
    public class SchoolYearService : ISchoolYearService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SchoolYearService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<SchoolYearResponse>> GetListSchoolYearAsync(
            int pageNumber, int pageSize,
            string? status = null, string? search = null)
        {
            var paged = await _unitOfWork.SchoolYearRepository.GetPagedAsync(
                pageNumber, pageSize,
                predicate: sy =>
                    !sy.IsDeleted &&
                    (string.IsNullOrWhiteSpace(status) || sy.Status == status),
                orderBy: q => q.OrderByDescending(sy => sy.StartDate),
                includeProperties: "Semesters"
            );

            return new PagedResponse<SchoolYearResponse>
            {
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = _mapper.Map<List<SchoolYearResponse>>(paged.Items)
            };
        }

        public async Task<SchoolYearResponse?> GetSchoolYearByIdAsync(int id)
        {
            var schoolYear = await _unitOfWork.SchoolYearRepository
                .GetFirstOrDefaultAsync(
                    filter: sy => sy.Id == id && !sy.IsDeleted,
                    includeProperties: "Semesters",
                    tracked: false);

            return schoolYear == null ? null : _mapper.Map<SchoolYearResponse>(schoolYear);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors, SchoolYearResponse? Data)>
            CreateSchoolYearAsync(CreateSchoolYearRequest request)
        {
            if (request.StartDate >= request.EndDate)
                return (false, new[] { "Start date must be before end date." }, null);
            if (request.Status == "Active")
            {
                var hasActive = await _unitOfWork.SchoolYearRepository.HasActiveSchoolYearAsync();
                if (hasActive)
                    return (false, new[] { "Another school year is already active." }, null);
            }

            var schoolYear = new SchoolYear
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SchoolYearRepository.AddAsync(schoolYear);
            await _unitOfWork.SaveChangesAsync();

            return (true, Enumerable.Empty<string>(), _mapper.Map<SchoolYearResponse>(schoolYear));
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors, SchoolYearResponse? Data)>
            UpdateSchoolYearAsync(int id, UpdateSchoolYearRequest request)
        {
            var schoolYear = await _unitOfWork.SchoolYearRepository
                .GetFirstOrDefaultAsync(
                    filter: sy => sy.Id == id && !sy.IsDeleted,
                    includeProperties: "Semesters");

            if (schoolYear == null)
                return (false, true, Enumerable.Empty<string>(), null);
            if (request.StartDate >= request.EndDate)
                return (false, false, new[] { "Start date must be before end date." }, null);
            if (request.Status == "Active" && schoolYear.Status != "Active")
            {
                var hasActive = await _unitOfWork.SchoolYearRepository.HasActiveSchoolYearAsync(excludeId: id);
                if (hasActive)
                    return (false, false, new[] { "Another school year is already active." }, null);
            }

            schoolYear.StartDate = request.StartDate;
            schoolYear.EndDate = request.EndDate;
            schoolYear.Status = request.Status;
            schoolYear.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>(), _mapper.Map<SchoolYearResponse>(schoolYear));
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)>
            DeleteSchoolYearAsync(int id)
        {
            var schoolYear = await _unitOfWork.SchoolYearRepository
                .GetFirstOrDefaultAsync(
                    filter: sy => sy.Id == id && !sy.IsDeleted,
                    includeProperties: "Semesters");

            if (schoolYear == null)
                return (false, true, Enumerable.Empty<string>());

            if (schoolYear.Semesters.Any(s => !s.IsDeleted))
                return (false, false, new[] { "Cannot delete a school year that has active semesters." });

            if (schoolYear.Status == "Active")
                return (false, false, new[] { "Cannot delete an active school year." });

            schoolYear.IsDeleted = true;
            schoolYear.DeletedAt = DateTime.UtcNow;
            schoolYear.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>());
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors, SchoolYearResponse? Data)>
            SetActiveSchoolYearAsync(int id)
        {
            var schoolYear = await _unitOfWork.SchoolYearRepository
                .GetFirstOrDefaultAsync(filter: sy => sy.Id == id && !sy.IsDeleted);

            if (schoolYear == null)
                return (false, true, Enumerable.Empty<string>(), null);

            if (schoolYear.Status == "Active")
                return (false, false, new[] { "This school year is already active." }, null);
            var currentActives = await _unitOfWork.SchoolYearRepository
                .FindAsync(filter: sy => !sy.IsDeleted && sy.Status == "Active");

            foreach (var active in currentActives)
            {
                active.Status = "Completed";
                active.UpdatedAt = DateTime.UtcNow;
            }

            schoolYear.Status = "Active";
            schoolYear.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>(), _mapper.Map<SchoolYearResponse>(schoolYear));
        }
    }
}