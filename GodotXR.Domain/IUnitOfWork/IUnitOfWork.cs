using GodotXR.Domain.IRepositories;

namespace GodotXR.Domain.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }

        IRoleRepository RoleRepository { get; }

        IProgramRepository ProgramRepository { get; }

        ILessonRepository LessonRepository { get; }
        // Generic repository method (for other entities if needed)

        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
