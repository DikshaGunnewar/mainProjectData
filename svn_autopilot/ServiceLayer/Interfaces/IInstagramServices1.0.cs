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
        bool SaveAccountDeatils(OAuthTokens tokens, string userId, string Email);
        InstaUser GetUserprofile(AccessDetails Token);
        IEnumerable<InstaUserList> SearchUser(string query, AccessDetails Token);
        InstaUser LikeUser(AccessDetails Token);
        bool RefreshToken(int socialId);
        bool AddBlockSuperTargetUser(SuperTargetUserVM user);
        List<SuperTargetUserVM> GetAllTargetUser(int socialId);
        bool RemoveTargetUser(int targetUserId);
        Task<IEnumerable<InstaSearchTag>> SearchTags(AccessDetails Token, string tagname);
       // IEnumerable<InstaSearchTag> SearchTag(AccessDetails Token, string tagname);
        bool AddLocationToTag(int tagId, string location);
        bool RemoveLocation(int tagId);
<<<<<<< .mine
        
=======
        //List<InstaRecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken);
>>>>>>> .r181
    }
}
