using GodotXR.Domain.Entities;
using GodotXR.Domain.IRepositories;
using GodotXR.Infrastructure.Data;


namespace GodotXR.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
