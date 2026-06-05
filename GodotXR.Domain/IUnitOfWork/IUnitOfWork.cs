using GodotXR.Domain.IRepositories;

namespace GodotXR.Domain.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        // Generic repository method (for other entities if needed)
        ILessonRepository LessonRepository { get; }
        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
