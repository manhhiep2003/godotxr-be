using GodotXR.Domain.Entities;
using GodotXR.Domain.IRepositories;
using GodotXR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GodotXR.Infrastructure.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context) { }

        public async Task<Enrollment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.Child)
                .Include(e => e.Classroom)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<IEnumerable<Enrollment>> GetAllWithDetailsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.Child)
                .Include(e => e.Classroom)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasActiveEnrollmentAsync(int childId, int classId, int? excludeId = null)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.ChildId == childId
                            && e.ClassId == classId
                            && e.Status == "Active"
                            && !e.IsDeleted
                            && (excludeId == null || e.Id != excludeId));
        }
    }
}