using GodotXR.Domain.IRepositories;
using GodotXR.Domain.IUnitOfWork;
using GodotXR.Infrastructure.Data;
using GodotXR.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GodotXR.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories = new();
        private IUserRepository? _userRepository;
        private IRoleRepository? _roleRepository;
        private ILessonRepository? _lessonRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(_context);
                return _userRepository;
            }
        }

        public IRoleRepository RoleRepository
        {
            get
            {
                _roleRepository ??= new RoleRepository(_context);
                return _roleRepository;
            }
        }

        public ILessonRepository LessonRepository
        {
            get
            {
                _lessonRepository ??= new LessonRepository(_context);
                return _lessonRepository;
            }
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.TryGetValue(type, out var repository))
            {
                repository = new GenericRepository<T>(_context);
                _repositories[type] = repository;
            }
            return (IGenericRepository<T>)repository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}