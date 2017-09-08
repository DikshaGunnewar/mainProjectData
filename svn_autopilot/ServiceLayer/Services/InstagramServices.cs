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
        /// Method to logout user in Instagram
        /// </summary>
        /// <returns></returns>                                                                 
        public bool Logout()
        {
            try
            {
                var request = new RestRequest("accounts/logout", Method.GET);
                var response = WebServiceHelper.WebRequest(request, "http://instagram.com/");
                return true;
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
        /// <summary>
        /// Method to save account details of authorize user in instagram
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="userId"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
        public string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email)
        {
            try
            {
                AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
                var returnMessage = string.Empty;
                InstaUserList profile = GetUserprofile(accessToken);
                var checkAccountIsAvail = _socialMedia.Get().Include(x => x.AccessDetails).Where(x => x.SMId == profile.id.ToString() && x.IsDeleted == false).FirstOrDefault();
                if (checkAccountIsAvail == null)
                {
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
                    returnMessage = "Account added successfully.";
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
        /// Method to update user profile 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="socialId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to get user profile using access token of authenticate user
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public InstaUserList GetUserprofile(AccessDetails Token)
        {
            try
            {

                var request = new RestRequest("/v1/users/self", Method.GET);
                request.AddParameter("format", "json");
                request.AddParameter("access_token", Token.AccessToken);

                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                var userAuth = WebServiceHelper.CheckTokenInValid(response);
                InstaUserProfile result = new InstaUserProfile() { data = new InstaUserList() };
                if (userAuth == true)
                {
                    result = deserial.Deserialize<InstaUserProfile>(response);
                }
                else
                {
                    result.data.IsAccountValid = false;

                }
                return result.data;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Method to get user profile using access token and userId 
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to get own follower list of instagram user
        /// </summary>
        /// <param name="SMId"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public List<InstaUserList> GetUserFollower(long SMId, AccessDetails Token)
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
        /// <summary>
        /// Method to validate token
        /// </summary>
        /// <param name="socialId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method for refresh token 
        /// </summary>
        /// <param name="socialId"></param>
        /// <returns></returns>
        public AccessDetails RefreshToken(int socialId)
        {
            try
            {
                var acc = _socialMedia.Get().Include(x => x.AccessDetails).Where(x => x.Id == socialId).FirstOrDefault();
                var request = new RestRequest("api/token", Method.POST);
                request.AddHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(consumerKey + ":" + consumerSecret)));
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("refresh_token", acc.AccessDetails.Refresh_token);
                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                var result = deserial.Deserialize<OAuthTokens>(response);
                acc.AccessDetails.AccessToken = result.access_token;
                acc.AccessDetails.Expires_in = DateTime.UtcNow.AddMinutes(58);
                _unitOfWork.Commit();
                return acc.AccessDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method to get list of search user 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method for autolike post for instagram user
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="mediaid"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to get list of all target user
        /// </summary>
        /// <param name="socialId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to add block super target user 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
        /// <summary>
        ///Method for remove super target user 
        /// </summary>
        /// <param name="targetUserId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method for to add location 
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="location"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to remove location
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to search tag using tag name 
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="tagname"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to check for blocked user
        /// </summary>
        /// <param name="targetUsers"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to check for blocked tag
        ///  </summary>
        /// <param name="tags"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Method to Search location using lat/long
        /// </summary>
        /// <param name="query"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public IEnumerable<Location> Location(AccessDetails Token, double Lat, double Lng)
        {
            try
            {
                var request = new RestRequest("v1/locations/search?", Method.GET);
                request.AddParameter("lat", Lat);
                request.AddParameter("lng", Lng);
                request.AddParameter("access_token", Token.AccessToken);
                var response = WebServiceHelper.WebRequest(request, ApiURL);
                JsonDeserializer deserial = new JsonDeserializer();
                var result = deserial.Deserialize<InstaUserLocation>(response);
                return result.data;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method to get algo for tags and super target user for sandbox user
        /// </summary>
        /// <param name="acc"></param>
        public void scheduleAlgo(SocialMediaVM acc)
        {
            try
            {
                if (GetUserprofile(acc.AccessDetails).IsAccountValid == true)
                {
                    List<Tags> tags = _tagRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
                    List<SuperTargetUser> targetUser = _targetUserRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
                    SearchPostUsingTag(tags, acc);
                    UpdateProfile(acc.AccessDetails, acc.Id);
                    _userService.SaveFollowersCount(acc.Id, GetUserprofile(acc.AccessDetails).counts.followed_by);
                    CheckForConversion(acc);
                }
                else
                {
                    var accountDetails = _socialMedia.Get().Where(x => x.Id == acc.Id).FirstOrDefault();
                    accountDetails.IsInvalid = true;
                    accountDetails.Status = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Method for to check user conversion 
        /// </summary>
        /// <param name="acc"></param>
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

        /// <summary>
        /// Method for search post using tag name 
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="acc"></param>
        public void SearchPostUsingTag(List<Tags> tags, SocialMediaVM acc)
        {
            try
            {
                if (tags.Count() > 0)
                {
                    foreach (var item in tags)
                    {
                        if (item.IsBlocked == false)
                        {
                            if (item.TagName != string.Empty)
                            {
                                var checkNegative = CheckForBlockedTag(tags, item.TagName);
                                if (checkNegative != true)
                                {
                                    var resList = SearchTags(acc.AccessDetails, item.TagName).Result.ToList();
                                    if (resList.Count() > 0)
                                    {
                                        foreach (var res in resList)
                                        {
                                            string id = res.Id;
                                            string name = res.user.username;
                                            if (res.user_has_liked == false)
                                            {
                                                if (PostLike(acc.AccessDetails, id) == true)
                                                {
                                                    _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, Tag = "#" + item.TagName, PostId = id.ToString(), Username = name, ActivityDate = DateTime.UtcNow });
                                                }
                                            }
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
        /// <summary>
        /// Method for search post using super target user
        /// </summary>
        /// <param name="targetUsers"></param>
        /// <param name="acc"></param>
        /// <param name="tags"></param>
        public void SuperTargeting(List<SuperTargetUser> targetUsers, SocialMediaVM acc, List<Tags> tags)
        {
            try
            {
                if (targetUsers.Count() > 0)
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
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Method to get recent activity 
        /// </summary>
        /// <param name="socialId"></param>
        /// <param name="_accessToken"></param>
        /// <returns></returns>
        public List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken)
        {
            try
            {
                var DateLimit = DateTime.UtcNow.AddDays(-2);
                var activity = _activityRepo.Get().Where(x => x.socialId == socialId && x.ActivityDate >= DateLimit).OrderByDescending(x => x.ActivityDate).Take(10).ToList();
                List<RecentActivityViewModel> activityDetails = new List<RecentActivityViewModel>();
                foreach (var item in activity)
                {
                    var follow = GetUserFollower(Convert.ToInt64(item.SMId), _accessToken);
                    activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), Tag = item.Tag });
                }
                return activityDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}

