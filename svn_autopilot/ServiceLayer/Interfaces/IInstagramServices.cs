using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface IInstagramServices
    {
        string Authorize();
        OAuthTokens GetToken(string Code);
        string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email);
        InstaUserList GetUserprofile(AccessDetails Token);
        AccessDetails RefreshToken(int socialId);
        IEnumerable<InstaUserList> SearchUser(string query, AccessDetails Token);
        bool AddBlockSuperTargetUser(SuperTargetUserVM user);
        List<SuperTargetUserVM> GetAllTargetUser(int socialId);
        bool RemoveTargetUser(int targetUserId);
        Task<IEnumerable<InstaSearchTag>> SearchTags(AccessDetails Token, string tagname);
        bool AddLocationToTag(int tagId, string location);
        bool RemoveLocation(int tagId);
        void scheduleAlgo(SocialMediaVM acc);
        IEnumerable<Location> Location(AccessDetails Token, double lat, double lng);
        List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken);
        bool Logout();
      
    }

}
