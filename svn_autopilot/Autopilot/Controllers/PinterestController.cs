using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EntitiesLayer.ViewModel;
using EntitiesLayer.Entities;

namespace Autopilot.Controllers
{
    [Authorize]
    public class PinterestController : Controller
    {
        private readonly IPinterestServices _pinterestService;
        private readonly IUserService _userService;
        public PinterestController(IUserService userService, IPinterestServices pinterestService)
        {
            _pinterestService = pinterestService;
            _userService = userService;
        }
        //
        // GET: /SocialMedia/
        public ActionResult PinterestAuth()
        {
            var URI = _pinterestService.Authorize();
           return new RedirectResult(URI, false /*permanent*/);

        }

        public ActionResult Reconnect()
        {
            try
            {
                return RedirectToAction("PinterestAuth");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult AuthCallBackPin(string code)
        {
            try
            {
                var tokens = _pinterestService.GetPinToken(code);
                var response = _pinterestService.SaveAccountDeatils(tokens, User.Identity.GetUserId(), User.Identity.Name);
                return RedirectToAction("Dashboard", "Users", new { Message = response });

                //if (result)
                //{
                //    //ViewBag.Message = "Account added successfully";
                //    return RedirectToAction("Dashboard", "Users", new { Message = "Account added successfully" });
                //}
                //else
                //{
                //    //ViewBag.Message = "Unable to add account";
                //    return RedirectToAction("Dashboard", "Users", new { Message = "Unable to add account" });

                //}
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        public ActionResult Settings(int socialId)
        {
            var acc = _userService.GetAccount(socialId);
            ViewBag.Languages = _userService.GetLanguages();
            return View(acc);
        }

        public ActionResult UserProfile(int socialId) {

            try
            {
                var acc = _userService.GetAccount(socialId);
               var profile = _pinterestService.GetUserprofile(acc.AccessDetails);
               return Json(profile, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ActionResult GetSchedulePin(int socialId) {
            try
            {
                var scheduleTask = _pinterestService.GetSchedulePin(socialId);
                return Json(new { status = true, task = scheduleTask },JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(new { status = false});
            }
        }

        public ActionResult GetBoardList(int socialId)
        {
            try
            {
                var acc = _userService.GetAccount(socialId);
                var boards = _pinterestService.GetAllBoard(acc.AccessDetails);
                return Json(new { status = true, boards = boards }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(new { status = false});
            }
        }

        public ActionResult SaveSchedulePin(PinterestScheduledPin PinInfo){
            try
            {
                PinInfo.ScheduleDate = PinInfo.ScheduleDate.ToUniversalTime();
                var result = _pinterestService.SaveSchedulePin(PinInfo);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveSchedulePin(int SchedulePinId)
        {
            try
            {
                var result = _pinterestService.RemoveSchedulePin(SchedulePinId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetASchedulePin(int SchedulePinId) {
            try
            {
               var pinInfo = _pinterestService.GetASchedulePin(SchedulePinId);
               return Json(new { status = true, pinInfo = pinInfo }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return Json(new { status = false  }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetFollowingBoardToConnect(int socialId, string myBoardId)
        {
            try
            {
                var acc = _userService.GetAccount(socialId);
                var followingBoard = _pinterestService.GetUsersFollowingBoard(acc.AccessDetails);
                var list = _pinterestService.CheckBoardConnection(socialId, myBoardId,followingBoard);
                return Json(new { status = true, list = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false}, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult SaveBoardMapping(int socialId, string myBoardId, string followingBoardIds)
        {
            try
            {
                //var acc = _userService.GetAccount(socialId);
                //var followingBoard = _pinterestService.GetUsersFollowingBoard(acc.AccessDetails);
                var result = _pinterestService.SaveBoardMapping(socialId, myBoardId, followingBoardIds);
                return Json(new { status = result}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false}, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult RecentActivity(int socialId) {
            try
            {
                var acc = _userService.GetAccount(socialId);
                var activities = _pinterestService.RecentActivities(socialId, acc.AccessDetails);
                return Json(new { status = true, activity = activities }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false}, JsonRequestBehavior.AllowGet);

            }
        }
        
    }
}