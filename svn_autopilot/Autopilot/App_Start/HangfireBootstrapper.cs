using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Autopilot.App_Start
{
    public class HangfireBootstrapper : IRegisteredObject
    {
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object _lockObject = new object();
        private bool _started;

        private BackgroundJobServer _backgroundJobServer;

        private HangfireBootstrapper()
        {
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;

                HostingEnvironment.RegisterObject(this);
                #region Hangfire dependencies
                var builder = new ContainerBuilder();
                builder.RegisterType<RepositoryLayer.Infrastructure.DbFactory>().As<RepositoryLayer.Infrastructure.IDbFactory>().InstancePerBackgroundJob();
                builder.RegisterType<RepositoryLayer.Infrastructure.UnitOfWork>().As<RepositoryLayer.Infrastructure.IUnitOfWork>().InstancePerBackgroundJob();
                builder.RegisterGeneric(typeof(RepositoryLayer.Repositories.Repository<>)).As(typeof(RepositoryLayer.Repositories.IRepository<>)).InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.UserService>().As<ServiceLayer.Interfaces.IUserService>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.PinterestServices>().As<ServiceLayer.Interfaces.IPinterestServices>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.TwitterServices>().As<ServiceLayer.Interfaces.ITwitterServices>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.LinkedInServices>().As<ServiceLayer.Interfaces.ILinkedInServices>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.InstagramServices>().As<ServiceLayer.Interfaces.IInstagramServices>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.SpotifyServices>().As<ServiceLayer.Interfaces.ISpotifyServices>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.AlgoService>().As<ServiceLayer.Interfaces.IAlgoService>().InstancePerBackgroundJob();
                builder.RegisterType<ServiceLayer.Services.DeezerServices>().As<ServiceLayer.Interfaces.IDeezerServices>().InstancePerBackgroundJob();
                var container = builder.Build();
                #endregion
                GlobalConfiguration.Configuration.UseAutofacActivator(container);
                JobActivator.Current = new AutofacJobActivator(container);
                GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 1 });
                GlobalConfiguration.Configuration.UseSqlServerStorage("Autopilot",
                                            new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(15) })
                                                .UseFilter(new Autopilot.App_Start.LogFailureAttribute()); 
                //GlobalConfiguration.Configuration
                //    .UseSqlServerStorage("connection string");
                // Specify other options here

                _backgroundJobServer = new BackgroundJobServer();
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_backgroundJobServer != null)
                {
                    _backgroundJobServer.Dispose();
                }

                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }
}