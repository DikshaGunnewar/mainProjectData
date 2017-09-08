using Autofac;
using Autofac.Integration.WebApi;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repository;
using ServiceLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Autopilot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Setup the Container Builder
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();

            // Register dependencies in controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerRequest();

            builder.RegisterType<RepositoryLayer.Repository.AutopilotRepository>().As<IUserRepository>().InstancePerRequest();
            builder.RegisterGeneric(typeof(RepositoryLayer.Infrastructure.RepositoryBase<>)).As(typeof(IRepositoryBase<>)).InstancePerRequest();
            //builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();

            // Register your repositories all at once using assembly scanning
            builder.RegisterAssemblyTypes(typeof(RepositoryLayer.Repository.AutopilotRepository).Assembly).Where(t => t.Name.EndsWith("Repository")).AsImplementedInterfaces().InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(RegisterService).Assembly).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces().InstancePerRequest();

            // Register your Web API controllers all at once using assembly scanning
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());


            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            // OPTIONAL: Register the Autofac model binder provider.
            builder.RegisterWebApiModelBinderProvider();

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}
