using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory databaseFactory;
        private AutopilotContext dataContext;
        public UnitOfWork(IDatabaseFactory databaseFactory)
        {
            this.databaseFactory = databaseFactory;
        }
        protected AutopilotContext DataContext
        {
            get { return dataContext ?? (dataContext = databaseFactory.Get()); }
        }
        public void Commit()
        {
            DataContext.Commit();
        }
    }
}
