using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Autofac.Core;
using System.Reflection;
using EntitiesLayer;
using System.Web.Mvc;
using System.Data.Entity;
using Autofac.Integration.Mvc;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;
using Autopilot.IdentityConfig;
using Microsoft.AspNet.Identity;
using Autopilot.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Ajax.Utilities;
using RepositoryLayer;

namespace Autopilot.App_Start
{
    public class AutofacConfig
    {

        public static void ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            // Register dependencies in controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.RegisterType<AutopilotContext>().As<DbContext>().InstancePerRequest();

            builder.RegisterType<DbFactory>().As<IDbFactory>().InstancePerRequest();
            builder.RegisterType<ApplicationUserManager>().As<UserManager<ApplicationUser>>().InstancePerRequest();
            builder.RegisterType<ApplicationSignInManager>().As<SignInManager<ApplicationUser,string>>().InstancePerRequest();

            
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerRequest();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerRequest();
            builder.RegisterType<TwitterServices>().As<ITwitterServices>().InstancePerRequest();
            builder.RegisterType<DeezerServices>().As<IDeezerServices>().InstancePerRequest();
            builder.RegisterType<PinterestServices>().As<IPinterestServices>().InstancePerRequest();
            builder.RegisterType<LinkedInServices>().As<ILinkedInServices>().InstancePerRequest();
            builder.RegisterType<InstagramServices>().As<IInstagramServices>().InstancePerRequest();
            builder.RegisterType<SpotifyServices>().As<ISpotifyServices>().InstancePerRequest();
            builder.RegisterType<AlgoService>().As<IAlgoService>().InstancePerRequest();
            builder.RegisterType<AdminServices>().As<IAdminServices>().InstancePerRequest();
          

            //// Register dependencies in filter attributes
            //builder.RegisterFilterProvider();

            //// Register dependencies in custom views
            //builder.RegisterSource(new ViewRegistrationSource());

            var container = builder.Build();
            // Set MVC DI resolver to use our Autofac container
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

           

            //config.DependencyResolver = resolver;
            //DependencyResolver.SetResolver(resolver);
            //// Register dependencies in filter attributes
            //builder.RegisterFilterProvider();

            //// Register dependencies in custom views
            //builder.RegisterSource(new ViewRegistrationSource());

            //// Register our Data dependencies
            //builder.RegisterModule(new DataModule("MVCWithAutofacDB"));

            //var container = builder.Build();

            //// Set MVC DI resolver to use our Autofac container
            //DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}