using Hangfire.Dashboard;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Autopilot.App_Start
{

        public class HangFireAuthorization : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                // In case you need an OWIN context, use the next line, `OwinContext` class
                // is the part of the `Microsoft.Owin` package.
                var owinContext = new OwinContext(context.GetOwinEnvironment());

                // Allow all authenticated users to see the Dashboard (potentially dangerous).
                bool boolAuthorizeCurrentUserToAccessHangFireDashboard = false;

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (HttpContext.Current.User.IsInRole("Admin"))
                        boolAuthorizeCurrentUserToAccessHangFireDashboard = true;
                }

                return boolAuthorizeCurrentUserToAccessHangFireDashboard;
                //if(owinContext.Authentication.User.Identity.IsAuthenticated){
                //    HttpCookie authCookie = HttpContext.Current.Request.Cookies["role"];
                //    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                //    if (authTicket.Name == "role" && authTicket.UserData == "Admin")
                //    {
                //        return true;
                //    }
                //    else {
                //        return false;
                //    }
                //}
                //else{
                //    return false; 
                //}

            }
        }

}
