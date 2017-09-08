using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EntitiesLayer.ViewModel;
using Microsoft.AspNet.Identity;
using ServiceLayer.Interfaces;
using Autopilot.Models;

namespace Autopilot.Controllers
{
     [Authorize]
    public class SubscriptionController : Controller
    {

        private readonly IUserService _userService;
        private readonly ITwitterServices _twitterService;
        private readonly IAlgoService _alogService;
        private readonly ApplicationDbContext _accountDb;


        public SubscriptionController(IUserService userService, ITwitterServices twitterService, IAlgoService algoService)
        {
            _userService = userService;
            _twitterService = twitterService;
            _accountDb = new ApplicationDbContext();
            _alogService = algoService;
            //var _accountDb = DependencyResolver.Current.GetService<AccountController>();
        }


        public ActionResult Index()
        {
            return View();
        }
        //[AllowAnonymous]
        //public ActionResult UserSubscription(string Message)
        //{

        //    //ViewBag.Message = Message;
        //    //var accounts = _userService.GetUsersAllAccounts(User.Identity.GetUserId());
        //    //return View(accounts);

        //    //SubscribeViewModel BCVM = new SubscribeViewModel();
        //   // BCVM.social = _userService.GetUsersAllAccounts(User.Identity.GetUserId()).ToList();
        //    //BCVM.subscribe = _userService.GetAllSubscriptionPlan().ToList();
        //    return View(BCVM);

        //}



        // [AllowAnonymous]
        //public ActionResult PaymentPlan(int PlanId,string Message)
        //{

        //    ViewBag.Message = Message;
        //    var accounts = _userService.GetASubscriptionPlan(PlanId);
        //    return View(accounts);
        //}
        //[AllowAnonymous]
        //public ActionResult PaymentPlans()
        //{
        //    return View();
        //}
    }
}