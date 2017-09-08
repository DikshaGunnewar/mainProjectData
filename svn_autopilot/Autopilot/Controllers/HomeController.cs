using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EntitiesLayer.Entities;



namespace Autopilot.Controllers{
    //[Authorize]
    public class HomeController : Controller
    {
        public HomeController( )
        {
           
        }
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

         [AllowAnonymous]
        public ActionResult Privacy()
        {
            return View();
        }

 
         [AllowAnonymous]
        public ActionResult TermsOfServices()
        {
            return View();
        }
         [AllowAnonymous]
         public ActionResult Error() {
             return View();
         }

         
    }
}