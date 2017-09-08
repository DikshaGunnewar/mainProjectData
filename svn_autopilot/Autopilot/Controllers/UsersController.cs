using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Autopilot.Models;
using System.Web.Hosting;
using EntitiesLayer.ViewModel;

namespace Autopilot.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITwitterServices _twitterService;
        private readonly IAlgoService _alogService;
        private readonly ApplicationDbContext _accountDb;

        public UsersController(IUserService userService, ITwitterServices twitterService, IAlgoService algoService)
        {
            _userService = userService;
            _twitterService = twitterService;
            _accountDb = new ApplicationDbContext();
            _alogService = algoService;
            //var _accountDb = DependencyResolver.Current.GetService<AccountController>();
        }

        //
        // GET: /User/
        public ActionResult Dashboard(string Message)
        {
            var userId = User.Identity.GetUserId();
            ViewBag.EmailConfirmed = _accountDb.Users.Where(x => x.Id == userId).FirstOrDefault().EmailConfirmed;
            ViewBag.Message = Message;
            var accounts = _userService.GetUsersAllAccounts(User.Identity.GetUserId());

            return View(accounts);
        }

        public ActionResult Profile()
        {
            try
            {
                var profile = _userService.GetUserprofile(User.Identity.GetUserId());
                var applicationUserDetail = _accountDb.Users.ToList().Where(x => x.Id == profile.UserId).FirstOrDefault();
                profile.Username = applicationUserDetail.UserName;
                profile.Email = applicationUserDetail.Email;
                profile.EmailVerified = applicationUserDetail.EmailConfirmed;
                return View(profile);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult UpdateBillAddress(EntitiesLayer.Entities.UserBillingAddress billAddress)
        {
            try
            {
                billAddress.UserId = User.Identity.GetUserId();
                var result = _userService.UpdateProfile(billAddress);
                return Json(result,JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult UpdateEmail(string Email) {
            try
            {
                if (_accountDb.Users.Where(x => x.Email == Email).FirstOrDefault() == null)
                {
                    var userId = User.Identity.GetUserId();
                    var user = _accountDb.Users.ToList().Where(x => x.Id == userId).FirstOrDefault();
                    user.Email = Email;
                    user.UserName = Email;
                    user.EmailConfirmed = false;
                    _accountDb.SaveChanges();
                    return Json(new { status = true }, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json(new { status = false, message = "Cannot update this email address,since this email address is already taken." }, JsonRequestBehavior.AllowGet);                
                }
                
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult SideMenu()
        {
            var accounts = _userService.GetUsersAllAccounts(User.Identity.GetUserId());
            return PartialView("~/Views/Shared/_SideMenu.cshtml", accounts);
        }

        public ActionResult UserManagement(int socialId, string Message)
        {
            var accounts = _userService.GetAccount(socialId);
            ViewBag.Message = Message;
            return PartialView("~/Views/Shared/_userManagement.cshtml", accounts);
        }

        public ActionResult AddUserInAccount(int socialId, string email)
        {
            try
            {
                var checkEmail = _accountDb.Users.Where(x => x.Email == email).FirstOrDefault();
                if (checkEmail != null)
                {
                    var result = _userService.AddUserToAccount(socialId, email, checkEmail.Id);
                    if (result == true)
                    {
                        return RedirectToAction("UserManagement", new { socialId = socialId, Message = "Successfully added user." });
                    }
                    else
                    {
                        return RedirectToAction("UserManagement", new { socialId = socialId, Message = "Unable to add user, please check email address." });
                    }
                }
                else
                {
                    return RedirectToAction("UserManagement", new { socialId = socialId, Message = "This email adddress is not register with our application." });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult RemoveUser(int userManagementId, int socialId)
        {
            try
            {
                var result = _userService.RemoveUser(userManagementId);
                if (result == true)
                {
                    return RedirectToAction("UserManagement", new { socialId = socialId, Message = "User removed successfully" });
                }
                else
                {
                    return RedirectToAction("UserManagement", new { socialId = socialId, Message = "Something went wrong." });
                }
            }
            catch (Exception)
            {

                return RedirectToAction("UserManagement", new { socialId = socialId, Message = "Something went wrong." });
            }
        }

        public ActionResult UpdateLanguages(string languageIds, int socialId)
        {
            var result = _userService.UpdateLanguage(languageIds, socialId);
            return Json(new { status = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddBlockTags(string tag, int socialId, bool IsBlocked)
        {
            try
            {
                var result = _userService.AddBlockTag(tag, socialId, IsBlocked);
                var tagList = _userService.GetAllTags(socialId);
                return Json(new { status = result, tagList = tagList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);


            }
        }

        public ActionResult GetTags(int socialId)
        {
            try
            {
                var tagList = _userService.GetAllTags(socialId);
                return Json(tagList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult RemoveTag(int tagId)
        {
            try
            {
                var result = _userService.RemoveTags(tagId);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Scheduler()
        {
            try
            {
                //var acc = _userService.GetAccount(socialId);
                //_userService.SchedulerMethod(acc);
                _alogService.BeginAlgo();
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ActionResult> CheckConversion(int socialId)
        {
            try
            {
                var acc = _userService.GetAccount(socialId);
                //_userService.CheckForConversion(acc);
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult GetConversionStats(int socialId)
        {
            try
            {
                var stats = _userService.CalculateConversion(socialId);
                return Json(stats, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult GetConversionCount(int socialId)
        {
            try
            {
                var conversionCount = _userService.CountConversion(socialId);
                return Json(conversionCount, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public ActionResult DeleteAccount(int socialId)
        {
            try
            {
                var result = _userService.DeleteAccount(socialId);
                if (result == true)
                {
                    return RedirectToAction("Dashboard", "Users", new { Message = "Account Delete successfully." });
                }
                else
                {
                    return RedirectToAction("Dashboard", "Users", new { Message = "Unable to delete this account." });

                }
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAllConversion(int socialId)
        {
            try
            {
                var conversion = _userService.GetConversion(socialId);
                return Json(conversion, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult GetTagPerformance(int socialId)
        {
            try
            {
                var performance = _userService.TagPerformance(socialId);
                return Json(performance, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult GetFollowersGrowth(int socialId)
        {
            try
            {
                var growth = _userService.GetFollowersGrowth(socialId);
                return Json(growth, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult UserSubscription(string Message)
        {

            //ViewBag.Message = Message;
            //var accounts = _userService.GetUsersAllAccounts(User.Identity.GetUserId());
            //return View(accounts);

            //SubscribeViewModel BCVM = new SubscribeViewModel();


            ViewBag.accounts = _userService.GetUsersAllAccounts(User.Identity.GetUserId()).ToList();
            var plans = _userService.GetAllSubscriptionPlan().Where(x=>x.IsTrail== false).ToList();
            return View(plans);

        }

        public ActionResult PaymentPlan(string PlanId,string socialIds)
        {

            //ViewBag.Message = Message;
            List<SocialMediaVM> accounts = new List<SocialMediaVM>();
            foreach (var item in socialIds.Split(','))
            {
                accounts.Add(_userService.GetAccount(Int32.Parse(item)));
            }

            ViewBag.accounts = accounts.ToList();
            ViewBag.socialIds = socialIds;
            //List<string> UserName = new List<string>();
 
            //foreach (var details in accounts)
            //{
            //    UserName.Add(details.UserName);
            //}
            //ViewBag.accounts_details = UserName.ToList();
            
            var plan = _userService.GetASubscriptionPlan(PlanId);
            return PartialView("~/Views/Payment/_PaymentPlan.cshtml", plan);
            //return View(accounts);
        }

        //Action method to upload a file
        [HttpPost]
        public ActionResult ImageUpload()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    string fileName = string.Empty;
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        fileName = Guid.NewGuid().ToString() + System.IO.Path.GetFileName(files[i].FileName);

                        // // Get the complete folder path and store the file inside it.  
                        string physicalPath = HostingEnvironment.MapPath("~/Images/SchedulePins/" + fileName);
                        files[i].SaveAs(physicalPath);

                        //calling web api to upload image
                        //fileName = Emp.FileuploadAPI(files[i]);
                    }
                    // Returns message that successfully uploaded  
                    return Json(new { status = true, fileName = fileName }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        
    }
}