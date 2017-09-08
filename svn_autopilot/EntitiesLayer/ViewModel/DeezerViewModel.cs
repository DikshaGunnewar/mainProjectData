using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.ViewModel
{
    public class DeezerViewModel
    {
    }
    public class DeezerUserVM
    {
        public long id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int status { get; set; }
        public string picture_small { get; set; }
        public string lang { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public string nb_album { get; set; }
        public string nb_fan { get; set; }

    }
    public class DeezerPlaylistVM
    {
        public long id { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public int nb_tracks { get; set; }
        public string picture_small { get; set; }
        public string type { get; set; }
        public DeezerUserVM user{ get; set; }

    }
    public class DeezerUserFollower {
        public List<DeezerUserVM> data { get; set; }
        public int total { get; set; }
        public string next { get; set; }
    
    }

    public class DeezerArtistSearch {
        public List<DeezerUserVM> data{ get; set; }
        public string total { get; set; }
        public string next { get; set; }
        public DeezerArtistSearch()
        {
            data = new List<DeezerUserVM>();
        }
    }

    public class DeezerUserSearch
    {
        public List<DeezerUserVM> data { get; set; }
        public string total { get; set; }
        public string next { get; set; }
        public DeezerUserSearch()
        {
            data = new List<DeezerUserVM>();
        }
    }

    public class DeezerPlaylistSearch
    {
        public List<DeezerPlaylistVM> data { get; set; }
        public string total { get; set; }
        public string next { get; set; }
        public DeezerPlaylistSearch()
        {
            data = new List<DeezerPlaylistVM>();
        }
    }

    public class DeezerTrack {
        public long id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public bool readable { get; set; }
        public DeezerUserVM artist { get; set; }

    }

    public class DeezerTrackSearch { 
        public List<DeezerTrack> data{ get; set; }
        public string total { get; set; }
        public string next { get; set; }
        public DeezerTrackSearch()
        {
            data = new List<DeezerTrack>();
        }
    }
}
