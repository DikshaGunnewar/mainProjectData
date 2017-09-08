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
using ServiceLayer.EnumStore;
using System.Data.Entity;
using System.Linq.Expressions;
using EntitiesLayer.ViewModel;
using ServiceLayer.Helper;

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

        /// <summary>
        /// Save accounts details like token,followers count & other details
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="userId"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
       public string SaveAccountDeatils(OAuthAccessToken tokens, string userId,string Email)
       {
           try
           {
               AccessDetails accessTokens = new AccessDetails() {AccessToken= tokens.Token,AccessTokenSecret = tokens.TokenSecret };
               var returnMessage = string.Empty;
               TwitterUser profile = GetUserprofile(accessTokens);
               var checkAccountIsAvail = _socialMediaRepo.Get().Include(x=>x.AccessDetails).Where(x =>x.SMId == profile.Id.ToString() && x.IsDeleted == false).FirstOrDefault();
               if (checkAccountIsAvail == null)
               {
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
               }
               else if (checkAccountIsAvail.UserId == userId)
               {
                   checkAccountIsAvail.AccessDetails.AccessToken = tokens.Token;
                   checkAccountIsAvail.AccessDetails.AccessTokenSecret = tokens.TokenSecret;
                   checkAccountIsAvail.IsInvalid = false;
                   checkAccountIsAvail.Status = true;
                   returnMessage = "Already added.";
               }
               else
               {
                   checkAccountIsAvail.AccessDetails.AccessToken = tokens.Token;
                   checkAccountIsAvail.AccessDetails.AccessTokenSecret = tokens.TokenSecret;
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
        /// Update profile data (Required when algo runs for updated data)
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="socialId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get current authenticated user profile
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Like a tweet
        /// </summary>
        /// <param name="tweetId"></param>
        /// <param name="_accessToken"></param>
        /// <returns></returns>
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
               var users = service.SearchForUser(new SearchForUserOptions{ Q = query,Count = 12});
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
               int count = Int16.Parse(ConfigurationSettings.AppSettings["twitterPostSearchCountPerTag"].ToString());
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               TwitterAsyncResult<TwitterSearchResult> posts;
               if (geoLocation != null) {
                  var coordinates = _userService.AddressToCoordinates(geoLocation);
                  posts = await service.SearchAsync(new SearchOptions { Q = query, Count = count, Lang = lang, Resulttype = TwitterSearchResultType.Recent, Geocode = new TwitterGeoLocationSearch { Coordinates = new TwitterGeoLocation.GeoCoordinates { Latitude = coordinates.latitude, Longitude = coordinates.longitude }, Radius = 500, UnitOfMeasurement = TweetSharp.TwitterGeoLocationSearch.RadiusType.Km } });

               }
               else {
                   posts = await service.SearchAsync(new SearchOptions { Q = query, Count = count, Lang = lang, Resulttype = TwitterSearchResultType.Recent });
               
               }
               
               return posts;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public async Task<TwitterAsyncResult<TwitterCursorList<TwitterUser>>> GetFollowersList(long SMId, AccessDetails _accessToken)
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               var followers =await service.ListFollowersAsync(new ListFollowersOptions { UserId = SMId ,Count=200});
               while (followers.Value.NextCursor != 0) {
                   var next = service.ListFollowers(new ListFollowersOptions { UserId = SMId,Count=200, Cursor = followers.Value.NextCursor });
                   foreach (var follower in next)
                   {
                       followers.Value.Add(follower);
                   }
                   followers.Value.NextCursor = next.NextCursor;
               }
               return followers;
           }
           catch (Exception)
           {
               throw;
           }
       }

       public async Task<TwitterAsyncResult<IEnumerable<TwitterStatus>>> ListPostOnUserTimeline(long userId, AccessDetails _accessToken) 
       {
           try
           {
               TwitterService service = new TwitterService(consumerKey, consumerSecret);
               service.AuthenticateWith(_accessToken.AccessToken, _accessToken.AccessTokenSecret);
               var posts = await service.ListTweetsOnUserTimelineAsync(new ListTweetsOnUserTimelineOptions { UserId = userId, Count = 2 });
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
                   if (tweetDetail != null) {
                       activityDetails.Add(new RecentActivityViewModel { Activity = item.Activity, ActivityDate = item.ActivityDate.ToLocalTime(), UserName = tweetDetail.User.ScreenName, Content = tweetDetail.TextDecoded, ProfilePic = tweetDetail.User.ProfileImageUrlHttps, Tag = item.Tag });                   
                   }
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
       
        /// <summary>
        /// Algo wrapper for twitter accounts.
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
       public async Task<bool> scheduleAlgo(SocialMediaVM acc)
       {
           try
           {
                var startTime = DateTime.UtcNow;
               List<Tags> tags = _tagRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
               List<SuperTargetUser> targetUser = _superTargetUserRepo.Get().Where(x => x.AccSettingId == acc.AccSettings.Id).ToList();
               if (acc.IsSubscribed == true)
               {
                   var planDetails = _userService.GetASubscriptionPlan(acc.planId);
                   if (planDetails.AllowSuperTargeting == true) {
                       await SuperTargeting(targetUser, tags, acc); //if supertargetting is allowed in plan.                  
                   }
                   if (planDetails.AllowNegativeTags == false) {
                       tags.RemoveAll(x => x.IsBlocked == true); //disabling negative keywords checking by removing blocked tags from list.
                   }
                   await SearchPostUsingTag(tags, acc);

               }
               else if (acc.IsTrail == true)
               {
                   await SearchPostUsingTag(tags, acc);
                   await SuperTargeting(targetUser, tags, acc);
               }
               UpdateProfile(acc.AccessDetails, acc.Id);
               _userService.SaveFollowersCount(acc.Id, GetUserprofile(acc.AccessDetails).FollowersCount);

               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       #region Alogrithm Method
        /// <summary>
        /// supertargeting task
        /// </summary>
        /// <param name="targetUsers"></param>
        /// <param name="tags"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
       public async Task<bool> SuperTargeting(List<SuperTargetUser> targetUsers,List<Tags> tags, SocialMediaVM acc)
       {
           try
           {
               if (targetUsers != null) {
                   foreach (var user in targetUsers)
                   {
                       var followers = ListFollowerOfTargetUser(long.Parse(user.SMId), acc.AccessDetails); //getting the list of followers of target users
                       if (followers!= null)
                       {
                           int count = 0;
                           while (count < Int16.Parse(ConfigurationSettings.AppSettings["twitterTargetUserLikeADay"].ToString())) { //limit for activity per target user starts here
                               Random r = new Random();
                               var key = r.Next(followers.Count()); //Randomly generating index for picking target user followers
                               var checkForBlockedUser = CheckForBlockedUser(targetUsers, followers[key]);
                               if (checkForBlockedUser != true)
                               {
                                   var tweets = await ListPostOnUserTimeline(followers[key].Id, acc.AccessDetails); //list post from user's  timeline
                                   if (tweets.Value.Count() != 0)
                                   {
                                       foreach (var tweet in tweets.Value)
                                       {
                                           var checkNegative = CheckForNegativeTag(tags, tweet);
                                           if (tweet.IsFavorited != true && checkNegative != true)
                                           {
                                               var likeStatus = LikeTweet(tweet.Id, acc.AccessDetails); //like tweet
                                               if (likeStatus == true)
                                               {
                                                    _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, PostId = tweet.Id.ToString(), Tag = "@" + user.UserName, Username = tweet.User.ScreenName, ActivityDate = DateTime.UtcNow });
                                                    await Task.Delay(100000);
                                                    count++;
                                               }

                                           }
                                       }
                                   }
                               }//check for negative if end
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

        /// <summary>
       /// checking whether user is blocked or not.
        /// </summary>
        /// <param name="targetUsers"></param>
        /// <param name="user"></param>
        /// <returns></returns>
       public  bool CheckForBlockedUser(List<SuperTargetUser> targetUsers, TwitterUser user)
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

        /// <summary>
       /// Search posts on the basis of tags & task regarding those post.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
       public async Task<bool> SearchPostUsingTag(List<Tags> tags, SocialMediaVM acc)
       {
           try
           {
               if (tags != null) {
                   foreach (var item in tags)
                   {
                       if (item.IsBlocked == false)
                       {

                           List<string> langCode = new List<string>();
                           if (acc.AccSettings.Language != null && acc.AccSettings.Language != string.Empty)
                           {
                               var languages = acc.AccSettings.Language.Split(',');
                               for (int i = 0; i < languages.Length; i++)
                               {
                                   langCode.Add(_userService.ReturnLanguageCode(languages[i])); //getting user's language preferences
                               }
                           }
                           if (langCode.Count() == 0)
                           {
                               langCode.Add("en"); //default language
                           }
                           foreach (var lang in langCode)
                           {
                               var tweets = await SearchTweet(item.TagName, lang, item.Location, acc.AccessDetails); //search posts on the basis of tags/lang preferences
                               if (tweets != null)
                               {
                                   foreach (var tweet in tweets.Value.Statuses)
                                   {
                                       var checkNegative = CheckForNegativeTag(tags, tweet); // checks whether posts contains negative tags
                                       if (tweet.IsFavorited != true && checkNegative != true)
                                       {
                                           var result = LikeTweet(tweet.Id, acc.AccessDetails); // Like tweets
                                           if (result == true)
                                           {    //if like is done, activity is store.
                                               _userService.SaveActivity(new Activities { Activity = Activity.Like.ToString(), SMId = acc.SMId, socialId = acc.Id, PostId = tweet.Id.ToString(), Tag = "#" + item.TagName, Username = tweet.User.ScreenName, ActivityDate = DateTime.UtcNow });
                                               await Task.Delay(100000);
                                           }
                                       }

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

        /// <summary>
        /// checking whether tweet contains the tags which are blocked by user.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="tweet"></param>
        /// <returns></returns>
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
     
        /// <summary>
        /// method to check follower conversion
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
       public async Task<bool> CheckForConversion(SocialMediaVM acc)
       {
           try
           {
               var accountActivity = _activityRepo.Get().Where(x => x.socialId == acc.Id).ToList(); // getting user's activity log
               var followers = await GetFollowersList(long.Parse(acc.SMId), acc.AccessDetails); //getting user's followers list
               if (accountActivity.Count() != 0 || followers.Value.Count() != 0) {
                   foreach (var item in accountActivity)
                   {
                       foreach (var follower in followers.Value)
                       {
                           if (follower.ScreenName == item.Username && _userService.CheckIfUserConvert(item.SMId, item.socialId) == false)//checking if users from his activity log is in followers list
                           {
                               _userService.SaveConversion(new Conversions { SMId = acc.SMId, socialId = acc.Id, ConvertDate = DateTime.UtcNow, Username = item.Username, Tag = item.Tag });
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

   
        #endregion
    }
}
