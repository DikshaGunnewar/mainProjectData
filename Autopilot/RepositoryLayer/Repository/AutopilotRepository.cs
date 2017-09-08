using EntitiesLayer;
using RepositoryLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repository
{
    public class AutopilotRepository : RepositoryBase<UserDetail>, IUserRepository
    {
        public AutopilotRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }
    }
    public interface IUserRepository : IRepositoryBase<UserDetail>
    {

    }
}
