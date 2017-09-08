using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Autopilot.Models;
using System.Web.Security;
using Autopilot.Helper;
using System.IO;
using System.Web.Configuration;
using RepositoryLayer.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using Autopilot.IdentityConfig;
using ServiceLayer.Interfaces;

namespace Autopilot.Controllers
{

    public class AccountController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

 
       // 
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        ////
        //// POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
         {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (user.EmailConfirmed == false && user.PasswordHash == null) {
                        ViewBag.Message = "Your email verification process is still pending. A verification email is sent to "+model.Email;
                        string token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var mailContent = new SendMailViewModel() { Email = model.Email, Token = token, userId = user.Id, MailType = (int)MailType.EmailVerification, ChangePassword = true };

                        var mailBody = ReturnMailBody(mailContent);
                        EmailHelper.SendEmail(model.Email, mailBody, "Activate Account", null, true);
                        return View(model);
                    }
                    //await SignInAsync(user, model.RememberMe);
                    SignInStatus signInResult = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                    if (signInResult != SignInStatus.Success) {
                        ViewBag.Message = "Please enter correct details.";
                        return View(model);
                    }
                    Session["token"] = await UserManager.GenerateUserTokenAsync("login",user.Id);
                    Session["userId"] = user.Id;

                    if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        Session["role"] = "Admin";
                        //Storing values in Cookies
                        AddCookie("role", "Admin");
                       return RedirectToAction("Dashboard", "Admin");   
                    }
                    else if (UserManager.IsInRole(user.Id, "User"))
                    {
                        Session["role"] = "User";
                        AddCookie("role", "User");
                    }
                   
