using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.EnumStore
{
    class EnumStore
    {
    }

    public enum SocialMediaProviders
    {
        Twitter,
        LinkedIn,
        Instagram,
        Pinterest,
        SoundClound,
        Spotify,
        Deezer
    }
    public enum Activity
    {
        Like,
        Comment,
        Follow,
        PinIt
    }

    public enum SpotifySearchType
    {
        artist,
        user,
        playlist,
        track

    }

}
