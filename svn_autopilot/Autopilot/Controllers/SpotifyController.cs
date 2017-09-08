using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EntitiesLayer.ViewModel;

namespace Autopilot.Controllers
{
    public class SpotifyController : Controller
    {
        private readonly ISpotifyServices _spotifyService;
        private readonly IUserService _userService;
        public SpotifyController(IUserService userService, ISpotifyServices spotifyService)
        {
            _spotifyService = spotifyService;
            _userService = userService;
        }
        //
        // GET: /SocialMedia/
        public ActionResult SpotifyAuth()
        {
            var URI = _spotifyService.Authorize();
           return new RedirectResult(URI, false /*permanent*/);

        }
        public ActionResult Reconnect()
        {
            try
            {
                return RedirectToAction("SpotifyAuth");
            }
            catch (Exception)
            {

                throw;
            }
        }
        //
        // GET: /Call back method for auth/
        public ActionResult AuthCallback(string code)
        {
            try
            {
                var tokens = _spotifyService.GetTokensOAuth(code);
                var response = _spotifyService.SaveAccountDeatils(tokens, User.Identity.GetUserId(), User.Identity.Name);
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

        public ActionResult Settings(int socialId, int tab)
        {
            try
            {
                var acc = _userService.GetAccount(socialId);
               // _spotifyService.SpotifyAlgoForUser(acc) ;
                ViewBag.Languages = _userService.GetLanguages();
                ViewBag.tabIndex = tab;                
                return View(acc);
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        public ActionResult Search(string query, int socialId,string type)
        {
            try
            {
                var Token = _userService.GetTokenForUser(socialId);
                var result = _spotifyService.Search(socialId, query, Token, type);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult GetAllTargetUsers(int socialId)
        {
            try
            {
                var targetUsers = _spotifyService.GetAllTargetUser(socialId);
                return Json(targetUsers, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult GetTargetPlaylists(int socialId)
        {
            try
            {
                var targetPlaylist = _spotifyService.GetAllTargetPlaylist(socialId);
                return Json(targetPlaylist, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult GetUsersPlaylists(int socialId)
        {
            try
            {
                var acc = _userService.GetAccount(socialId);
                var playlists = _spotifyService.GetUserPlaylists(socialId, acc.SMId, acc.AccessDetails);
                return Json(playlists, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult SearchUser(string query, int socialId)
        {
            //try
            //{
            //    var _accessToken = _userService.GetTokenForUser(socialId);
            //    var users = _twitterService.SearchUser(query, _accessToken);
            //    return Json(users, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            return null;
        }

        public ActionResult AddBlockSuperTargetUser(SuperTargetUserVM user)
        {
            try
            {
                var result = _spotifyService.AddBlockSuperTargetUser(user);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult AddTargetPlaylist(TargetPlaylistVM playlist)
        {
            try
            {
                var result = _spotifyService.AddTargetPlaylist(playlist);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult RemovePlaylist(int TargetPlaylistId)
        {
            try
            {
                var result = _spotifyService.RemoveTargetPlaylist(TargetPlaylistId);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
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
                var result = _spotifyService.RemoveTargetUser(TargetUserId);
                return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //public ActionResult RecentActivity(int socialId)
        //{
        //    try
        //    {
        //        var accessDetails = _userService.GetAccount(socialId).AccessDetails;
        //        var activities = _spotifyService.RecentActivities(socialId, accessDetails);
        //        return Json(activities, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

        public ActionResult AddLocation(int tagId, string location)
        {
            //try
            //{
            //    var result = _twitterService.AddLocationToTag(tagId, location);
            //    return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception)
            //{
            //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            //}
            return null;

        }

        public ActionResult RemoveLocation(int tagId)
        {
            //try
            //{
            //    var result = _twitterService.RemoveLocation(tagId);
            //    return Json(new { status = result }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception)
            //{
            //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            //}
            return null;

        }

        public ActionResult GetRecentActivities(int socialId,int offset)
        {
            try
            {
                var accessToken = _userService.GetAccount(socialId).AccessDetails;
                var result = _spotifyService.RecentActivities(socialId, accessToken, offset);
                return Json(new { status = true,result= result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult MusicBoard(int socialId)
        {
            var acc = _userService.GetAccount(socialId);
            ViewBag.MyPlaylist = _spotifyService.GetUserPlaylists(socialId, acc.SMId, acc.AccessDetails).items.Where(x => x.owner.id == acc.SMId).ToList();
            return PartialView("~/Views/Spotify/_musicBoard.cshtml",acc);
        }

        public ActionResult GetSuggestedTrack(int socialId) {

            try
            {
            var tracks = _spotifyService.GetSuggestedTracks(socialId);
            return Json(tracks, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ActionResult IgnoreSuggestedTracks(int socialId,string trackIds) {
            try
            {
                if (trackIds != string.Empty)
                {
                    var result = _spotifyService.IgnoreTrack(socialId, trackIds);
                    return Json(new { status=result, message="Success"}, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json(new { status = false, message = "No item is selected." }, JsonRequestBehavior.AllowGet);                
                }
               
            }
            catch (Exception)
            {

                return Json(new { status = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);       
            }
        }

        public ActionResult AddTracks(int socialId, string trackIds)
        {
            try
            {
                if (trackIds != string.Empty)
                {
                    var acc = _userService.GetAccount(socialId);
                    var tracksIds = trackIds.Split(',');
                    if (tracksIds.Length > 50) {
                        int limit = 0;
                        while ((limit * 50 )< tracksIds.Length) {
                            var count = tracksIds.Length - (limit * 50) > 50 ? 50 : tracksIds.Length - (limit * 50); 
                            trackIds = string.Join(",", tracksIds, limit * 50,count);
                            var result = _spotifyService.SaveTrack(socialId, 
                                trackIds, acc.AccessDetails);
                            if (result == false)
                            {
                                return Json(new { status = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                limit++;
                            }
                        }
                        return Json(new { status = true, message = "Success" }, JsonRequestBehavior.AllowGet);
                        
                    }
                    else{
                        var result = _spotifyService.SaveTrack(socialId, trackIds, acc.AccessDetails);
                        if (result == false)
                        {
                            return Json(new { status = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { status = true, message = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    
                }
                else
                {
                    return Json(new { status = false, message = "No item is selected." }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {

                return Json(new { status = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddToPlaylist(int socialId, string suggestedTrackIds, string playlistId) {
            try
            {
                if (suggestedTrackIds != string.Empty)
                {
                    var acc = _userService.GetAccount(socialId);
                    //var trackIdStack = tracksIds.Split(',');
                    var result = _spotifyService.TracksToPlaylist(acc, suggestedTrackIds, playlistId);
                    if (result == true)
                    {
                        return Json(new { status = true, message = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, message = "No item is selected." }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {
                return Json(new { status=false,message = "Something went wrong."}, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CreatePlaylist(int socialId,string playlistName){
            try
            {
                var acc = _userService.GetAccount(socialId);
                //var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(new { name = playlistName });
                var result = _spotifyService.CreatePlaylist(socialId, acc.SMId, playlistName, acc.AccessDetails);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("false", JsonRequestBehavior.AllowGet);
            }
        }
	}
}