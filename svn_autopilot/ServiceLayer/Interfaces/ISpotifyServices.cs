using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface ISpotifyServices
    {
        string Authorize();
        OAuthTokens GetTokensOAuth(string Code);
        string SaveAccountDeatils(OAuthTokens tokens, string userId, string Email);
        bool AddBlockSuperTargetUser(SuperTargetUserVM user);
        bool AddTargetPlaylist(TargetPlaylistVM playlist);
        List<TargetPlaylistVM> GetAllTargetPlaylist(int socialId);
        List<SuperTargetUserVM> GetAllTargetUser(int socialId);
        bool RemoveTargetUser(int targetUserId);
        bool RemoveTargetPlaylist(int targetPlaylistId);
        object Search(int socialId, string query, AccessDetails Token, string type);
        AccessDetails RefreshToken(int socialId);
        SpotifyUser GetUserprofile(AccessDetails Token,int socialId=0);
        List<SpotifyArtist> GetArtistRelatedArtist(int socialId, string spotifyArtistId, AccessDetails token);
        SpotifyCategoryList GetCategoryList(int socialId, AccessDetails token);
        SpotifyArtistList GetFollowedArtist(int socialId, string query, AccessDetails token);
        bool Follow(int socialId, string spotifyId, AccessDetails token, string type);
        SpotifyUserPlaylists GetUserPlaylists(int socialId, string userId, AccessDetails token);
        PlaylistTrackResponse GetPlaylistTracks(string playlistId, string ownerId, int socialId, AccessDetails token, int count);
        Task<bool> SpotifyAlgoForUser(SocialMediaVM acc);
        List<RecentActivityViewModel> RecentActivities(int socialId, AccessDetails _accessToken, int offset);
        SpotifyTrackSearchResponse SearchTrack(int socialId, string query, AccessDetails token, int count);
        List<SuggestedTracks> GetSuggestedTracks(int socialId);
        void CheckForConversion(SocialMediaVM acc);
        bool IgnoreTrack(int socialId, string trackIds);
        bool SaveTrack(int socialId, string trackId, AccessDetails token);
        bool AddToPlaylist(int socialId, AddToPlaylistParameter data, AccessDetails token);
        bool TracksToPlaylist(SocialMediaVM acc, string suggestedTrackIds, string playlistId);
        string CreatePlaylist(int socialId, string userId, string playlistName, AccessDetails token);
    }
}
