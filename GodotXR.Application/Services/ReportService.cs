using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GodotXR.Application.DTOs.Request.Report;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Report;
using GodotXR.Domain.Entities;
using GodotXR.Domain.IUnitOfWork;

namespace GodotXR.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<ReportResponse>> GetListReportAsync(int pageNumber, int pageSize)
        {
            var paged = await _unitOfWork.ReportRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                r => !r.IsDeleted,
                includeProperties: "GeneratedByUser");

            return new PagedResponse<ReportResponse>
            {
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToResponse).ToList()
            };
        }

        public async Task<ReportResponse?> GetReportByIdAsync(int id)
        {
            var report = await _unitOfWork.ReportRepository.GetFirstOrDefaultAsync(
                filter: r => r.Id == id && !r.IsDeleted,
                includeProperties: "GeneratedByUser",
                tracked: false);

            return report == null ? null : MapToResponse(report);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors, ReportResponse? Data)> CreateReportAsync(int generatedBy, CreateReportRequest request)
        {
            var errors = new List<string>();

            // BR-125: chỉ dùng valid data — kiểm tra AnalyzeId tồn tại
            var analyzeExists = await _unitOfWork.AnalyzeRepository.ExistsAsync(
                a => a.Id == request.AnalyzeId && !a.IsDeleted);

            if (!analyzeExists)
                errors.Add($"Analyze '{request.AnalyzeId}' không tồn tại.");

            if (errors.Any())
                return (false, errors, null);

            var report = new Report
            {
                AnalyzeId = request.AnalyzeId,
                GeneratedBy = generatedBy,
                ReportFormat = request.ReportFormat,
                FileUrl = request.FileUrl
            };

            await _unitOfWork.ReportRepository.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.ReportRepository.GetFirstOrDefaultAsync(
                r => r.Id == report.Id,
                includeProperties: "GeneratedByUser",
                tracked: false);

            return (true, Enumerable.Empty<string>(), MapToResponse(created!));
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors, ReportResponse? Data)> UpdateReportAsync(int id, UpdateReportRequest request)
        {
            var report = await _unitOfWork.ReportRepository.GetFirstOrDefaultAsync(
                r => r.Id == id && !r.IsDeleted,
                includeProperties: "GeneratedByUser");

            if (report == null)
                return (false, true, Enumerable.Empty<string>(), null);

            if (!string.IsNullOrWhiteSpace(request.ReportFormat))
                report.ReportFormat = request.ReportFormat;

            if (request.FileUrl != null)
                report.FileUrl = request.FileUrl;

            report.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ReportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>(), MapToResponse(report));
        }

        public async Task<(bool Succeeded, bool NotFound, IEnumerable<string> Errors)> DeleteReportAsync(int id)
        {
            var report = await _unitOfWork.ReportRepository.GetFirstOrDefaultAsync(
                r => r.Id == id && !r.IsDeleted);

            if (report == null)
                return (false, true, Enumerable.Empty<string>());

            report.IsDeleted = true;
            report.DeletedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ReportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();

            return (true, false, Enumerable.Empty<string>());
        }

        private static ReportResponse MapToResponse(Report r) => new()
        {
            Id = r.Id,
            AnalyzeId = r.AnalyzeId,
            GeneratedBy = r.GeneratedBy,
            GeneratedByName = r.GeneratedByUser?.FullName ?? string.Empty,
            ReportFormat = r.ReportFormat,
            FileUrl = r.FileUrl,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}