using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.ViewModel
{
    class SpotifyVM
    {
    }


    public class SpotifyUser
    {
        public string id { get; set; }
        public string email { get; set; }
        public string type { get; set; }
        public string country { get; set; }
        public string display_name { get; set; }

        public SpotifyFollower followers { get; set; }
        public List<SpotifyProfileImage> images { get; set; }

        public SpotifyUser()
        {
            images = new List<SpotifyProfileImage>();
        }
    }
    public class SpotifyFollower
    {
        public string href { get; set; }
        public int total { get; set; }
    }

    public class SpotifyProfileImage
    {
        public string url { get; set; }
    }

    public class SpotifyArtistSearch
    {
        public SpotifyArtistList artists { get; set; }

    }

    public class SpotifyArtistList
    {
        public string href { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public List<SpotifyArtist> items { get; set; }

        public SpotifyArtistList()
        {
            items = new List<SpotifyArtist>();
        }


    }

    public class SpotifyRelatedArtist {
        public List<SpotifyArtist> artists { get; set; }
        public SpotifyRelatedArtist()
        {
            artists = new List<SpotifyArtist>();
        }
    }
    public class SpotifyArtist
    {
        public string name { get; set; }
        public SpotifyFollower followers { get; set; }
        public List<SpotifyProfileImage> images { get; set; }
        public ExternalUri external_urls { get; set; }
        public string id { get; set; }
        public string uri { get; set; }
        public string type { get; set; }
        public int popularity { get; set; }

        //         public string[] genres { get; set; }

        public SpotifyArtist()
        {
            images = new List<SpotifyProfileImage>();

        }
    }
    public class SpotifyCategorySearch
    {
        public SpotifyCategoryList categories { get; set; }

    }
    public class SpotifyCategoryList
    {
        public string href { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public List<SpotifyCategory> items { get; set; }

        public SpotifyCategoryList()
        {
            items = new List<SpotifyCategory>();
        }


    }
    public class SpotifyCategory {
        public string href{ get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public List<SpotifyProfileImage> icons { get; set; }
        public SpotifyCategory()
        {
            icons = new List<SpotifyProfileImage>();
        }

    }

    public class SpotifyUserPlaylists
    {
        public string href { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public List<SpotifyPlaylist> items { get; set; }

        public SpotifyUserPlaylists()
        {
            items = new List<SpotifyPlaylist>();
        }
    }

    public class SpotifyPlaylistSearch
    {
        public SpotifyUserPlaylists playlists { get; set; }

    }

    public class ExternalUri {
        public string spotify { get; set; }
    }


    public class SpotifyPlaylist
    {
        public string href { get; set; }
        public bool collaborative { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string uri { get; set; }
        public string type { get; set; }
        public SpotifyUser owner { get; set; }
        public ExternalUri external_urls { get; set; }
        public SpotifyFollower tracks { get; set; }

        public List<SpotifyProfileImage> images { get; set; }
        public SpotifyPlaylist()
        {
            images = new List<SpotifyProfileImage>();
            //owner = new List<SpotifyUser>();
        }

    }

    public class TargetPlaylistVM
    {
        public int Id { get; set; }
        public int SocailId { get; set; }
        public string OwnerId { get; set; }
        public string PlaylistId { get; set; }
        public int TracksCount { get; set; }
        public string Name { get; set; }
    }

    public class PlaylistTrackResponse {
        public string href { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public List<PlaylistTracks> items { get; set; }

        public PlaylistTrackResponse()
        {
            items = new List<PlaylistTracks>();
        }
    }

    public class PlaylistTracks
    {
        public Tracks track { get; set; }
        public SpotifyUser added_by { get; set; }
    }
    public class Tracks {
        public List<SpotifyArtist> artists { get; set; }
        public string id { get; set; }
        public string href { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
        public Tracks()
        {
            artists = new List<SpotifyArtist>();
        }
    }

    public class SpotifyTrackSearch {
        public SpotifyTrackSearchResponse tracks { get; set; }
    }

    public class SpotifyTrackSearchResponse
    {
        public string href { get; set; }
        public List<Tracks> items { get; set; }
        public string offset { get; set; }
        public long total{ get; set; }
        public SpotifyTrackSearchResponse()
        {
            items = new List<Tracks>();
        }

    }

    public class AddToPlaylistParameter {
        public string userId { get; set; }
        public string playlistId { get; set; }
        public string uri { get; set; }

    }

}
