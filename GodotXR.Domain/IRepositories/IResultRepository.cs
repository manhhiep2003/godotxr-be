using GodotXR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotXR.Domain.IRepositories
{
    public interface IResultRepository : IGenericRepository<Result>
    {
        Task<Result?> GetBySessionIdAsync(string sessionId);
        Task<int> GetAttemptCountAsync(int childId, int exerciseId); 
        Task<Result?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Result>> GetByChildIdAsync(int childId);
        Task<IEnumerable<Result>> GetByExerciseIdAsync(int exerciseId);
    }
}
