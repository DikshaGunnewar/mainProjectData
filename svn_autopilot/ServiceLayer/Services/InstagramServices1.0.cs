using EntitiesLayer.Entities;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLayer.Helper;
using EntitiesLayer.ViewModel;
using RestSharp.Deserializers;
using ServiceLayer.Enum;
using ServiceLayer.Interfaces;
using System.Data.Entity;
using System.Collections;


namespace ServiceLayer.Services
{
    
   public class InstagramServices:IInstagramServices
    {
       private readonly IRepository<SocialMedia> _socialMedia;
       private readonly IRepository<AccessDetails> _accessDetail;
       private readonly IRepository<SuperTargetUser> _targetUserRepo;
       private readonly IRepository<Tags> _tagRepo;
       private readonly IRepository<Activities> _activityRepo;
     
       private readonly IUserService _userService;  
       private readonly IUnitOfWork _unitOfWork;
       private readonly string consumerKey;
       private readonly string consumerSecret;
       private readonly string redirecturl;
       private readonly string ApiURL;
       //private readonly TwitterService service;

      // public InstagramServices(IRepository<SocialMedia> socialMedia, IUnitOfWork unitOfWork, IRepository<AccessDetails> accessDetail)
       public InstagramServices(IRepository<SocialMedia> socialMedia, IUnitOfWork unitOfWork, IRepository<AccessDetails> accessDetail, IUserService userService, IRepository<SuperTargetUser> targetUserRepo, IRepository<Tags> tagRepo)
       {
           _socialMedia = socialMedia;
           _unitOfWork = unitOfWork;
           _accessDetail = accessDetail;
           _userService = userService;
           _targetUserRepo = targetUserRepo;
           _tagRepo = tagRepo;

           consumerKey = ConfigurationSettings.AppSettings["InstaID"];
           consumerSecret = ConfigurationSettings.AppSettings["InstaSecret"];
           redirecturl = ConfigurationSettings.AppSettings["InstaRedirectURL"];
           ApiURL = "https://api.instagram.com/";
      }

       public InstagramServices(string consumerKey, string consumerSecret)
       {
           // TODO: Complete member initialization
           this.consumerKey = consumerKey;
           this.consumerSecret = consumerSecret;
       }

