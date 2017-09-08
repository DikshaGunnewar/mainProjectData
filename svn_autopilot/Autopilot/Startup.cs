using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using Autopilot.Models;
using Hangfire;
using Hangfire.SqlServer;
using System;
using Hangfire.Dashboard;
using System.Web;

[assembly: OwinStartupAttribute(typeof(Autopilot.Startup))]
namespace Autopilot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            //var options = new DashboardOptions
            //{
            //    AuthorizationFilters = new[]
            //{
            //    new LocalRequestsOnlyAuthorizationFilter()
            //}
            //};

            //app.UseHangfireDashboard("/hangfire");


            #region Reoccuring Algo Job
            //RecurringJob.AddOrUpdate<ServiceLayer.Interfaces.IAlgoService>(
            //    x=>x.BeginAlgo(),
            //    Cron.Daily);
            //BackgroundJob.Schedule<ServiceLayer.Interfaces.IPinterestServices>(
            //                         x => x.PostSchedulePin(0,0),
            //                         System.DateTime.Now.AddMinutes(1));
            #endregion
            app.MapSignalR();
            ConfigureAuth(app);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new Autopilot.App_Start.HangFireAuthorization() },
                AppPath = VirtualPathUtility.ToAbsolute("~/Admin/Dashboard"),
               
            });
            //createRolesandUsers();
                 
        }
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User 
            if (!roleManager.RoleExists("Admin"))
            {
                // first we create Admin rool 
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website				

                var user = new ApplicationUser();

                user.UserName = "admin";
                user.Email = "info@socializy.net";
                string userPWD = "Password@123";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }

            // creating Creating Principal role 
            if (!roleManager.RoleExists("User"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "User";
                roleManager.Create(role);

            }
        }
    }
}
