using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using RestSharp;
using RestSharp.Deserializers;
using ServiceLayer.Helper;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using ServiceLayer.EnumStore;

namespace ServiceLayer.Services
{
    public class DeezerServices:IDeezerServices
    {
        #region initialization
        private readonly IRepository<SocialMedia> _socialMediaRepo;
       private readonly IRepository<AccessDetails> _accessDetailRepo;
       private readonly IRepository<SuperTargetUser> _targetUserRepo;
       private readonly IRepository<Activities> _activityRepo;
       private readonly IRepository<SuperTargetUser> _superTargetUserRepo;
       private readonly IRepository<FollowersGraph> _followersGraphRepo;
       private readonly IRepository<TargetPlaylist> _targetPlaylistRepo;
       private readonly IRepository<SuggestedTracks> _trackSuggestionRepo;

        
       private readonly IRepository<Tags> _tagRepo;

       private readonly IUserService _userService;

       private readonly IUnitOfWork _unitOfWork;
       private readonly string consumerKey;
       private readonly string consumerSecret;
       private readonly string RedirectURL;
       private readonly string AccountApiURL;
       private readonly string ApiURL;

       public DeezerServices(IRepository<SocialMedia> socialMedia,
           IUnitOfWork unitOfWork, 
           IRepository<AccessDetails> accessDetail, 
           IRepository<SuperTargetUser> targetUserRepo,
           IRepository<Activities> activityRepo, IUserService userService,IRepository<Tags> tagRepo,
           IRepository<SuperTargetUser> superTargetUserRepo, IRepository<FollowersGraph> followersGraphRepo,
           IRepository<TargetPlaylist> targetPlaylistRepo, IRepository<SuggestedTracks> trackSuggestionRepo)
       {
           _socialMediaRepo = socialMedia;
           _unitOfWork = unitOfWork;
           _targetUserRepo = targetUserRepo;
           _accessDetailRepo = accessDetail;
           _activityRepo = activityRepo;
           _superTargetUserRepo = superTargetUserRepo;
           _tagRepo = tagRepo;
           _trackSuggestionRepo = trackSuggestionRepo;
           _targetPlaylistRepo = targetPlaylistRepo;
           _followersGraphRepo = followersGraphRepo;
           _userService = userService;
           consumerKey = ConfigurationSettings.AppSettings["deezerId"];
           consumerSecret = ConfigurationSettings.AppSettings["deezerSecret"];
           RedirectURL = ConfigurationSettings.AppSettings["RedirectURLDeezer"];
           AccountApiURL = "https://connect.deezer.com/";
           ApiURL = "https://api.deezer.com/";
      }

        #endregion  
       public string Authorize()
       {
           //var token = Session["FacebookAccessToken"];

           var request = new RestRequest("oauth/auth.php", Method.GET);
           request.AddHeader("Content-Type", "application/json");
           request.AddParameter("perms", "basic_access,email,manage_library,offline_access");
           request.AddParameter("app_id", consumerKey);
           request.AddParameter("redirect_uri", RedirectURL);
           request.AddParameter("response_type", "code");

           //request.AddParameter("response_type", "Code");
           // request.AddParameter("host_id", "QwJrNRmITnC3ZenQzXtedg");
           var response = WebServiceHelper.WebRequest(request, AccountApiURL);

           var uri = response.ResponseUri.AbsoluteUri;
           //JsonDeserializer deserial = new JsonDeserializer();
           //UserInfo result = deserial.Deserialize<UserInfo>(response);
           return uri.ToString();
       }

       /// <summary>
       /// Method to get OAuth token for authenticate user
       /// </summary>
       /// <param name="Code"></param>
       /// <returns></returns>
       public OAuthTokens GetTokensOAuth(string Code)
       {
           var request = new RestRequest("oauth/access_token.php?app_id=" + consumerKey + "&secret=" + consumerSecret + "&code=" + Code + "", Method.POST);

           var response = WebServiceHelper.WebRequest(request, AccountApiURL);
           //JsonDeserializer deserial = new JsonDeserializer();
           //var result = deserial.Deserialize<OAuthTokens>(response);
           OAuthTokens result = new OAuthTokens() { access_token = response.Content.Substring(13, (63 - 13)) };
           return result;
       }