       /// <summary>
       /// Method to get the uri for authorizing user in Instagram
       /// </summary>
       /// <returns></returns>                                                                 
       public string Authorize()
       {
           try
           {
               var request = new RestRequest("oauth/authorize", Method.GET);
               request.AddHeader("Content-Type", "application/json");
               request.AddParameter("scope", "public_content basic follower_list comments relationships likes");
               request.AddParameter("client_id", consumerKey);
               request.AddParameter("redirect_uri", redirecturl);
               request.AddParameter("response_type", "Code");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
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
       public OAuthTokens GetToken(string Code)
       {

          
           var request = new RestRequest("oauth/access_token", Method.POST);
           request.AddHeader("Content-Type", "application/json");
           //request.AddHeader("Authorization", "OAuth " + token);
           request.AddParameter("client_id", consumerKey);
           request.AddParameter("client_secret", consumerSecret);
           request.AddParameter("code", Code);
           request.AddParameter("redirect_uri", redirecturl);
           request.AddParameter("grant_type", "authorization_code");
           var response = WebServiceHelper.WebRequest(request, ApiURL);
           JsonDeserializer deserial = new JsonDeserializer();
           OAuthTokens result = deserial.Deserialize<OAuthTokens>(response);
           return result;
          
       }

       public bool SaveAccountDeatils(OAuthTokens tokens, string userId,string Email)
       {
           try
           {
               AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
               InstaUser profile = GetUserprofile(accessToken);
               SocialMedia socialDetails = new SocialMedia()
               {
                   UserId = userId,
                   Provider = SocialMediaProviders.Instagram.ToString(),
                   AccessDetails = new AccessDetails { AccessToken = tokens.access_token },
                   ProfilepicUrl = profile.data.profile_picture,
                   SMId = profile.data.id.ToString(),
                   Status = true,
                   UserName = profile.data.username,
<<<<<<< .mine
                   Followers = profile.data.counts.follows

               };
=======
                   Followers = profile.data.Followers,
                   AccSettings = new AccSettings()
                 };
               socialDetails.AccSettings.UserManagement.Add(new UserManagement { Email = Email, userId = userId, Role = "Owner" });
>>>>>>> .r181
               _socialMedia.Add(socialDetails);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false; 
           }
       }

       public InstaUser GetUserprofile(AccessDetails Token)
       {
           try
           {
               var request = new RestRequest("/v1/users/self", Method.GET);
               request.AddParameter("format", "json");
               request.AddParameter("access_token", Token.AccessToken);
               //request.AddParameter("{user-id}", "id,username,profile_picture,full_name,bio,Follows");
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               InstaUser result = deserial.Deserialize<InstaUser>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public bool RefreshToken(int socialId)
       {
           try
           {
               var acc = _socialMedia.Get().Include(x => x.AccessDetails).Where(x => x.Id == socialId).FirstOrDefault();
               var request = new RestRequest("api/token", Method.POST);
               //+ ":" +System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(consumerSecret))
               request.AddHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(consumerKey + ":" + consumerSecret)));
               request.AddParameter("grant_type", "refresh_token");
               request.AddParameter("refresh_token", acc.AccessDetails.Refresh_token);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<OAuthTokens>(response);
               acc.AccessDetails.AccessToken = result.access_token;
               acc.AccessDetails.Expires_in = DateTime.UtcNow.AddMinutes(55);
               _unitOfWork.Commit();
               //acc.AccessDetails.Refresh_token = result.refresh_token;
               return true;
           }
           catch (Exception)
           {
               return false;
               throw;
           }

       }


       public IEnumerable<InstaUserList> SearchUser(string query, AccessDetails Token)
       {
               try
               {
                   var request = new RestRequest("v1/users/search", Method.GET);
                   request.AddParameter("format", "json");
                   request.AddHeader("access_token", Token.AccessToken);
                   request.AddParameter("q", query);
                   request.AddParameter("access_token", Token.AccessToken);
                   //request.AddParameter("{user-id}", "id,username,profile_picture,full_name,bio,counts.follows");
                   var response = WebServiceHelper.WebRequest(request, ApiURL);
                   JsonDeserializer deserial = new JsonDeserializer();
                   var result = deserial.Deserialize<InstaSearchUserList>(response);
                   return result.data;
               }
               catch (Exception)
               {
                   throw;
               }
       }

       public InstaUser LikeUser(AccessDetails Token)
       {
           try
           {
               var request = new RestRequest("/v1/media/{media-id}/likes", Method.POST);
               request.AddHeader("access_token", Token.AccessToken);
               request.AddParameter("access_token", Token.AccessToken);
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<InstaUser>(response);
               return result;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public List<SuperTargetUserVM> GetAllTargetUser(int socialId)
       {
           try
           {
               var acc = _socialMedia.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.SuperTargetUser).Where(x => x.Id == socialId).FirstOrDefault();
               List<SuperTargetUserVM> targetUserList = new List<SuperTargetUserVM>();
               if (acc.AccSettings != null) {
                   foreach (var item in acc.AccSettings.SuperTargetUser)
                   {
                       targetUserList.Add(new SuperTargetUserVM { Id = item.Id, IsBlocked = item.IsBlocked, SMId = item.SMId, SocailId = socialId, Followers = item.Followers, UserName = item.UserName });
                   }
               }
               return targetUserList;
           }
           catch (Exception)
           {

               throw;
           }
       }

       public bool AddBlockSuperTargetUser(SuperTargetUserVM user)
       {
           try
           {
               var acc = _socialMedia.Get().Include(x => x.AccSettings).Where(x => x.Id == user.SocailId).FirstOrDefault();
               if (acc.AccSettings == null)
               {
                   acc.AccSettings = new AccSettings();
               }

               acc.AccSettings.SuperTargetUser.Add(new SuperTargetUser { IsBlocked = user.IsBlocked, UserName = user.UserName, SMId = user.SMId, Followers = user.Followers });
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
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



       public bool AddLocationToTag(int tagId, string location)
       {
           try
           {
               var tag = _tagRepo.Get().Where(x => x.Id == tagId).FirstOrDefault();
               tag.Location = location;
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       public bool RemoveLocation(int tagId)
       {
           try
           {
               var tag = _tagRepo.Get().Where(x => x.Id == tagId).FirstOrDefault();
               tag.Location = null;
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }


       public async Task<IEnumerable<InstaSearchTag>> SearchTags(AccessDetails Token, string tagname)
       {
           try
           {
               InstagramServices service = new InstagramServices(consumerKey, consumerSecret);
               string post=await WaitSynchronously();
               var request = new RestRequest("v1/tags/" + tagname + "/media/recent", Method.GET);
               request.AddHeader("access_token", Token.AccessToken);
               request.AddParameter("access_token", Token.AccessToken);
               
               var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               var result = deserial.Deserialize<InstaSearchTagList>(response);
               return result.data;
           }
           catch (Exception)
           {
               throw;
           }
       }


       public async Task<string> WaitSynchronously()
       {
           // Add a using directive for System.Threading.
           await Task.Delay(0);
           return null;

          

       }


    }
}

