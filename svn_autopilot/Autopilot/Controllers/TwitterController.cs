using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using EntitiesLayer.ViewModel;


namespace Autopilot.Controllers
{
    [Authorize]
    public class TwitterController : Controller
    {
        private readonly ITwitterServices _twitterService;
        private readonly IUserService _userService;

        public TwitterController(ITwitterServices twitterService, IUserService userService)
        {
            _twitterService = twitterService;
            _userService = userService;
        }
        //
        // GET: /SocialMedia/
        public ActionResult TwitterAuth()
        {
           var URI = _twitterService.Authorize();
           return new RedirectResult(URI, false /*permanent*/);

        }

        public ActionResult Reconnect() {
            try
            {
                return RedirectToAction("TwitterAuth");
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ActionResult TwitterAuthCallback(string oauth_token, string oauth_verifier)
        {
            var tokens = _twitterService.GetTokensOAuth(oauth_token, oauth_verifier);
            var response = _twitterService.SaveAccountDeatils(tokens, User.Identity.GetUserId(), User.Identity.Name);
            return RedirectToAction("Dashboard", "Users", new { Message = response });
            
            //if (result)
            //{
            //    //ViewBag.Message = "Account added successfully";
            //    return RedirectToAction("Dashboard", "Users", new { Message = "Account added successfully" });
            //}
            //else {
            //    //ViewBag.Message = "Unable to add account";
            //    return RedirectToAction("Dashboard", "Users", new { Message = "Unable to add account" });
            
            //}

           
            //return null;
        }

        public async Task<ActionResult> Settings(int socialId,int tab)
        {
            var acc= _userService.GetAccount(socialId);
            ViewBag.Languages =_userService.GetLanguages();
            ViewBag.tabIndex= tab;

            return View(acc);
        }

        public ActionResult GetAllTargetUsers(int socialId)
        {
            try
            {
                var targetUsers = _twitterService.GetAllTargetUser(socialId);
                return Json(targetUsers,JsonRequestBehavior.AllowGet);
             }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult SearchUser(string query, int socialId) 
        
        {
            try
            {
                var _accessToken = _userService.GetTokenForUser(socialId);
                var users = _twitterService.SearchUser(query, _accessToken).ToList();
                return Json(users,JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                
                throw;
            }

        }

        public ActionResult AddBlockSuperTargetUser(SuperTargetUserVM user) {
            try
            {
               var result = _twitterService.AddBlockSuperTargetUser(user);
               return Json(new { status = result  }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveTargetUser(int TargetUserId)
        {
            try
            {
                var result = _twitterService.RemoveTargetUser(TargetUserId);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
    
        public ActionResult RecentActivity(int socialId)
        {
            try
            {
                var accessDetails= _userService.GetAccount(socialId).AccessDetails;
                var activities = _twitterService.RecentActivities(socialId, accessDetails);
                return Json(new { status = true, result = activities }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddLocation(int  tagId,string location)
        {
            try
            {
                var result = _twitterService.AddLocationToTag(tagId, location);
                return Json(new { status = result}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveLocation(int tagId) {
            try
            {
                var result = _twitterService.RemoveLocation(tagId );
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);

            }
        }
    }
}