        /// <summary>
        /// Save user social profile details/tokens 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="userId"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
       public string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email)
       {
           try
           {
               //AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
               DeezerUserVM profile = GetCurrentUserprofile(new AccessDetails { AccessToken = tokens.access_token });
               var followers = GetCurrentUserFollower(new AccessDetails { AccessToken = tokens.access_token });
               var returnMessage = string.Empty;
               var checkAccountIsAvail = _socialMediaRepo.Get().Include(x => x.AccessDetails).Where(x => x.SMId == profile.id.ToString() && x.IsDeleted == false).FirstOrDefault();
               if (checkAccountIsAvail == null)
               {
                   SocialMedia socialDetails = new SocialMedia()
                   {
                       UserId = userId,
                       Provider = SocialMediaProviders.Deezer.ToString(),
                       AccessDetails = new AccessDetails { AccessToken = tokens.access_token, Refresh_token = tokens.refresh_token, Expires_in = DateTime.UtcNow.AddMinutes(58) },
                       SMId = profile.id.ToString(),
                       Status = true,
                       ProfilepicUrl = profile.picture_small,
                       Followers = followers.total,
                       UserName = profile.name,
                       AccSettings = new AccSettings()
                   };
                   socialDetails.AccSettings.UserManagement.Add(new UserManagement { Email = Email, userId = userId, Role = "Owner" });
                   _socialMediaRepo.Add(socialDetails);
               }
               else if (checkAccountIsAvail.UserId == userId)
               {
                   checkAccountIsAvail.AccessDetails.AccessToken = tokens.access_token;
                   checkAccountIsAvail.IsInvalid = false;
                   checkAccountIsAvail.Status = true;
                   returnMessage = "Already added.";
               }
               else
               {
                   checkAccountIsAvail.AccessDetails.AccessToken = tokens.access_token;
                   checkAccountIsAvail.IsInvalid = false;
                   checkAccountIsAvail.Status = true;
                   returnMessage = "Cannot add this account, as already added by other user.";
               }
               _unitOfWork.Commit();
               return returnMessage;
           }
           catch (Exception)
           {
               return "Something went wrong.";
           }
       }

