using Autopilot.Models;
using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;




namespace Autopilot.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAdminServices _adminServices;
        private readonly IUserService _userServices;
        private readonly ApplicationDbContext _accountDb;

        public AdminController(IAdminServices adminServices, IUserService userServices)
        {
            _adminServices = adminServices;
            _userServices = userServices;
            _accountDb = new ApplicationDbContext(); 
        }

        //
        // GET: /Admin/
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult UsersHistory() {
            try
            {
                var users = _accountDb.Users.ToList().OrderByDescending(x=>x.RegisterationDate);
                return View(users);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ActionResult ManagePlans()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult GetAllPlan()
        {
            try
            {
                var plans = _userServices.GetAllSubscriptionPlan();
                return Json(plans, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                
                throw; 
            }
        }

        public ActionResult GetAPlan(string PlanId)
        {
            try
            {
                var plan = _userServices.GetASubscriptionPlan(PlanId);
                return Json(new{status=true,plan = plan}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult AddPlan(SubscriptionsPlan planObject) {
            try
            {
                var result = _adminServices.AddSubscriptionsPlan(planObject);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult UpdatePlan(SubscriptionsPlan planObject)
        {
            try
            {
                var result = _adminServices.UpdateSubscriptionsPlan(planObject);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult RemovePlan(int planId)
        {
            try
            {
                var result = _adminServices.RemoveSubscriptionPlan(planId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAccountsPerformance() {
            try
            {
                var stats = _adminServices.SocialAccountsStats();
                return Json(stats,JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ActionResult GetLastWeekAddedUsers() {
            try
            {
                var Users = _accountDb.Users.ToList();
                List<ApplicationUserVM> userlist = new List<ApplicationUserVM>();
                foreach (var item in Users)
                {
                    userlist.Add(new ApplicationUserVM { Email = item.Email, RegisterationDate = item.RegisterationDate });          
                }
                var stats = _adminServices.CalculateLastWeekAddedAccount(userlist);
                return Json(stats, JsonRequestBehavior.AllowGet);
                //return null;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

	}
}