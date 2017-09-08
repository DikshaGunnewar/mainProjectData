using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Autopilot.Controllers
{
    [Authorize]
    public class LinkedInController : Controller
    {
        private readonly ILinkedInServices _linkedinService;
        private readonly IUserService _userService;
        public LinkedInController(IUserService userService, ILinkedInServices linkedinService)
        {
            _linkedinService = linkedinService;
            _userService = userService;
        }
        //
        // GET: /SocialMedia/
        public ActionResult LinkedInAuth()
        {
            try
            {
                var URI = _linkedinService.Authorize();
                return new RedirectResult(URI, false /*permanent*/);
            }
            catch (Exception)
            {
                
                throw;
            }

        }

        public ActionResult AuthCallback(string code)
        {
           try 
	        {
                var tokens = _linkedinService.GetToken(code);
                var result = _linkedinService.SaveAccountDeatils(tokens, User.Identity.GetUserId(), User.Identity.Name);
                    if (result)
                    {
                        //ViewBag.Message = "Account added successfully";
                        return RedirectToAction("Dashboard", "Users", new { Message = "Account added successfully" });
                    }
                    else {
                        //ViewBag.Message = "Unable to add account";
                        return RedirectToAction("Dashboard", "Users", new { Message = "Unable to add account" });
            
                    }
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

	}
}