        /// <summary>
        /// get current user's profile
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
       public DeezerUserVM GetCurrentUserprofile(AccessDetails token)
       {
           try
           {
               var request = new RestRequest("/user/me", Method.GET);
               request.AddParameter("access_token", token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserVM>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

        /// <summary>
        /// Get current user's followers list & count
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
       public DeezerUserFollower GetCurrentUserFollower(AccessDetails token)
       {
           try
           {
               var request = new RestRequest("/user/me/followers", Method.GET);
               request.AddParameter("access_token", token.AccessToken);
               request.AddParameter("limit", "100");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserFollower>(response);
               if (result.data.Count < result.total) {
                    response = WebServiceHelper.WebRequest(request, ApiURL);
                    request.AddParameter("limit", result.total);
                    result = deserial.Deserialize<DeezerUserFollower>(response);
               }
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerUserVM> SearchArtist(string query) {
           try
           {
               var request = new RestRequest("/search/artist/", Method.GET);
               request.AddParameter("q", query);              
               request.AddParameter("limit", "12");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerArtistSearch>(response);
               return result.data;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public List<DeezerTrack> SearchTrack(string query)
       {
           try
           {
               int count = Int16.Parse(ConfigurationSettings.AppSettings["deezerSearchTrackCount"].ToString());
               var request = new RestRequest("/search/track/", Method.GET);
               request.AddParameter("q", query);
               request.AddParameter("limit", count);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerTrackSearch>(response);
               return result.data;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerPlaylistVM> SearchPlaylist(string query)
       {
           try
           {
               var request = new RestRequest("/search/playlist/", Method.GET);
               request.AddParameter("q", query);
               request.AddParameter("limit", "12");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerPlaylistSearch>(response);
               return result.data;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public DeezerUserVM GetArtist(string Id)
       {
           try
           {
               var request = new RestRequest("/artist/"+Id, Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserVM>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerUserVM> GetArtistFans(string artistId) {
           try
           {
               List<DeezerUserVM> fansList = new List<DeezerUserVM>();
               var request = new RestRequest("/artist/" + artistId + "/fans", Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserSearch>(response);
               if (result.data.Count > 0)
               {
                   foreach (var item in result.data)
                   {
                       fansList.Add(item);
                   }

                   if (result.data.Count() < Int16.Parse(result.total))
                   {
                       while (result.next == null)
                       {
                           request = new RestRequest(result.next, Method.GET);
                           response = WebServiceHelper.WebRequest(request, ApiURL);
                           result = deserial.Deserialize<DeezerUserSearch>(response);
                           foreach (var item in result.data)

                               fansList.Add(item);
                       }
                   }
               }
               return fansList;

           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public DeezerUserVM GetUser(string Id)
       {
           try
           {
               var request = new RestRequest("/user/" + Id, Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserVM>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public DeezerUserVM GetTrack(string Id)
       {
           try
           {
               var request = new RestRequest("/track/" + Id, Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserVM>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public DeezerPlaylistVM GetPlaylist(string Id)
       {
           try
           {
               var request = new RestRequest("/playlist/" + Id, Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerPlaylistVM>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerTrack> GetPlaylistTrack(string Id)
       {
           try
           {
               var request = new RestRequest("/playlist/" + Id + "/tracks", Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerTrackSearch>(response);
               return result.data;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerPlaylistVM> UsersPlaylists(string query)
       {
           try
           {
               var request = new RestRequest("/user/me/playlists", Method.GET);
               //request.AddParameter("q", query);
               //request.AddParameter("limit", "12");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerPlaylistSearch>(response);
               return result.data;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerUserVM> GetRelatedArtist(string Id)
       {
           try
           {
               var request = new RestRequest("/artist/" + Id + "/related", Method.GET);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerArtistSearch>(response);
               return result.data;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<DeezerUserVM> GetUsersFavArtist(AccessDetails token)
       {
           try
           {
               List<DeezerUserVM> followersList = new List<DeezerUserVM>();

               var request = new RestRequest("/user/me/artists", Method.GET);
               request.AddParameter("access_token", token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerArtistSearch>(response);
               if (result.data.Count > 0) {
                   foreach (var item in result.data)
                   {
                       followersList.Add(item);
                   }
                   if (result.data.Count() < Int16.Parse(result.total))
                   {
                       while (result.next == null)
                       {
                           request = new RestRequest(result.next, Method.GET);
                           response = WebServiceHelper.WebRequest(request, ApiURL);
                           result = deserial.Deserialize<DeezerArtistSearch>(response);
                           foreach (var item in result.data)

                               followersList.Add(item);
                       }
                   }
               }

               return followersList;
               
           }
           catch (Exception)
           {
               throw;
           }
       }

       public List<DeezerUserVM> GetUsersFollowing(AccessDetails token)
       {
           try
           {
               List<DeezerUserVM> followersList = new List<DeezerUserVM>();

               var request = new RestRequest("/user/me/followings", Method.GET);
               request.AddParameter("access_token", token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<DeezerUserSearch>(response);
               if (result.data.Count > 0)
               {
                   foreach (var item in result.data)
                   {
                       followersList.Add(item);
                   }
                   if (result.data.Count() < Int16.Parse(result.total))
                   {
                       while (result.next == null)
                       {
                           request = new RestRequest(result.next, Method.GET);
                           response = WebServiceHelper.WebRequest(request, ApiURL);
                           result = deserial.Deserialize<DeezerUserSearch>(response);
                           foreach (var item in result.data)

                               followersList.Add(item);
                       }
                   }
               }

               return followersList;

           }
           catch (Exception)
           {
               throw;
           }
       }

       public bool FollowArtist(string artistId,AccessDetails token) {
           try
           {
                var request = new RestRequest("/user/me/artists", Method.POST);
                request.AddParameter("artist_id", artistId);
                request.AddParameter("access_token", token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.Content == "true")
               {
                   return true;
               }
               else {
                   return false;
               }
           }
           catch (Exception)
           {
               return false;
           }
       }

       public bool FollowUser(string user_id,AccessDetails token)
       {
           try
           {
               var request = new RestRequest("/user/me/followings", Method.POST);
               request.AddParameter("user_id", user_id);
               request.AddParameter("access_token", token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.Content == "true")
               {
                   return true;
               }
               else
               {
                   return false;
               }
           }
           catch (Exception)
           {
               return false;
           }
       }

       public bool FollowPlaylist(string playlist_id, AccessDetails token)
       {
           try
           {
               var request = new RestRequest("user/me/playlists", Method.POST);
               request.AddParameter("playlist_id", playlist_id);
               request.AddParameter("access_token", token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.Content == "true")
               {
                   return true;
               }
               else
               {
                   return false;
               }
           }
           catch (Exception)
           {
               return false;
           }
       }

       public bool AddBlockSuperTargetUser(SuperTargetUserVM user)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.SuperTargetUser).Where(x => x.Id == user.SocailId).FirstOrDefault();
               if (acc.AccSettings == null)
               {
                   acc.AccSettings = new AccSettings();
               }
               var check = acc.AccSettings.SuperTargetUser.Where(x => x.SMId.ToString() == user.SMId.ToString()).FirstOrDefault();
               if (check == null)
               {
                   acc.AccSettings.SuperTargetUser.Add(new SuperTargetUser { IsBlocked = user.IsBlocked, UserName = user.UserName, SMId = user.SMId.ToString(), Followers = user.Followers });
                   _unitOfWork.Commit();
                   return true;
               }
               else
               {
                   return false;
               }


           }
           catch (Exception)
           {
               return false;
           }

       }

       public List<SuperTargetUserVM> GetAllTargetUser(int socialId)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.SuperTargetUser).Where(x => x.Id == socialId).FirstOrDefault();
               List<SuperTargetUserVM> targetUserList = new List<SuperTargetUserVM>();
               if (acc.AccSettings != null)
               {
                   foreach (var item in acc.AccSettings.SuperTargetUser)
                   {
                       targetUserList.Add(new SuperTargetUserVM
                       {
                           Id = item.Id,
                           IsBlocked = item.IsBlocked,
                           SMId = item.SMId,
                           SocailId = socialId,
                           Followers = item.Followers,
                           UserName = item.UserName
                       });
                   }
               }
               return targetUserList;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public bool RemoveTargetUser(int targetUserId)
       {
           try
           {
               var targetUser = _targetUserRepo.Get().Where(x => x.Id == targetUserId).FirstOrDefault();
               _targetUserRepo.Remove(targetUser);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       public bool AddTargetPlaylist(TargetPlaylistVM playlist)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.TargetPlaylist).Where(x => x.Id == playlist.SocailId).FirstOrDefault();
               if (acc.AccSettings == null)
               {
                   acc.AccSettings = new AccSettings();
               }
               var check = acc.AccSettings.TargetPlaylist.Where(x => x.PlaylistId.ToString() == playlist.PlaylistId.ToString()).FirstOrDefault();
               if (check == null)
               {
                   acc.AccSettings.TargetPlaylist.Add(new TargetPlaylist { Name = playlist.Name, PlaylistId = playlist.PlaylistId, OwnerId = playlist.OwnerId, TracksCount = playlist.TracksCount });
                   _unitOfWork.Commit();
                   return true;
               }
               else
               {
                   return false;
               }


           }
           catch (Exception)
           {
               return false;
           }

       }

       public List<TargetPlaylistVM> GetAllTargetPlaylist(int socialId)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.TargetPlaylist).Where(x => x.Id == socialId).FirstOrDefault();
               List<TargetPlaylistVM> targetPlaylist = new List<TargetPlaylistVM>();
               if (acc.AccSettings != null)
               {
                   foreach (var item in acc.AccSettings.TargetPlaylist)
                   {
                       targetPlaylist.Add(new TargetPlaylistVM { Id = item.Id, SocailId = socialId, Name = item.Name, OwnerId = item.OwnerId, PlaylistId = item.PlaylistId, TracksCount = item.TracksCount });
                   }
               }
               return targetPlaylist;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public bool RemoveTargetPlaylist(int targetPlaylistId)
       {
           try
           {
               var targetPlaylist = _targetPlaylistRepo.Get().Where(x => x.Id == targetPlaylistId).FirstOrDefault();
               _targetPlaylistRepo.Remove(targetPlaylist);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       public bool UpdateProfile(AccessDetails tokens,int socialId)
       {
           try
           {
               var updatedData = GetCurrentUserprofile(tokens);
               var followersCount = GetCurrentUserFollower(tokens).total;
               var account = _socialMediaRepo.Get().Where(x => x.Id == socialId).FirstOrDefault();
               account.Followers = followersCount;
               account.ProfilepicUrl = updatedData.picture_small;
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }

       }

       public bool CheckArtistFollowedByUser(long SMId, AccessDetails tokens) {
           try
           {
               var favArtist = GetUsersFavArtist(tokens);
               foreach (var item in favArtist)
               {
                   if (item.id == SMId)
                   {
                       return true;   
                   }
               }
               return false;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken, int offset)
       {
           try
           {
               var DateLimit = DateTime.UtcNow.AddDays(-2);
               var activity = _activityRepo.Get().Where(x => x.socialId == socialId && x.ActivityDate >= DateLimit).OrderByDescending(x => x.ActivityDate).Skip(offset).Take(10).ToList();
               List<RecentActivityViewModel> activityDetails = new List<RecentActivityViewModel>();

               foreach (var item in activity)
               {
                   if (item.UserType == SpotifySearchType.artist.ToString())
                   {
                       var artist = GetArtist(item.SMId);
                       activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), Content = artist.name, Tag = item.Tag, ProfilePic = artist.picture_small  });

                   }

               }
               return activityDetails;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public void CheckForConversion(SocialMediaVM acc)
       {
           try
           {
               var accountActivity = _activityRepo.Get().Where(x => x.socialId == acc.Id).ToList();
               var followerList = GetCurrentUserFollower(acc.AccessDetails);
               foreach (var activity in accountActivity)
               {
                   if (followerList.data.Where(x => x.id.ToString() == activity.SMId).FirstOrDefault() != null && _userService.CheckIfUserConvert(activity.SMId, activity.socialId) == false)
                   {
                       _userService.SaveConversion(new Conversions { SMId = acc.SMId, socialId = acc.Id, ConvertDate = DateTime.UtcNow, Username = activity.Username, Tag = activity.Tag });
                   }

               }

           }
           catch (Exception)
           {

               throw;
           }
       }

       //public bool CheckUserFollowedByUser(long SMId, AccessDetails tokens)
       //{
       //    try
       //    {
       //        var followingUser = GetUsersFollowing(tokens);
       //        foreach (var item in followingUser)
       //        {
       //            if (item.id == SMId)
       //            {
       //                return true;
       //            }
       //        }
       //        return false;
       //    }
       //    catch (Exception)
       //    {

       //        throw;
       //    }
       //}

       public async Task<bool> DeezerAlgoForUser(SocialMediaVM acc)
       {
           try
           {
               List<Tags> tags = _tagRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
               List<SuperTargetUser> artists = _targetUserRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
               List<TargetPlaylist> playlist = _targetPlaylistRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
               if (acc.IsSubscribed == true)
               {
                   var planDetails = _userService.GetASubscriptionPlan(acc.planId);
                   if (planDetails.AllowSuperTargeting == true)
                   {
                       await TargetArtistRelatedArtistTask(artists, acc);//if supertargetting is allowed in plan. 
                       TargetPlaylistTask(playlist, acc);
                   }
                   if (planDetails.AllowNegativeTags == false)
                   {
                       tags.RemoveAll(x => x.IsBlocked == true); //disabling negative keywords checking by removing blocked tags from list.
                   }
                   TagRegardingTask(tags, acc);

               }
               else if (acc.IsTrail == true)
               {
                   await TargetArtistRelatedArtistTask(artists, acc);//if supertargetting is allowed in plan. 
                TargetPlaylistTask(playlist, acc);
                   TagRegardingTask(tags, acc);
               }
               UpdateProfile(acc.AccessDetails,acc.Id);
               _userService.SaveFollowersCount(acc.Id, GetCurrentUserFollower(acc.AccessDetails).total);
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       #region Algo method
       public async Task<bool> TargetArtistRelatedArtistTask(List<SuperTargetUser> artists, SocialMediaVM acc)
       {
           try
           {
               if (artists.Count() > 0)
               {
                   foreach (var artist in artists)
                   {

                         //related artist task
                           var relatedArtists = GetRelatedArtist(artist.SMId);
                           if (relatedArtists.Count() != 0)
                           {
                               //foreach (var item in relatedArtists)
                               //{
                               int count = 0;
                               while (count < Int16.Parse(ConfigurationSettings.AppSettings["spotifyRelatedArtistFollow"].ToString()))
                               {
                                   Random r = new Random();
                                   var key = r.Next(relatedArtists.Count());
                                   if (CheckArtistFollowedByUser(relatedArtists[key].id, acc.AccessDetails) == false)
                                   {
                                       if (FollowArtist(relatedArtists[key].id.ToString(), acc.AccessDetails) == true)
                                       {
                                           _userService.SaveActivity(new Activities
                                           {
                                               SMId = relatedArtists[key].id.ToString(),
                                               socialId = acc.Id,
                                               Activity = Activity.Follow.ToString(),
                                               ActivityDate = DateTime.UtcNow,
                                               Tag = '#' + artist.UserName,
                                               Username = relatedArtists[key].name,
                                               UserType = relatedArtists[key].type
                                           });
                                           count++;
                                           await Task.Delay(10000);
                                       }
                                   }
                               }
                           // }
                       }
                       //artist fans task

                       var artistFans = GetArtistFans(artist.SMId);
                       if (artistFans.Count() != 0)
                       {

                           var followingUser = GetUsersFollowing(acc.AccessDetails);
                           //foreach (var item in relatedArtists)
                           //{
                           int limit = 0;
                           while (limit < Int16.Parse(ConfigurationSettings.AppSettings["spotifyRelatedArtistFollow"].ToString()))
                           {
                               Random r = new Random();
                               var key = r.Next(artistFans.Count());
                               if (followingUser.Where(x=>x.id == artistFans[key].id).FirstOrDefault() == null)
                               {
                                   if (FollowUser(artistFans[key].id.ToString(), acc.AccessDetails) == true)
                                   {
                                       _userService.SaveActivity(new Activities
                                       {
                                           SMId = artistFans[key].id.ToString(),
                                           socialId = acc.Id,
                                           Activity = Activity.Follow.ToString(),
                                           ActivityDate = DateTime.UtcNow,
                                           Tag = '#' + artist.UserName,
                                           Username = artistFans[key].name,
                                           UserType = artistFans[key].type
                                       });
                                       limit++;
                                       await Task.Delay(10000);
                                   }
                               }
                           }
                       }


                   }
               }
               return true;
           }
           catch (Exception)
           {

               return false;
           }

       }

       public async void TargetPlaylistTask(List<TargetPlaylist> playlists, SocialMediaVM acc)
       {
           try
           {
               if (playlists.Count() > 0)
               {
                   foreach (var playlist in playlists)
                   {
                       int limit = 0;
                       bool breakLoop = false;
                       int count = 0;
                       int spotifytargetPlaylistUserArtistFollow = Int16.Parse(ConfigurationSettings.AppSettings["spotifytargetPlaylistUserArtistFollow"].ToString());
                       while (count < spotifytargetPlaylistUserArtistFollow || breakLoop == true)
                       {
                           var tracks = GetPlaylistTrack(playlist.PlaylistId);
                           foreach (var item in tracks)
                           {
                               if (CheckArtistFollowedByUser(item.artist.id,acc.AccessDetails) == false)
                               {
                                   if (FollowArtist(item.artist.id.ToString(), acc.AccessDetails))
                                   {
                                       _userService.SaveActivity(new Activities { SMId = item.artist.id.ToString(), socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + playlist.Name, Username = item.artist.name});
                                       count++;
                                       await Task.Delay(10000);
                                   }
                               }
                               //if (CheckArtistFollowedByUser(acc.Id, item.track.artists[0].id, acc.AccessDetails, item.track.artists[0].type) == false)
                               //{
                               //    if (Follow(acc.Id, item.track.artists[0].id, acc.AccessDetails, item.track.artists[0].type))
                               //    {
                               //        _userService.SaveActivity(new Activities { SMId = item.track.artists[0].id, socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + playlist.Name, Username = item.added_by.display_name, UserType = item.track.artists[0].type });
                               //        count++;
                               //        await Task.Delay(100000);
                               //    }
                               //}

                           }
                           if (limit * 5 > tracks.Count())
                           {
                               breakLoop = true;
                           }
                           limit++;
                       }//while end

                       var temp = _targetPlaylistRepo.Get().Where(x => x.Id == playlist.Id).FirstOrDefault();
                       temp.count = temp.count + 1;
                       _unitOfWork.Commit();
                   }
               }
           }
           catch (Exception)
           {
               throw;
           }
       }

       public async void TagRegardingTask(List<Tags> tags, SocialMediaVM acc)
       {
           try
           {
               if (tags.Count() > 0)
               {
                   foreach (var item in tags)
                   {
                       if (item.IsBlocked != true)
                       {
                           int limit = 0;
                           int SearchTrackByTagTask = Int16.Parse(ConfigurationSettings.AppSettings["spotifySearchTrackByTagTask"].ToString());

                           while (limit < SearchTrackByTagTask)
                           {
                               var tracksResult = SearchTrack(item.TagName);
                               if (tracksResult.Count() > 0)
                               {
                                   foreach (var track in tracksResult)
                                   {
                                       if (_trackSuggestionRepo.Get().Where(x => x.SocialId == acc.Id && x.TrackId == track.id.ToString()).FirstOrDefault() == null)
                                       {
                                           var suggestedTrack = new SuggestedTracks { Provider = SocialMediaProviders.Spotify.ToString(), SocialId = acc.Id, TrackId = track.id.ToString(), TrackName = track.title, uri = track.link, SourceName = "#" + item.TagName, SourceType = "Tag" };
                                           _trackSuggestionRepo.Add(suggestedTrack);
                                           _unitOfWork.Commit();
                                           limit++;
                                           await Task.Delay(100000);
                                       }

                                   }
                               }
                           }

                       }
                   }
               }

           }
           catch (Exception)
           {

               throw;
           }
       }

       #endregion
    }
}
