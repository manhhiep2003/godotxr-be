using GodotXR.Domain.Entities;

namespace GodotXR.Domain.IRepositories
{
    public interface ISchoolYearRepository : IGenericRepository<SchoolYear>
    {
        Task<bool> HasActiveSchoolYearAsync(int? excludeId = null);
    }
}