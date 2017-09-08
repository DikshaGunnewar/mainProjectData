using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RepositoryLayer.Infrastructure
{
    public class DbFactory:Disposable,IDbFactory
    {
        AutopilotContext dbContext;

        public AutopilotContext Init()
        {
            return dbContext ?? (dbContext = new AutopilotContext());
        }

        protected override void DisposeCore()
        {
            if (dbContext != null) { 
                dbContext.Dispose();
            }
        }

    }
}