                    //ViewBag.Message = model.Message;
                    if (returnUrl != null)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                    else {
                        return RedirectToAction("Dashboard", "Users", model.Message);
                    
                    }
                }
                else
                {
                    ViewBag.Message = "Please enter correct details.";
                  
                    //ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            else 
            {
                ViewBag.Message = "Email/Password is required.";
       
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void AddCookie(string Name, string UserData)
        { 
            try 
	            {	        
		            FormsAuthenticationTicket objTicket = null;
                    HttpCookie objCookie = null;
                    objTicket = new FormsAuthenticationTicket(1, Name, System.DateTime.Now, DateTime.Now.AddDays(1), true, UserData);
                    objCookie = new HttpCookie("role");
                    objCookie.Value = FormsAuthentication.Encrypt(objTicket);
                    objCookie.Expires = DateTime.Now.AddYears(60);
                    Response.Cookies.Add(objCookie);
	            }
	            catch (Exception)
	            {
		
		            throw;
	            }       

        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.BusinessCategory = _userService.GetBusinessCategory();
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                var emailExist = UserManager.FindByEmail(model.Email);

                if (emailExist != null)
                {
                    ViewBag.Message = "Email already taken";
                }
                if (model.Password == null && emailExist == null)
                {
                    var user = new ApplicationUser()
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        BusinessCategory = model.BusinessCategory,
                        PasswordUpdateDate = DateTime.UtcNow,
                        
                    };

                    model.Password = "Socializy@123";
                    var message = "You have been succesfully registered with Socializy";
                    var result = await UserManager.CreateAsync(user);
                    string token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    SendMailViewModel mailContent = new SendMailViewModel() { Email = model.Email, Token = token, message = message, userId = user.Id, MailType = (int)MailType.EmailVerification ,ChangePassword = true};
                    var mailBody = ReturnMailBody(mailContent);
                    if (result.Succeeded)
                    {
                        await UserManager.AddToRoleAsync(user.Id, "User");
                        EmailHelper.SendEmail(model.Email, mailBody, "Activate Account", null, true);
                        //ResponseObj.IsSuccess = true;
                        //ResponseObj.SuccessMessage = "User is added successfully!";
                        return RedirectToAction("ProcessSuccess", new { pastUrl = "Register", userId = user.Id });
                    }
                    else
                    {

                        ViewBag.Message = "Email already taken";
                    }

                }
            }
            catch (Exception Ex)
            {
               ViewBag.Message = Ex.Message.ToString();
            }
            return View(model);
        }

        public string ReturnMailBody(SendMailViewModel model) {
            try
            {
                var logo = WebConfigurationManager.AppSettings["BaseURL"] + "Images/Socializy-logo-black.png";
                string body = string.Empty;
                if (model.MailType == 1) {
                    var URL = WebConfigurationManager.AppSettings["BaseURL"] + "/Account/VerifyEmail?Token=" + Server.UrlEncode(model.Token) + "&userId=" + Server.UrlEncode(model.userId) + "&changePassword=" + model.ChangePassword;

                    using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/SendCredentialsEmail.html")))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("[USERNAMEE]", model.Email);
                    body = body.Replace("[MESSAGE]", model.message);
                    body = body.Replace("[LoginURL]", URL);
                    body = body.Replace("[USERNAME]", model.Email);
                    body = body.Replace("[LOGO]", logo);
                }
                else if (model.MailType == 0) {
                    var URL = WebConfigurationManager.AppSettings["BaseURL"] + "/Account/ChangePassword?token=" + Server.UrlEncode(model.Token) + "&userId=" + Server.UrlEncode(model.userId);

                    
                    using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/ForgetPasswordEmail.html")))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("[USERNAME]", model.Email);
                    body = body.Replace("[Link]", URL);
                    body = body.Replace("[LOGO]", logo);
                }
                
              
                return body;

            }
            catch (Exception)
            {
                
                throw;
            }
         }

        public async Task<ActionResult> EmailVerification() {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());

                var Token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var mailBody = ReturnMailBody(new SendMailViewModel { Email = user.Email,MailType = (int)MailType.EmailVerification,message="To verify your email click on the link below",Token = Token,userId= user.Id,ChangePassword=false});
                EmailHelper.SendEmail(user.Email, mailBody, "Accounts credentials", null, true);
                ViewBag.message = "Email has been send successfully";
                return Json(true,JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        [AllowAnonymous]
        public ActionResult ProcessSuccess(string pastUrl, string userId)
        {
            SendMailViewModel model = new SendMailViewModel();
            model.userId = userId;
            var user = UserManager.FindById(userId);
            if (pastUrl == "Register")
            {
                ViewBag.Content = "You are successfully Registered! Please confirm the mail sent to your Email-Id!!!";
                model.MailType = (int)MailType.EmailVerification;
            }
            else if(pastUrl == "ForgotPassword" )
            {
                if (user.EmailConfirmed == false)
                {
                    ViewBag.Content = "Your account is not activated, activate it by going through the Email";
                    model.MailType = (int)MailType.ResetPassword;
                }
                else 
                {
                    ViewBag.Content = "Follow the instruction in email to reset your password";
                    model.MailType = (int)MailType.ResetPassword;
                }
              
            }

            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ProcessSuccess(SendMailViewModel model)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(model.userId);
                model.Email = user.Email;
                model.ChangePassword = true;
                if (model.MailType == (int)MailType.EmailVerification) 
                {
                    model.Token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var mailBody = ReturnMailBody(model);
                    EmailHelper.SendEmail(model.Email, mailBody, "Accounts credentials", null, true);
                    ViewBag.message = "Email has been send successfully";
                    return View(model);             
                
                }
                else if (model.MailType == (int)MailType.ResetPassword) 
                {
                    model.Token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var mailBody = ReturnMailBody(model);
                    EmailHelper.SendEmail(model.Email, mailBody, "Reset account password", null, true);
                    ViewBag.message = "Email has been send successfully";
                    return View(model);             
                }
              
            }
            catch (Exception)
            {

                throw;
            }
            return null;
         }

        // GET: /Account/ForgetPassword
        [AllowAnonymous]
        public ActionResult ChangePassword(string userId,string token,bool firstLogin)
        {
            ManageUserViewModel model = new ManageUserViewModel()
            {
                UserId = userId,
                Token = token,
                FirstLogin = firstLogin
            };
            if (UserManager.FindById(userId).EmailConfirmed == true)
            {
                ViewBag.Title = "Change password";
            }
            else {
                ViewBag.Title = "Set password";
            }

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ChangePassword(ManageUserViewModel userModel)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var result = await UserManager.ResetPasswordAsync(userModel.UserId, userModel.Token, userModel.NewPassword);
                    if (result.Succeeded)
                    {
                        var user = UserManager.FindById(userModel.UserId);
                        user.PasswordUpdateDate = DateTime.UtcNow;
                        await UserManager.UpdateAsync(user);
                        LoginViewModel model = new LoginViewModel()
                        {
                            Email = UserManager.FindById(userModel.UserId).Email,
                            Password = userModel.NewPassword,
                            Message = "Password changed successfully"
                        };
                        if (userModel.FirstLogin == true)
                        {
                            var planDetails = _userService.GetAllSubscriptionPlan().Where(x => x.IsDeleted == false && x.IsTrail == true).FirstOrDefault();
                            if (planDetails != null) {
                                var addDays = double.Parse(planDetails.TrailDays);
                                DateTime planExpiresDate = DateTime.UtcNow.AddDays(addDays);
                                _userService.ApplyUserSubscription(new EntitiesLayer.Entities.UserAccountSubscription {  userId = user.Id,ExpiresOn = planExpiresDate, IsTrail = true,PlanId= planDetails.Id });                            
                            }
                        }
                        await SignInAsync(user, false);
                        //await SignInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                        Session["token"] = await UserManager.GenerateUserTokenAsync("login", user.Id);
                        Session["userId"] = user.Id;

                        if (UserManager.IsInRole(user.Id, "Admin"))
                        {
                            Session["role"] = "Admin";
                            return RedirectToAction("Login", model);
                        }
                        else if (UserManager.IsInRole(user.Id, "User"))
                        {
                            Session["role"] = "User";
                            return RedirectToAction("Dashboard", "Users", new { Message = model.Message });
                        }
                     
                    }
                    else {
                        ViewBag.Message = "Session is expired, please again generate an email for forgot password";
                    }
                }
                catch (Exception)
                {
                
                
                }
            }
            else
        	{
                ViewBag.Message = "Password didn't match";
	        }

            return View(userModel);
        }

        public async Task<ActionResult> VerifyEmail(string Token, string userId,bool changePassword)
        {
            try
            {
                var result = await UserManager.ConfirmEmailAsync(userId, Token);


                if (result.Succeeded && changePassword == true)
                {
                    var resetToken = UserManager.GeneratePasswordResetToken(userId);

                    return RedirectToAction("ChangePassword", new { userId = userId, token = resetToken });
                }
                else if (changePassword == false)
                {
                    return RedirectToAction("Dashboard", "Users");
                }

                else{
                    return Content("Invalid Token, please try again");
                }

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #region "Forget Password"
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model) 
        {
            try
            {
                var UserModel = UserManager.FindByEmail(model.Email);
                
                if (UserModel != null)
                {
                    SendMailViewModel mailContent;
                    var mailBody = String.Empty; 
                    
                    if (UserModel.EmailConfirmed == false)
                    {

                        string token = await UserManager.GenerateEmailConfirmationTokenAsync(UserModel.Id);
                        mailContent = new SendMailViewModel() { Email = model.Email, Token = token, message = "Activate your and set new password", userId = UserModel.Id, MailType = (int)MailType.EmailVerification,ChangePassword = true };
                        mailBody = ReturnMailBody(mailContent);
                        EmailHelper.SendEmail(model.Email, mailBody, "Activate Account", null, true);
                     
                    }
                    else
                    {
                        var Token = UserManager.GeneratePasswordResetToken(UserModel.Id);
                        mailContent = new SendMailViewModel() { Email = model.Email, Token = Token, userId = UserModel.Id, MailType = (int)MailType.ResetPassword,ChangePassword= true };
                        mailBody = ReturnMailBody(mailContent);
                        EmailHelper.SendEmail(model.Email, mailBody, "Reset account password", null, true);
                     
                    }
                     return RedirectToAction("ProcessSuccess", new { pastUrl = "ForgotPassword", userId = UserModel.Id });
                    
                }
                else
                {
                    ViewBag.Message = "No user is registered with this email, please try again.";
                    return View(model);

                }
            }
            catch (Exception)
            {
                
                throw;
            }

            
           
        }


        #endregion "Forget Password"

        
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            if (Request.Cookies["role"] != null)
            {
                FormsAuthentication.SignOut();
                Response.Cookies["role"].Expires = DateTime.Now.AddDays(-1);
                //Request.Cookies.Remove("role");
            }
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }
        //check remotely whether email exist or not
        public async Task<ActionResult> CheckEmailExist(string Email)
        {
            bool chk;
            if (await UserManager.FindByEmailAsync(Email) != null)
            {
                chk = false;
            }
            else
            {
                chk = true;
            }

            return Json(chk, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}