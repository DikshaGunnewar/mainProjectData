using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesLayer.Entities;

namespace RepositoryLayer
{
    public class Context:DbContext
    {
        public Context()
            : base("Autopilot")
        {
           // Database.SetInitializer<Context>(null);

        }
        #region Entities

        public IDbSet<BusinessCategory> BusinessCategory { get; set; }
        #endregion
        public virtual void Commit()
        {
            base.SaveChanges();
        }
    }
}
