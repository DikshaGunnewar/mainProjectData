using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface IDeezerServices
    {
        string Authorize();
        OAuthTokens GetTokensOAuth(string Code);
        string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email);
        DeezerUserVM GetCurrentUserprofile(AccessDetails token);
        DeezerUserFollower GetCurrentUserFollower(AccessDetails token);
        List<DeezerUserVM> SearchArtist(string query);
        List<DeezerTrack> SearchTrack(string query);
        List<DeezerPlaylistVM> SearchPlaylist(string query);
        DeezerUserVM GetArtist(string Id);
        DeezerUserVM GetUser(string Id);
        DeezerUserVM GetTrack(string Id);
        List<DeezerUserVM> GetUsersFavArtist(AccessDetails token);
        DeezerPlaylistVM GetPlaylist(string Id);
        List<DeezerTrack> GetPlaylistTrack(string Id);
        List<DeezerPlaylistVM> UsersPlaylists(string query);
        List<DeezerUserVM> GetRelatedArtist(string Id);
        bool FollowArtist(string artistId, AccessDetails token);
        bool FollowUser(string user_id, AccessDetails token);
        bool FollowPlaylist(string playlist_id, AccessDetails token);
        bool AddBlockSuperTargetUser(SuperTargetUserVM user);
        List<SuperTargetUserVM> GetAllTargetUser(int socialId);
        bool RemoveTargetUser(int targetUserId);
        bool AddTargetPlaylist(TargetPlaylistVM playlist);
        List<TargetPlaylistVM> GetAllTargetPlaylist(int socialId);
        bool RemoveTargetPlaylist(int targetPlaylistId);
        Task<bool> DeezerAlgoForUser(SocialMediaVM acc);
        List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken, int offset);
        void CheckForConversion(SocialMediaVM acc);
    }
}
