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
    public interface IPinterestServices
    {
        string Authorize();
        OAuthTokens GetPinToken(string Code);
        string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email);
        PinterestUser GetUserprofile(AccessDetails Token);
        List<PinterestSchedulePinVm> GetSchedulePin(int socialId);
        List<Boards> GetAllBoard(AccessDetails Token);
        bool SaveSchedulePin(PinterestScheduledPin PinInfo);
        PinterestScheduledPin GetASchedulePin(int schedulePinId);
        bool RemoveSchedulePin(int schedulePinId);
        List<Boards> GetUsersFollowingBoard(AccessDetails Token);
        List<Boards> CheckBoardConnection(int socailId, string myBoardId, List<Boards> followingBoard);
        bool SaveBoardMapping(int socialId, string myBoardId, string followingBoardIds);
        void ScheduleAlgo(SocialMediaVM acc);
        bool FollowUser(string userId, AccessDetails Token);
        List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken);
        void PostSchedulePin(int schedulePinId,int socialId);
    }
}
