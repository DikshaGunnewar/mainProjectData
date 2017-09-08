using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Infrastructure
{
    public class DatabaseFactory : Disposable, IDatabaseFactory
    {
        private AutopilotContext dataContext;
        public AutopilotContext Get()
        {
            return dataContext ?? (dataContext = new AutopilotContext());
        }
        protected override void DisposeCore()
        {
            if (dataContext != null)
                dataContext.Dispose();
        }
    }
}
