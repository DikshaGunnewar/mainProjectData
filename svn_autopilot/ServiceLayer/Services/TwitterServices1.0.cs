using RepositoryLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;
using System.Configuration;
using ServiceLayer.Interfaces;
using RepositoryLayer.Repositories;
using EntitiesLayer.Entities;
using ServiceLayer.Enum;
using System.Data.Entity;
using System.Linq.Expressions;
using EntitiesLayer.ViewModel;


namespace ServiceLayer.Services
{
    public class TwitterServices:ITwitterServices
    {

       private readonly IRepository<SocialMedia> _socialMediaRepo;
       private readonly IRepository<AccessDetails> _accessDetailRepo;
       private readonly IRepository<SuperTargetUser> _targetUserRepo;
       private readonly IRepository<Activities> _activityRepo;
       private readonly IRepository<SuperTargetUser> _superTargetUserRepo;
       private readonly IRepository<FollowersGraph> _followersGraphRepo;

       private readonly IRepository<Tags> _tagRepo;

       private readonly IUserService _userService;

       private readonly IUnitOfWork _unitOfWork;
       private readonly string consumerKey;
       private readonly string consumerSecret;

       public TwitterServices(IRepository<SocialMedia> socialMedia,
           IUnitOfWork unitOfWork, 
           IRepository<AccessDetails> accessDetail, 
           IRepository<SuperTargetUser> targetUserRepo,
           IRepository<Activities> activityRepo, IUserService userService,IRepository<Tags> tagRepo,
           IRepository<SuperTargetUser> superTargetUserRepo, IRepository<FollowersGraph> followersGraphRepo)
       {
           _socialMediaRepo = socialMedia;
           _unitOfWork = unitOfWork;
           _targetUserRepo = targetUserRepo;
           _accessDetailRepo = accessDetail;
           _activityRepo = activityRepo;
           _superTargetUserRepo = superTargetUserRepo;
           _tagRepo = tagRepo;
           _followersGraphRepo = followersGraphRepo;
           _userService = userService;
           consumerKey = ConfigurationSettings.AppSettings["twitterConsumerKey"];
           consumerSecret = ConfigurationSettings.AppSettings["twitterConsumerSecret"];
          
      }
        /// <summary>
        /// Method to get the uri for authorizing user in Twitter
        /// </summary>
        /// <returns></returns>
       public string Authorize() {
           try
           {
               var url = ConfigurationSettings.AppSettings["BaseURL"];
               // Step 1 - Retrieve an OAuth Request Token
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
           //OAuthAccessToken access = service.GetAccessTokenWithXAuth("", "");
           // This is the registered callback URL
          
           OAuthRequestToken requestToken = service.GetRequestToken(url+"/Twitter/TwitterAuthCallback");

           //// Step 2 - Redirect to the OAuth Authorization URL
           Uri uri = service.GetAuthorizationUri(requestToken);
           return uri.ToString();
           }
           catch (Exception)
           {
               
               throw;
           }
           
       }
        /// <summary>
        /// Method to get OAuth token for authenticate user
        /// </summary>
        /// <param name="oauth_token"></param>
        /// <param name="oauth_verifier"></param>
        /// <returns></returns>
       public OAuthAccessToken GetTokensOAuth(string oauth_token, string oauth_verifier)
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               var requestToken = new OAuthRequestToken { Token = oauth_token };
               OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);
               return accessToken;
           }
           catch (Exception)
           {

               throw;
           }

       }

       public bool SaveAccountDeatils(OAuthAccessToken tokens, string userId,string Email)
       {
           try
           {
               AccessDetails accessTokens = new AccessDetails() {AccessToken= tokens.Token,AccessTokenSecret = tokens.TokenSecret };
               TwitterUser profile = GetUserprofile(accessTokens);
               
               SocialMedia socialDetails = new SocialMedia()
                 {
                     UserId = userId,
                     Provider = SocialMediaProviders.Twitter.ToString(),
                     AccessDetails = new AccessDetails { AccessToken = tokens.Token, AccessTokenSecret = tokens.TokenSecret },
                     ProfilepicUrl = profile.ProfileImageUrlHttps,
                     Followers = profile.FollowersCount,
                     SMId = profile.Id.ToString(),
                     Status = true,
                     UserName = profile.ScreenName,
                     AccSettings = new AccSettings()
                 };
               socialDetails.AccSettings.UserManagement.Add(new UserManagement { Email = Email, userId = userId, Role = "Owner" });
               _socialMediaRepo.Add(socialDetails);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       public bool UpdateProfile(AccessDetails tokens, int socialId) {
           try
           {
               var updatedData = GetUserprofile(tokens);
               var account = _socialMediaRepo.Get().Where(x => x.Id == socialId).FirstOrDefault();
               account.Followers = updatedData.FollowersCount;
               account.ProfilepicUrl = updatedData.ProfileImageUrlHttps;
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       
       }

       public TwitterUser GetUserprofile(AccessDetails accessToken)
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(accessToken.AccessToken, accessToken.AccessTokenSecret);
               var profile = service.GetUserProfile(new GetUserProfileOptions { IncludeEmail = true, IncludeEntities = true });
               return profile;
           }
           catch (Exception)
           {

               throw;
           }
       }

       //public bool GetStatus(string userId) {
       //    TwitterService service = new TwitterService(consumerKey, consumerSecret);
       //    var accessToken = _socialMedia.Get().Where(x=>x.UserId==userId).Include(x=>x.AccessDetails).FirstOrDefault();
       //    service.AuthenticateWith(accessToken.AccessDetails.AccessToken, accessToken.AccessDetails.AccessTokenSecret);
       //    var tweets = service.Search(new SearchOptions { Q = "metro", Count = 5 }).Statuses;
       //    return true;
       //}

       public bool LikeTweet(long tweetId,  AccessDetails _accessToken)
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               var result = service.FavoriteTweet(new FavoriteTweetOptions { Id = tweetId });
               return result.IsFavorited;
           }
           catch (Exception)
           {
               return false;
           }
       }

       public IEnumerable<TwitterUser> SearchUser(string query, AccessDetails _accessToken)
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               var users = service.SearchForUser(new SearchForUserOptions{ Q = query, Count = 12 });
               return users;
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
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x=>x.AccSettings.SuperTargetUser).Where(x => x.Id == user.SocailId).FirstOrDefault();
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
               else {
                   return false;
               }

               
           }
           catch (Exception)
           {
               return false;
           }

       }

       public List<SuperTargetUserVM> GetAllTargetUser(int socialId) {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.SuperTargetUser).Where(x => x.Id == socialId).FirstOrDefault();
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

       public async Task<TwitterAsyncResult<TwitterSearchResult>> SearchTweet(string query,string lang,string geoLocation, AccessDetails _accessToken)
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               TwitterAsyncResult<TwitterSearchResult> posts;
               if (geoLocation != null) {
                  var coordinates = _userService.AddressToCoordinates(geoLocation);
                   posts = await service.SearchAsync(new SearchOptions { Q = query, Count = 5, Lang = lang, Resulttype = TwitterSearchResultType.Recent, Geocode = new TwitterGeoLocationSearch { Coordinates = new TwitterGeoLocation.GeoCoordinates { Latitude = coordinates.latitude, Longitude = coordinates.longitude }, Radius = 500, UnitOfMeasurement = TweetSharp.TwitterGeoLocationSearch.RadiusType.Km } });

               }
               else {
                    posts = await service.SearchAsync(new SearchOptions { Q = query, Count = 5, Lang = lang, Resulttype = TwitterSearchResultType.Recent });
               
               }
               
               return posts;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public TwitterCursorList<TwitterUser> GetFollowersList(long SMId, AccessDetails _accessToken) {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               var followers = service.ListFollowers(new ListFollowersOptions { UserId = SMId });
               return followers;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public IEnumerable<TwitterStatus> ListPostOnUserTimeline(long userId, AccessDetails _accessToken) 
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               var posts = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { UserId = userId, Count = 5 });
               return posts;
           }
           catch (Exception)
           {

               throw;
           }
       
       }

       public TwitterStatus GetTweet(long tweetId, AccessDetails _accessToken)
       {
           try
           {
                 TwitterService service = new TwitterService(consumerKey, consumerSecret);
                 service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
                 var tweet = service.GetTweet(new GetTweetOptions { Id = tweetId });
                 return tweet;
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
               var DateLimit =DateTime.UtcNow.AddDays(-2);
               var activity = _activityRepo.Get().Where(x => x.socialId == socialId && x.ActivityDate >= DateLimit).OrderByDescending(x => x.ActivityDate).Take(10).ToList();
               List<RecentActivityViewModel> activityDetails = new List<RecentActivityViewModel>();

               foreach (var item in activity)
               {
                   var tweetDetail = GetTweet(long.Parse(item.PostId), _accessToken);
                   activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity,ActivityDate=item.ActivityDate.ToLocalTime(),tweet= tweetDetail,Tag=item.Tag});
               }
               return activityDetails;

           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public TwitterCursorList<TwitterUser> ListFollowerOfTargetUser(long targetId, AccessDetails _accessToken) {
           try
           {
                TwitterService service = new TwitterService(consumerKey, consumerSecret);
                service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
                var followers = service.ListFollowers(new ListFollowersOptions { UserId = targetId });
                return followers;

           }
           catch (Exception)
           {
               
               throw;
           }
       }

       public bool AddLocationToTag(int tagId, string location) {
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
       
       public async void scheduleAlgo(SocialMediaVM acc)
       {
           List<Tags> tags = _tagRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
           List<SuperTargetUser> targetUser = _superTargetUserRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
           SearchPostUsingTag(tags, acc);
           SuperTargeting(targetUser, acc);
           UpdateProfile(acc.AccessDetails, acc.Id);
           SaveFollowersCount(acc.Id, GetUserprofile(acc.AccessDetails).FollowersCount);
       }

       #region Alogrithm Method
       public void SuperTargeting(List<SuperTargetUser> targetUsers, SocialMediaVM acc)
       {
           try
           {
               foreach (var user in targetUsers)
               {
                   var followers = ListFollowerOfTargetUser(long.Parse(user.SMId), acc.AccessDetails);
                   if (followers != null) {
                       foreach (var follower in followers)
                       {
                         
                           var checkForBlockedUser = CheckForBlockedUser(targetUsers, follower);
                           if (checkForBlockedUser != true)
                           {
                               var tweets = ListPostOnUserTimeline(follower.Id, acc.AccessDetails);
                               if (tweets.Count() != 0)
                               {

                                   foreach (var tweet in tweets)
                                   {
                                       //var checkNegative = CheckForNegativeTag(tags, tweet);&& checkNegative != true
                                       if (tweet.IsFavorited != true)
                                       {
                                           var likeStatus = LikeTweet(tweet.Id, acc.AccessDetails);
                                           if (likeStatus == true)
                                           {
                                               _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, PostId = tweet.Id.ToString(), Tag = "@" + user.UserName, Username = tweet.User.ScreenName, ActivityDate = DateTime.UtcNow });
                                           }

                                       }
                                   }
                               }
                           }//check for negative if end


                       }
                   }
               }
           }
           catch (Exception)
           {

               throw;
           }

       }

       public bool CheckForBlockedUser(List<SuperTargetUser> targetUsers, TwitterUser user)
       {
           try
           {
               bool status = false;
               foreach (var item in targetUsers)
               {
                   if (item.IsBlocked == true && item.UserName == user.ScreenName)
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

       public async void SearchPostUsingTag(List<Tags> tags, SocialMediaVM acc)
       {
           try
           {
               foreach (var item in tags)
               {
                   if (item.IsBlocked == false)
                   {
<<<<<<< .mine
                       var tweets = await SearchTweet(item.TagName, "en", item.Location, acc.AccessDetails);

                       foreach (var tweet in tweets.Value.Statuses)
=======
                      
                       List<string> langCode = new List<string>();
                       if (acc.AccSettings.Language != null && acc.AccSettings.Language!= string.Empty)
>>>>>>> .r227
                       {
                           var languages = acc.AccSettings.Language.Split(',');
                           for (int i = 0; i < languages.Length; i++)
                           {
                               langCode.Add(_userService.ReturnLanguageCode(languages[i]));
                           }
                       }
                       if (langCode.Count() == 0) {
                           langCode.Add("en");
                       }
                       foreach (var lang in langCode)
                       {
                           var tweets = await SearchTweet(item.TagName, lang, item.Location, acc.AccessDetails);
                           if (tweets != null)
                           {
                               foreach (var tweet in tweets.Value.Statuses)
                               {
                                   var checkNegative = CheckForNegativeTag(tags, tweet);
                                   if (tweet.IsFavorited != true && checkNegative != true)
                                   {
                                       var result = LikeTweet(tweet.Id, acc.AccessDetails);
                                       if (result == true)
                                       {
                                           _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, PostId = tweet.Id.ToString(), Tag = "#" + item.TagName, Username = tweet.User.ScreenName, ActivityDate = DateTime.UtcNow });
                                       }
                                   }
                                   //System.Threading.Thread.Sleep(10000);
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

       private bool CheckForNegativeTag(List<Tags> tags, TwitterStatus tweet)
       {
           try
           {
               bool status = false;
               foreach (var item in tags)
               {
                   if (item.IsBlocked == true)
                   {
                       status = tweet.TextDecoded.Contains(item.TagName);
                   }
               }
               return status;
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
               var followers = GetFollowersList(long.Parse(acc.SMId), acc.AccessDetails);
               foreach (var item in accountActivity)
               {
                   foreach (var follower in followers)
                   {
                       if (follower.ScreenName == item.Username && _userService.CheckIfUserConvert(item.SMId,item.socialId)== false)
                       {
                           _userService.SaveConversion(new Conversions { SMId = acc.SMId, socialId = acc.Id, ConvertDate = DateTime.UtcNow, Username = item.Username,Tag= item.Tag });
                       }
                   }
               }

           }
           catch (Exception)
           {

               throw;
           }
       }

       public bool SaveFollowersCount(int socialId,int followerCount) 
       {
           try
           {
               _followersGraphRepo.Add(new FollowersGraph { Date = DateTime.UtcNow, Followers = followerCount, SocialId = socialId });
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;

           }
       }
        #endregion
    }
}
