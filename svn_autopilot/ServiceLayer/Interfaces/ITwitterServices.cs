using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace ServiceLayer.Interfaces
{
    public interface ITwitterServices
    {
        string Authorize();
        OAuthAccessToken GetTokensOAuth(string oauth_token, string oauth_verifier);
        string SaveAccountDeatils(OAuthAccessToken tokens, string userId, string Email);
        bool UpdateProfile(AccessDetails tokens, int socialId);
        IEnumerable<TwitterUser> SearchUser(string query, AccessDetails _accessToken);
        bool AddBlockSuperTargetUser(SuperTargetUserVM user);
        List<SuperTargetUserVM> GetAllTargetUser(int socialId);
        bool RemoveTargetUser(int targetUserId);
        TwitterUser GetUserprofile(AccessDetails accessToken);
        Task<TwitterAsyncResult<TwitterSearchResult>> SearchTweet(string query,string lang,string geoLocation, AccessDetails _accessToken);
        bool LikeTweet(long tweetId, AccessDetails _accessToken);
        Task<TwitterAsyncResult<TwitterCursorList<TwitterUser>>> GetFollowersList(long SMId, AccessDetails _accessToken);
        TwitterStatus GetTweet(long tweetId, AccessDetails _accessToken);
        List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken);
        Task<TwitterAsyncResult<IEnumerable<TwitterStatus>>> ListPostOnUserTimeline(long userId, AccessDetails _accessToken);
        TwitterCursorList<TwitterUser> ListFollowerOfTargetUser(long targetId, AccessDetails _accessToken);
        Task<bool> scheduleAlgo(SocialMediaVM acc);
        bool AddLocationToTag(int tagId, string location);
        Task<bool> CheckForConversion(SocialMediaVM acc);
        bool RemoveLocation(int tagId);
    }
}
