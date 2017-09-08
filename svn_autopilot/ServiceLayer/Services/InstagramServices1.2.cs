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
using ServiceLayer.EnumStore;
using ServiceLayer.Interfaces;
using System.Data.Entity;
using System.Collections;


namespace ServiceLayer.Services
{

    public class InstagramServices : IInstagramServices
    {
        private readonly IRepository<SocialMedia> _socialMedia;
        private readonly IRepository<AccessDetails> _accessDetail;
        private readonly IRepository<SuperTargetUser> _targetUserRepo;
        private readonly IRepository<Tags> _tagRepo;
        private readonly IRepository<Activities> _activityRepo;
        private readonly IRepository<FollowersGraph> _followersGraphRepo;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string consumerKey;
        private readonly string consumerSecret;
        private readonly string redirecturl;
        private readonly string ApiURL;
        //private readonly TwitterService service;

        // public InstagramServices(IRepository<SocialMedia> socialMedia, IUnitOfWork unitOfWork, IRepository<AccessDetails> accessDetail)
        public InstagramServices(IRepository<SocialMedia> socialMedia, IUnitOfWork unitOfWork, IRepository<AccessDetails> accessDetail, IRepository<Activities> activityRepo,
            IUserService userService, IRepository<SuperTargetUser> targetUserRepo, IRepository<Tags> tagRepo, IRepository<FollowersGraph> followersGraphRepo)
        {
            _socialMedia = socialMedia;
            _unitOfWork = unitOfWork;
            _accessDetail = accessDetail;
            _activityRepo = activityRepo;
            _userService = userService;
            _targetUserRepo = targetUserRepo;
            _tagRepo = tagRepo;
            _followersGraphRepo = followersGraphRepo;

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

        public AccessDetails TokenValidate(int socialId, AccessDetails token)
        {
            try
            {
                if (token.Expires_in <= DateTime.UtcNow)
                {
                    var newToken = RefreshToken(socialId);
                    return newToken;
                }
                else
                {
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
       

        public bool SaveAccountDeatils(OAuthTokens tokens, string userId, string Email)
        {
            try
            {
                AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
                //InstaUser profile = GetUserprofile(accessToken);
                InstaUserList profile = GetUserprofile(accessToken);
                SocialMedia socialDetails = new SocialMedia()
                {
                    UserId = userId,
                    Provider = SocialMediaProviders.Instagram.ToString(),
                    AccessDetails = new AccessDetails { AccessToken = tokens.access_token },
                    ProfilepicUrl = profile.profile_picture,
                    SMId = profile.id.ToString(),
                    Status = true,
                    UserName = profile.username,
                    Followers = profile.counts.followed_by,
                    AccSettings = new AccSettings()
                };
                socialDetails.AccSettings.UserManagement.Add(new UserManagement { Email = Email, userId = userId, Role = "Owner" });
                _socialMedia.Add(socialDetails);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateProfile(AccessDetails tokens, int socialId)
        {
            try
            {
                var updatedData = GetUserprofile(tokens);
                var account = _socialMedia.Get().Where(x => x.Id == socialId).FirstOrDefault();
                account.Followers = updatedData.counts.followed_by;
                account.ProfilepicUrl = updatedData.profile_picture;
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public InstaUserList GetUserprofile(AccessDetails Token)
        {
            try
            {

                var request = new RestRequest("/v1/users/self", Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);

                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                InstaUserProfile result = deserial.Deserialize<InstaUserProfile>(response);
                return result.data;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public InstaUserList GetUserprofile(AccessDetails Token, string userId)
        {
            try
            {
                var request = new RestRequest("/v1/users/" + userId + "", Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);

                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                InstaUserProfile result = deserial.Deserialize<InstaUserProfile>(response);
                return result.data;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<InstaUserList> GetUserFollower(long SMId,AccessDetails Token)
        {
            try
            {
                var request = new RestRequest("v1/users/self/follows", Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);

                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                InstaUserFollow result = deserial.Deserialize<InstaUserFollow>(response);
                return result.data;
            }
            catch (Exception)
            {
                throw;
            }
        }

       
        public IEnumerable<InstaUserList> SearchUser(string query, AccessDetails Token)
        {
            try
            {
                var request = new RestRequest("v1/users/search", Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);
                request.AddParameter("q", query);
                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                var result = deserial.Deserialize<InstaSearchUserList>(response);
                foreach (var item in result.data)
                {

                    item.counts.followed_by = GetUserprofile(Token, item.id.ToString()).counts.followed_by;
                    item.counts.media = GetUserprofile(Token, item.id.ToString()).counts.media;
                }
                return result.data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public InstaUserList LikePost(AccessDetails Token, string mediaid)
        //{
        //    try
        //    {
        //        var request = new RestRequest("/v1/media/" + mediaid + "/likes", Method.POST);
        //        request.AddHeader("access_token", Token.AccessToken);
        //        request.AddParameter("access_token", Token.AccessToken);
        //        var response = WebServiceHelper.WebRequest(request, ApiURL);
        //        JsonDeserializer deserial = new JsonDeserializer();
        //        var result = deserial.Deserialize<InstaUserList>(response);
        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public bool PostLike(AccessDetails Token, string mediaid)
        {
            try
            {
                var request = new RestRequest("/v1/media/" + mediaid + "/likes", Method.POST);
                request.AddHeader("access_token", Token.AccessToken);
                request.AddParameter("access_token", Token.AccessToken);
                var response = WebServiceHelper.WebRequest(request, ApiURL);

                if (response.StatusCode.ToString() == "OK")
                {
                    //JsonDeserializer deserial = new JsonDeserializer();
                    //var result = deserial.Deserialize<>(response);
                    //return result;
                    return true;
                }

                else
                {
                    return false;
                }

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
                if (acc.AccSettings != null)
                {
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
                var request = new RestRequest("v1/tags/" + tagname + "/media/recent", Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);
                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                var result = deserial.Deserialize<InstaSearchTagList>(response);
                return result.data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CheckForBlockedUser(List<SuperTargetUser> targetUsers, InstaUserProfile user)
        {
            try
            {
                bool status = false;
                foreach (var item in targetUsers)
                {
                    if (item.IsBlocked == true && item.UserName == user.data.username)
                    {
                        status = true;
                    }
                }
                return status;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool CheckForBlockedTag(List<Tags> tags, string TagName)
        {
            try
            {
                bool status = false;

                foreach (var item in tags)
                {
                    if (item.IsBlocked == true && item.TagName == TagName)
                    {
                        status = true;
                    }
                }
                return status;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<location> Location(string query, AccessDetails Token)
        {
            try
            {
                var request = new RestRequest("v1/locations/" + 1, Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);

                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                var result = deserial.Deserialize<location>(response);
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void scheduleAlgo(SocialMediaVM acc)
        {
            List<Tags> tags = _tagRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
            List<SuperTargetUser> targetUser = _targetUserRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
            SearchPostUsingTag(tags, acc);
            SuperTargeting(targetUser, acc, tags);
            UpdateProfile(acc.AccessDetails, acc.Id);
            _userService.SaveFollowersCount(acc.Id, GetUserprofile(acc.AccessDetails).counts.followed_by);
            CheckForConversion(acc);
        }

        public void CheckForConversion(SocialMediaVM acc)
        {
            try
            {
                var accountActivity = _activityRepo.Get().Where(x => x.socialId == acc.Id).ToList();
                var followers = GetUserFollower(long.Parse(acc.SMId), acc.AccessDetails);
                foreach (var item in accountActivity)
                {
                    foreach (var follower in followers)
                    {
                        if (follower.username == item.Username && _userService.CheckIfUserConvert(item.SMId, item.socialId) == false)
                        {
                            _userService.SaveConversion(new Conversions { SMId = acc.SMId, socialId = acc.Id, ConvertDate = DateTime.UtcNow, Username = item.Username, Tag = item.Tag });
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

     
        #region Alog methods
        public void SearchPostUsingTag(List<Tags> tags, SocialMediaVM acc)
        {
            try
            {
                if (tags.Count > 0)
                {
                    foreach (var item in tags)
                    {
<<<<<<< .mine
                        if (item.IsBlocked == false)
=======
                        if (item.TagName != string.Empty)
>>>>>>> .r276
                        {
                            if (item.TagName != "")
                            {
<<<<<<< .mine
                                var checkNegative = CheckForBlockedTag(tags, item.TagName);
                                if (checkNegative != true)
=======
                                var resList = SearchTags(acc.AccessDetails, item.TagName).Result.ToList();
                               // var resList = instaPost.Result.ToList();
                                foreach (var res in resList)
>>>>>>> .r276
                                {
                                    var instaPost = SearchTags(acc.AccessDetails, item.TagName);
                                    var resList = instaPost.Result.ToList();
                                    foreach (var res in resList)
                                    {
<<<<<<< .mine
                                        string id = res.Id;
                                        string name = res.user.username;
                                        if (res.user_has_liked == false)
                                        {
                                            if (PostLike(acc.AccessDetails, id) == true)
                                                _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, Tag = "#" + item.TagName, PostId = id.ToString(), Username = name, ActivityDate = DateTime.UtcNow });
                                        }
=======
                                        if (PostLike(acc.AccessDetails, id) == true) {
                                            _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, Tag = "#" + item.TagName, PostId = id.ToString(), Username = name, ActivityDate = DateTime.UtcNow });
>>>>>>> .r276
                                        
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

        public void SuperTargeting(List<SuperTargetUser> targetUsers, SocialMediaVM acc, List<Tags> tags)
        {
            try
            {
                foreach (var item in tags)
                {
                    foreach (var user in targetUsers)
                    {
                        var followers = SearchTags(acc.AccessDetails, item.TagName);
                        var resList = followers.Result.ToList();
                        foreach (var res in resList)
                        {
                            string id = res.Id;
                            string name = res.user.username;
                            if (PostLike(acc.AccessDetails, id) == true)
                            {
                                _userService.SaveActivity(new Activities { Activity = Activity.Follow.ToString(), SMId = acc.SMId, socialId = acc.Id, PostId = id.ToString(), Tag = "@" + user.UserName, Username = name, ActivityDate = DateTime.UtcNow });
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

        public List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken)
        {
            try
            {
                var DateLimit = DateTime.UtcNow.AddDays(-2);
                var activity = _activityRepo.Get().Where(x => x.socialId == socialId && x.ActivityDate >= DateLimit).OrderByDescending(x => x.ActivityDate).Take(10).ToList();
                List<RecentActivityViewModel> activityDetails = new List<RecentActivityViewModel>();

                foreach (var item in activity)
                {
                    
                    var artist = GetUserFollower(Convert.ToInt64(item.SMId),_accessToken);
                   // activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), Tag = item.Tag, ProfilePic =prop.Count() > 0 ? artist.images[0].url : "" });
                }
                return null;

            }
            catch (Exception)
            {

                throw;
            }
        }


        #endregion


    }
}

