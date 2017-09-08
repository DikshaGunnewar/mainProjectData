using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Infrastructure
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly IDbFactory dbFactory;
        private AutopilotContext dbContext;

        public UnitOfWork(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public AutopilotContext DbContext
        {
            get { return dbContext ?? (dbContext = dbFactory.Init()); }
        }
        public void Commit()
        {
            DbContext.Commit();
        }

    }
}
