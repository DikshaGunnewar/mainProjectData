using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceLayer.Interfaces;
using Microsoft.AspNet.Identity;
using EntitiesLayer.ViewModel;
using System.Threading.Tasks;
using EntitiesLayer.Entities;


namespace Autopilot.Controllers
{
    [Authorize]
    public class InstagramController : Controller
    {
        private readonly IInstagramServices _instagramService;
        private readonly IUserService _userService;
        public InstagramController(IUserService userService, IInstagramServices instagramService)
        {
            _instagramService = instagramService;
            _userService = userService;
        }
        /// <summary>
        /// Method for Authenticate and get URL path
        /// </summary>
        /// <returns></returns>
        public ActionResult InstaAuth()
        {
            try
            {
                var URI = _instagramService.Authorize();
                return new RedirectResult(URI, false /*permanent*/);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Redirect to Authenticate
        /// </summary>
        /// <returns></returns>
        public ActionResult Reconnect()
        {
            try
            {
                return RedirectToAction("InstaAuth");
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method for save account details
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult AuthCallback(string code)
        {
            try
            {
                var tokens = _instagramService.GetToken(code);
                var response = _instagramService.SaveAccountDeatils(tokens, User.Identity.GetUserId(), User.Identity.Name);
                return RedirectToAction("Dashboard", "Users", new { Message = response });
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method for to get account details and display languages 
        /// </summary>
        /// <param name="socialId"></param>
        /// <returns></returns>
        public ActionResult Settings(int socialId)
        {
            try
            {
                var acc = _userService.GetAccount(socialId);
                //_instagramService.scheduleAlgo(acc);
                ViewBag.Languages = _userService.GetLanguages();
                return View(acc);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method to get search user for instagram 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="socialId"></param>
        /// <returns></returns>
        public ActionResult SearchUser(string query, int socialId)
        {
            try
            {
                var Token = _userService.GetTokenForUser(socialId);
                var users = _instagramService.SearchUser(query, Token);
                return Json(users, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method for Search location using LAT/Long 
        /// </summary>
        /// <param name="socialId"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public ActionResult AddressLocation(int socialId, string location)
        {
            try
            {
                var Token = _userService.GetTokenForUser(socialId);
                var users = _userService.AddressToCoordinates(location);
                var locationAddress = _instagramService.Location(Token, users.latitude, users.longitude);
                return Json(locationAddress, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method to get list of all super target user
        /// </summary>
        /// <param name="socialId"></param>
        /// <returns></returns>
        public ActionResult GetAllTargetUsers(int socialId)
        {
            try
            {
                var targetUsers = _instagramService.GetAllTargetUser(socialId);
                return Json(targetUsers, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method for add supertarget user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult AddBlockSuperTargetUser(SuperTargetUserVM user)
        {
            try
            {
                var result = _instagramService.AddBlockSuperTargetUser(user);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Method to remove super target user
        /// </summary>
        /// <param name="TargetUserId"></param>
        /// <returns></returns>
        public ActionResult RemoveTargetUser(int TargetUserId)
        {
            try
            {
                var result = _instagramService.RemoveTargetUser(TargetUserId);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Method return searchtag list using tagname and socialid
        /// </summary>
        /// <param name="socialId"></param>
        /// <param name="tagname"></param>
        /// <returns></returns>
        public ActionResult SearchTag(int socialId, string tagname)
        {
            try
            {
                var Token = _userService.GetTokenForUser(socialId);
                var users = _instagramService.SearchTags(Token, tagname);
                return Json(users, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method to return recent activities of user
        /// </summary>
        /// <param name="socialId"></param>
        /// <returns></returns>
        public ActionResult RecentActivity(int socialId)
        
        {
            try
            {
                var accessDetails = _userService.GetAccount(socialId).AccessDetails;
                var activities = _instagramService.RecentActivities(socialId, accessDetails);
                return Json(activities, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method for add location for individual tag
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public ActionResult AddLocation(int tagId, string location)
        {
            try
            {
                var result = _instagramService.AddLocationToTag(tagId, location);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Method for remove location to individual tag
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public ActionResult RemoveLocation(int tagId)
        {
            try
            {
                var result = _instagramService.RemoveLocation(tagId);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}