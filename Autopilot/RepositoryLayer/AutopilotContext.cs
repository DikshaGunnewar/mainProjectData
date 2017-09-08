using EntitiesLayer;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer
{
    public class AutopilotContext : IdentityDbContext
    {
        public AutopilotContext() : base("name=MyConnectionString")
        {

        }
        public DbSet<SocialMedia> SocialMedias { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public virtual void Commit()
        {
            base.SaveChanges();
        }
    }
}
