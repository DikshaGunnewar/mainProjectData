using EntitiesLayer.Entities;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLayer.Helper;
using RestSharp;
using EntitiesLayer.ViewModel;
using RestSharp.Deserializers;
using ServiceLayer.Interfaces;
using ServiceLayer.EnumStore;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ServiceLayer.Services
{
    public class SpotifyServices:ISpotifyServices
    {
        #region Initialization
        private readonly IRepository<SocialMedia> _socialMediaRepo;
       private readonly IRepository<AccessDetails> _accessDetail;
       private readonly IRepository<SuperTargetUser> _targetUserRepo;
       private readonly IRepository<TargetPlaylist> _targetPlaylistRepo;
       private readonly IRepository<Activities> _activityRepo;
       private readonly IRepository<SuggestedTracks> _trackSuggestionRepo;
       private readonly IRepository<Tags> _tagRepo;

       private readonly IUserService _userService;  
       private readonly IUnitOfWork _unitOfWork;
       private readonly string consumerKey;
       private readonly string consumerSecret;
       private readonly string ApiURL ;
       private readonly string AccountApiURL;
       private readonly string RedirectURL;

       public SpotifyServices(IRepository<SocialMedia> socialMedia,
           IUnitOfWork unitOfWork, 
           IRepository<AccessDetails> accessDetail,
           IRepository<SuperTargetUser> targetUserRepo,
           IUserService userService, IRepository<TargetPlaylist> targetPlaylistRepo,
           IRepository<Activities> activityRepo, IRepository<SuggestedTracks> trackSuggestionRepo,
           IRepository<Tags> tagRepo
           )
       {
           _socialMediaRepo = socialMedia;
           _unitOfWork = unitOfWork;
           _accessDetail = accessDetail;
           _userService = userService;
           _targetUserRepo = targetUserRepo;
           _targetPlaylistRepo = targetPlaylistRepo;
           _tagRepo = tagRepo;
           _trackSuggestionRepo = trackSuggestionRepo;
           _activityRepo = activityRepo;
           consumerKey = ConfigurationSettings.AppSettings["spotifyId"];
           consumerSecret = ConfigurationSettings.AppSettings["spotifySecret"];
           AccountApiURL = "https://accounts.spotify.com";
           ApiURL = "https://api.spotify.com";
           RedirectURL = ConfigurationSettings.AppSettings["spotifyRedirectUrl"];
      }
        #endregion

       /// <summary>
       /// Method to get the uri for authorizing user in Spotify
       /// </summary>
       /// <returns></returns>
       public string Authorize() {
           try
           {
               var request = new RestRequest("oauth/authorize", Method.GET);
               request.AddHeader("Content-Type", "application/json");
               request.AddParameter("scope", "user-read-private user-read-email user-follow-read user-follow-modify user-library-modify user-library-read playlist-read-private playlist-read-collaborative playlist-modify-public playlist-modify-private user-follow-read");
               request.AddParameter("client_id", consumerKey);
               request.AddParameter("redirect_uri", RedirectURL);
               request.AddParameter("response_type", "code");
               // request.AddParameter("host_id", "QwJrNRmITnC3ZenQzXtedg");
               var response = WebServiceHelper.WebRequest(request, AccountApiURL);
               var uri = response.ResponseUri.AbsoluteUri;

               return uri;
           }
           catch (Exception)
           {
               
               throw;
           }
           
       }
       
        /// <summary>
       /// Method to get OAuth token for authenticate user
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
       public OAuthTokens GetTokensOAuth(string Code)
       {
           var request = new RestRequest("api/token", Method.POST);
           request.AddHeader("Content-Type", "application/json");
           request.AddParameter("client_id", consumerKey);
           request.AddParameter("client_secret", consumerSecret);
           request.AddParameter("grant_type", "authorization_code");
           request.AddParameter("code", Code);
           request.AddParameter("redirect_uri", RedirectURL);
           var response = WebServiceHelper.WebRequest(request, AccountApiURL);
           JsonDeserializer deserial = new JsonDeserializer();
           var result = deserial.Deserialize<OAuthTokens>(response);
           return result;
       }

       public AccessDetails TokenValidate(int socialId, AccessDetails token){
           try
           {
               if (token.Expires_in <= DateTime.UtcNow)
               {
                  var newToken =  RefreshToken(socialId);
                  return newToken;
               }
               else {
                   return token;
               }
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public AccessDetails RefreshToken(int socialId)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccessDetails).Where(x => x.Id == socialId).FirstOrDefault();
               var request = new RestRequest("api/token", Method.POST);
               //+ ":" +System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(consumerSecret))
               request.AddHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(consumerKey+ ":" +consumerSecret)) );
               request.AddParameter("grant_type", "refresh_token");
               request.AddParameter("refresh_token", acc.AccessDetails.Refresh_token);
               var response = WebServiceHelper.WebRequest(request, AccountApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<OAuthTokens>(response);
               acc.AccessDetails.AccessToken = result.access_token;
               acc.AccessDetails.Expires_in = DateTime.UtcNow.AddMinutes(58);
               _unitOfWork.Commit();
               //acc.AccessDetails.Refresh_token = result.refresh_token;
               return acc.AccessDetails;
           }
           catch (Exception)
           {
               
               throw;
           } 
         
       }

       public string SaveAccountDeatils(OAuthTokens tokens, string userId,string Email)
       {
           try
           {
               //AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
               SpotifyUser profile = GetUserprofile(new AccessDetails { AccessToken = tokens.access_token });
               var profilePic=string.Empty;
               var returnMessage = string.Empty;
               //if (profile.images != null) { profilePic = profile.images[0].url; }
               //else { profilePic = null; }
               var checkAccountIsAvail = _socialMediaRepo.Get().Include(x => x.AccessDetails).Where(x => x.SMId == profile.id.ToString() && x.IsDeleted == false).FirstOrDefault();
               if (checkAccountIsAvail == null)
               {
                   SocialMedia socialDetails = new SocialMedia()
                       {
                           UserId = userId,
                           Provider = SocialMediaProviders.Spotify.ToString(),
                           AccessDetails = new AccessDetails { AccessToken = tokens.access_token, Refresh_token = tokens.refresh_token, Expires_in = DateTime.UtcNow.AddMinutes(58) },

                           SMId = profile.id.ToString(),
                           Status = true,

                           //ProfilepicUrl = profilePic,
                           Followers = profile.followers.total,
                           UserName = profile.email,
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

       public bool UpdateProfile(AccessDetails tokens, int socialId)
       {
           try
           {
               var updatedData = GetUserprofile(tokens, socialId);
               var account = _socialMediaRepo.Get().Where(x => x.Id == socialId).FirstOrDefault();
               account.Followers = updatedData.followers.total;
               account.ProfilepicUrl = updatedData.images[0].url != null ? updatedData.images[0].url : string.Empty;
               _unitOfWork.Commit();
               return true;
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
                       targetUserList.Add(new SuperTargetUserVM { Id = item.Id, 
                                                                  IsBlocked = item.IsBlocked, 
                                                                  SMId = item.SMId, 
                                                                  SocailId = socialId, 
                                                                  Followers = item.Followers,
                                                                  UserName = item.UserName });
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
        
       public SpotifyUser GetUserprofile(AccessDetails token,int socialId=0)
       {
           try
           {
               var Token = new AccessDetails();
               if (socialId != 0)
               {
                    Token = TokenValidate(socialId, token);
               }
               else {
                   Token = token;
               }
               var request = new RestRequest("v1/me", Method.GET);
               request.AddHeader("Authorization","Bearer "+ Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyUser>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public SpotifyUser GetUser(AccessDetails token, int socialId ,string userId)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/users/"+userId, Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyUser>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public SpotifyArtist GetArtist(AccessDetails token, int socialId, string artistId)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/artists/" + artistId, Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyArtist>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public object Search(int socialId,string query,AccessDetails token, string type) 
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/search", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               request.AddParameter("q", query);
               request.AddParameter("type", type);
               request.AddParameter("limit", "5");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               if (type == SpotifySearchType.artist.ToString()) { 
                   var result = deserial.Deserialize<SpotifyArtistSearch>(response);
                   return result.artists;
               }
               else if (type == SpotifySearchType.playlist.ToString()) {
                   var result = deserial.Deserialize<SpotifyPlaylistSearch>(response);
                   return result.playlists;
               }
              
               return null;

           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public SpotifyTrackSearchResponse SearchTrack(int socialId, string query, AccessDetails token,int count)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/search", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               request.AddParameter("q", query);
               request.AddParameter("type", "track");
               request.AddParameter("limit", "5");
               request.AddParameter("offset", 5*count);

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyTrackSearch>(response);
               return result.tracks;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public PlaylistTrackResponse GetPlaylistTracks(string playlistId, string ownerId, int socialId, AccessDetails token, int count)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/users/"+ownerId+"/playlists/"+playlistId+"/tracks", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               request.AddParameter("offset", count*5);
               request.AddParameter("limit", "5");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();

               var result = deserial.Deserialize<PlaylistTrackResponse>(response);
                   return result;

           }
           catch (Exception)
           {

               throw;
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
                   acc.AccSettings.TargetPlaylist.Add(new TargetPlaylist { Name = playlist.Name,PlaylistId=playlist.PlaylistId,OwnerId=playlist.OwnerId,TracksCount=playlist.TracksCount});
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
                       targetPlaylist.Add(new TargetPlaylistVM {Id= item.Id,SocailId=socialId,Name=item.Name,OwnerId=item.OwnerId,PlaylistId=item.PlaylistId,TracksCount = item.TracksCount });
                   }
               }
               return targetPlaylist;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<SpotifyArtist> GetArtistRelatedArtist(int socialId, string spotifyArtistId, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/artists/"+spotifyArtistId+"/related-artists", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyRelatedArtist>(response);
               return result.artists;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public SpotifyUserPlaylists GetUserPlaylists(int socialId, string userId, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/users/" + userId + "/playlists", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyUserPlaylists>(response);
               return result;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public SpotifyCategoryList GetCategoryList(int socialId, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/browse/categories", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyCategorySearch>(response);
               return result.categories;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public SpotifyCategory GetCategory(int socialId, string categoryId, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/browse/categories/"+categoryId, Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyCategory>(response);
               return result;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public SpotifyArtistList GetFollowedArtist(int socialId, string query, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/me/following", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               request.AddParameter("type", "artist");
               //request.AddParameter("limit", "10");

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<SpotifyArtistSearch>(response);
               return result.artists;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken,int offset)
       {
           try
           {
               var DateLimit = DateTime.UtcNow.AddDays(-2);
               var activity = _activityRepo.Get().Where(x => x.socialId == socialId && x.ActivityDate >= DateLimit).OrderByDescending(x => x.ActivityDate).Skip(offset).Take(10).ToList();
               List<RecentActivityViewModel> activityDetails = new List<RecentActivityViewModel>();

               foreach (var item in activity)
               {
                   if (item.UserType == SpotifySearchType.artist.ToString()) {
                       var artist = GetArtist(_accessToken, socialId, item.SMId);
                       activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), Content = artist.name, Tag = item.Tag ,ProfilePic = artist.images.Count()>0?artist.images[0].url:""});

                   }
                   else if (item.UserType == SpotifySearchType.user.ToString())
                   {
                       var user = GetUser(_accessToken, socialId, item.SMId);
                       activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), Content = user.display_name != null ? user.display_name : user.id, Tag = item.Tag, ProfilePic = user.images.Count() > 0 ? user.images[0].url : "" });
                   
                   }

               }
               return activityDetails;

           }
           catch (Exception)
           {

               throw;
           }
       }

       public List<SuggestedTracks> GetSuggestedTracks(int socialId) {
           try
           {
               var suggestedTracks = _trackSuggestionRepo.Get().Where(x => x.SocialId == socialId && x.IsIgnored == false && x.IsAdded == false).ToList();
               return suggestedTracks;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public string CreatePlaylist(int socialId, string userId, string playlistName, AccessDetails token)
       {
           try
           {
               var jsonData = new { name = playlistName };
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/users/"+userId+"/playlists", Method.POST);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               request.AddHeader("Content-Type", "application/json");
               //request.AddBody("data", jsonData);
               request.AddJsonBody(jsonData);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "Created")
               {
                   var temp = (JObject)JsonConvert.DeserializeObject(response.Content);
                   var playlistId = temp["id"].Value<string>();
                   return playlistId;
               }
               else
               {
                   return "false";
               }
           }
           catch (Exception)
           {

               return "false";
           }
       }

       public bool SaveTrack(int socialId, string trackId, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/me/tracks?ids=" + trackId + "", Method.PUT);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "OK")
               {
                   var trackDetail = _trackSuggestionRepo.Get().Where(x => x.TrackId == trackId && x.IsIgnored == false && x.SocialId == socialId).FirstOrDefault();
                   trackDetail.IsAdded = true;
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

       public bool AddToPlaylist(int socialId,AddToPlaylistParameter data, AccessDetails token)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/users/" + data.userId + "/playlists/" + data.playlistId + "/tracks?uris=" + data.uri + "", Method.POST);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "Created")
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

       public bool TracksToPlaylist(SocialMediaVM acc,string suggestedTrackIds,string playlistId) {
           try
           {
               var TrackIdStack = suggestedTrackIds.Split(',');
               foreach (var item in TrackIdStack)
               {
                   var trackId = Int64.Parse(item);
                   var trackDetail = _trackSuggestionRepo.Get().Where(x => x.Id == trackId).FirstOrDefault();
                   var result = AddToPlaylist(acc.Id, new AddToPlaylistParameter { playlistId = playlistId, uri = trackDetail.uri, userId = acc.SMId }, acc.AccessDetails);
                   if (result == false)
                   {
                       return false;
                   }
                   else {
                       trackDetail.IsAdded = true;
                       _unitOfWork.Commit();
                   }
               }
               
               return true;
           }
           catch (Exception)
           {
               return false;

           }       
       }

       public bool IgnoreTrack(int socialId, string trackIds) {
           try
           {
            var trackIdStack = trackIds.Split(',');
            foreach (var item in trackIdStack)
            {
                var track =_trackSuggestionRepo.Get().Where(x => x.SocialId == socialId && x.TrackId == item).FirstOrDefault();
                track.IsIgnored = true;
                _unitOfWork.Commit();
            }
            return true;
           }
           catch (Exception)
           {
             return false;
           }
       }

       public bool Follow(int socialId, string spotifyId, AccessDetails token, string type)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/me/following?type="+type+"&ids="+spotifyId+"", Method.PUT);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "NoContent")
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

       public bool CheckArtistFollowedByUser(int socialId, string spotifyId, AccessDetails token, string type)
       {
           try
           {
               var Token = TokenValidate(socialId, token);
               var request = new RestRequest("v1/me/following/contains", Method.GET);
               request.AddHeader("Authorization", "Bearer " + Token.AccessToken);
               //request.AddParameter("q", spotifyId);
               request.AddParameter("type", type);
               //request.AddParameter("limit", "10");
               request.AddParameter("ids", spotifyId);

               var response = WebServiceHelper.WebRequest(request, ApiURL);
               if (response.StatusCode.ToString() == "OK")
               {
                   JsonDeserializer deserial = new JsonDeserializer();
                   var result = deserial.Deserialize<List<bool>>(response);

                   return result[0];
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

       public void CheckForConversion(SocialMediaVM acc)
       {
           try
           {
               var accountActivity = _activityRepo.Get().Where(x => x.socialId == acc.Id).ToList();
               foreach (var item in accountActivity)
               {
                        var Token = TokenValidate(acc.Id, acc.AccessDetails);
                        if (CheckArtistFollowedByUser(acc.Id, item.SMId, Token,item.UserType) && _userService.CheckIfUserConvert(item.SMId, item.socialId) == false)
                       {
                           _userService.SaveConversion(new Conversions { SMId = acc.SMId, socialId = acc.Id, ConvertDate = DateTime.UtcNow, Username = item.Username, Tag = item.Tag });
                       }

               }

           }
           catch (Exception)
           {

               throw;
           }
       }

       public async Task<bool> SpotifyAlgoForUser(SocialMediaVM acc) 
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
               else if(acc.IsTrail == true)
               {
                   await TargetArtistRelatedArtistTask(artists, acc);//if supertargetting is allowed in plan. 
                   TargetPlaylistTask(playlist, acc);
                   TagRegardingTask(tags, acc);
               }
                FollowingPlaylistTask(acc);
                UpdateProfile(acc.AccessDetails, acc.Id);
               _userService.SaveFollowersCount(acc.Id, GetUserprofile(acc.AccessDetails, acc.Id).followers.total);
               return true;           
           }
           catch (Exception)
           {
               return false;
           }
       }

       #region Algo method
       public async Task<bool> TargetArtistRelatedArtistTask(List<SuperTargetUser> artists, SocialMediaVM acc) {
           try
           {
               if (artists.Count() > 0) {
                   foreach (var artist in artists)
                   {
                       var relatedArtists =  GetArtistRelatedArtist(acc.Id, artist.SMId, acc.AccessDetails);
                        if(relatedArtists.Count()!= 0){
                            //foreach (var item in relatedArtists)
                            //{
                            int count = 0;
                            while (count < Int16.Parse(ConfigurationSettings.AppSettings["spotifyRelatedArtistFollow"].ToString()))
                            {
                                Random r = new Random();
                                var key = r.Next(relatedArtists.Count());
                                if (CheckArtistFollowedByUser(acc.Id, relatedArtists[key].id, acc.AccessDetails, relatedArtists[key].type) == false)
                                {
                                    if (Follow(acc.Id, relatedArtists[key].id, acc.AccessDetails, relatedArtists[key].type) == true)
                                    {
                                        _userService.SaveActivity(new Activities
                                        {
                                            SMId = relatedArtists[key].id,socialId = acc.Id,Activity = Activity.Follow.ToString(),ActivityDate = DateTime.UtcNow,Tag = '#' + artist.UserName,Username = relatedArtists[key].name,UserType = relatedArtists[key].type
                                        });
                                        count++;
                                        await Task.Delay(100000);
                                    }
                                }
                            }
                           // }
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

       public async void TargetPlaylistTask(List<TargetPlaylist> playlists,SocialMediaVM acc) {
           try
           {
               if (playlists.Count() > 0) {
                   foreach (var playlist in playlists)
                   {
                       int limit = 0;
                       bool breakLoop =false;
                       int count = 0;
                       int spotifytargetPlaylistUserArtistFollow=Int16.Parse(ConfigurationSettings.AppSettings["spotifytargetPlaylistUserArtistFollow"].ToString());
                       while (count < spotifytargetPlaylistUserArtistFollow || breakLoop == true)
                       {
                           var tracks = GetPlaylistTracks(playlist.PlaylistId, playlist.OwnerId, acc.Id, acc.AccessDetails, limit);
                           foreach (var item in tracks.items)
                           {
                               if (CheckArtistFollowedByUser(acc.Id, item.added_by.id, acc.AccessDetails, item.added_by.type) == false)
                               {
                                   if (Follow(acc.Id, item.added_by.id, acc.AccessDetails, item.added_by.type))
                                   {
                                       _userService.SaveActivity(new Activities { SMId = item.added_by.id, socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + playlist.Name, Username = item.added_by.display_name, UserType = item.added_by.type });
                                       count++;
                                       await Task.Delay(100000);                                      
                                   }
                               }
                               if (CheckArtistFollowedByUser(acc.Id, item.track.artists[0].id, acc.AccessDetails, item.track.artists[0].type) == false)
                               {
                                   if (Follow(acc.Id, item.track.artists[0].id, acc.AccessDetails, item.track.artists[0].type))
                                   {
                                       _userService.SaveActivity(new Activities { SMId = item.track.artists[0].id, socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + playlist.Name, Username = item.added_by.display_name, UserType = item.track.artists[0].type });
                                       count++;
                                       await Task.Delay(100000);                                      
                                   }
                               }

                           }
                           if (limit * 5 > tracks.total) {
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

       public async void FollowingPlaylistTask(SocialMediaVM acc) {
           try
           {
               var playlists = GetUserPlaylists(acc.Id, acc.SMId, acc.AccessDetails);
               if (playlists.items.Count() > 0) {
                   foreach (var playlist in playlists.items)
                   {
                       if (playlist.owner.id != acc.SMId)
                       {
                           int limit = 0;
                           int count = 0;
                           int spotifyFollowingPlaylistTask = Int16.Parse(ConfigurationSettings.AppSettings["spotifyFollowingPlaylistTask"].ToString());

                           while (count < spotifyFollowingPlaylistTask)
                           {
                               var tracks = GetPlaylistTracks(playlist.id, playlist.owner.id, acc.Id, acc.AccessDetails, limit);
                               foreach (var item in tracks.items)
                               {
                                   
                                   if (CheckArtistFollowedByUser(acc.Id, item.added_by.id, acc.AccessDetails, item.added_by.type) == false)
                                   {
                                       if (Follow(acc.Id, item.added_by.id, acc.AccessDetails, item.added_by.type))
                                       {
                                           _userService.SaveActivity(new Activities { SMId = item.added_by.id, socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + playlist.name, Username = item.added_by.id, UserType = item.added_by.type });
                                           count++;
                                           await Task.Delay(100000);                                      
                                       }
                                   }
                                   if (CheckArtistFollowedByUser(acc.Id, item.added_by.id, acc.AccessDetails, item.added_by.type) == false)
                                   {
                                       if (Follow(acc.Id, item.track.artists[0].id, acc.AccessDetails, item.track.artists[0].type))
                                       {
                                           _userService.SaveActivity(new Activities { SMId = item.track.artists[0].id, socialId = acc.Id, Activity = Activity.Follow.ToString(), ActivityDate = DateTime.UtcNow, Tag = '@' + playlist.name, Username = item.added_by.id, UserType = item.track.artists[0].type });
                                           count++;
                                           await Task.Delay(100000);                                      
                                       }
                                   }
                                   if (_trackSuggestionRepo.Get().Where(x => x.SocialId == acc.Id && x.TrackId == item.track.id).FirstOrDefault() == null) {
                                       var suggestedTrack = new SuggestedTracks { Provider = SocialMediaProviders.Spotify.ToString(), SocialId = acc.Id, TrackId = item.track.id, TrackName = item.track.name, uri = item.track.uri, SourceName = "@" + playlist.name, SourceType = "Playlist" };
                                       _trackSuggestionRepo.Add(suggestedTrack);
                                       _unitOfWork.Commit();
                                       count++;
                                   }

                               }

                               limit++;
                           }//tracks for loop


                       }
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
                       int count = 0;int limit = 0;
                       int SearchTrackByTagTask = Int16.Parse(ConfigurationSettings.AppSettings["spotifySearchTrackByTagTask"].ToString());

                       while (limit < SearchTrackByTagTask)
                       {
                           var tracksResult = SearchTrack(acc.Id, item.TagName, acc.AccessDetails,count);
                           if (tracksResult.items.Count() > 0) {
                               foreach (var track in tracksResult.items)
                               {
                                   if (_trackSuggestionRepo.Get().Where(x => x.SocialId == acc.Id && x.TrackId == track.id).FirstOrDefault() == null)
                                   {
                                       var suggestedTrack = new SuggestedTracks { Provider = SocialMediaProviders.Spotify.ToString(), SocialId = acc.Id, TrackId = track.id, TrackName = track.name, uri = track.uri, SourceName = "#" + item.TagName, SourceType = "Tag" };
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
