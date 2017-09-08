using EntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EntitiesLayer.ViewModel
{
    public class ViewModels
    {
    }
    public class SocialMediaVM {

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Provider { get; set; }
        public bool Status { get; set; }
        public int Followers { get; set; }
        public string SMId { get; set; }
        public string UserName { get; set; }
        public string ProfilepicUrl { get; set; }
        public AccessDetails AccessDetails { get; set; }
        public AccSettings AccSettings { get; set; }
    }

    public class PinterestUser{
        public UserVM data { get; set; }
       
    }
    public class UserVM{
        public string id{ get; set; }
        public string first_name{ get; set; }
        public string last_name{ get; set; }
        public string url{ get; set; }
        public string username{ get; set; }
        }
    public class OAuthTokens
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        //public string[] scope { get; set; }expires_in
        public int expires_in { get; set; }
        public string refresh_token { get; set; }

        
    }

    public class LinkedInUser
    {
        public string emailAddress { get; set; }
        public string id { get; set; }
        public int numConnections { get; set; }
        public string publicProfileUrl { get; set; }
        public string pictureUrl { get; set; }

    }
    public class TagsVM
    {
        public int Id { get; set; }
        public int AccSettingId { get; set; }
        public string TagName { get; set; }
        public string Location { get; set; }
        public bool IsBlocked { get; set; }
    }
    public class SuperTargetUserVM {
        public int Id { get; set; }
        public int SocailId { get; set; }
        public string UserName { get; set; }
        public string SMId { get; set; }
        public int Followers { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class InstaUser
    {
        public InstaUserList data { get; set; }
        
    }

    public class InstaSearchTag
    {
        
        public int Id { get; set; }
        public int SocailId { get; set; }
        public string UserName { get; set; }
        public string SMId { get; set; }
        public int Followers { get; set; }
        public bool IsBlocked { get; set; }
        public List<TagsVM> tagvm { get; set; }

        public InstaSearchTag()
         {
             tagvm = new List<TagsVM>();
         }

        [Column(TypeName = "DateTime2")]
        public DateTime ActivityDate { get; set; }
        public string Activity { get; set; }
    }

    public class InstaSearchTagList
    {
        public List<InstaSearchTag> data { get; set; }
       
    }


    public class InstaUserList
    {
        public long id { get; set; }
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string full_name { get; set; }
        public string bio { get; set; }
        public InstaUserFollower counts { get; set; }
    }

    public class InstaSearchUserList
    {
        public List<InstaUserList> data { get; set; }
       
    }

<<<<<<< .mine


    public class InstaUserFollower
    {
        public int follows { get; set; }
    }

    public class SpotifyUser
    {

        public string id { get; set; }
        public string email { get; set; }
        public string country { get; set; }
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

     public class SpotifyArtistSearch { 
         public SpotifyArtist artists { get; set; }
    
     }
     public class SpotifyArtist { 
         public string href { get; set; }
         public int limit { get; set; }
         public int total { get; set; }
         public List<ArtistItem> items { get; set; }

         public SpotifyArtist()
         {
             items = new List<ArtistItem>();
         }
         

     }
     public class ArtistItem
     {
         public string name { get; set; }
         public SpotifyFollower followers { get; set; }
         public List<SpotifyProfileImage> images { get; set; }
         public string id { get; set; }
         public string uri { get; set; }

//         public string[] genres { get; set; }

         public ArtistItem()
         {
             images = new List<SpotifyProfileImage>();

         }
     }

=======
>>>>>>> .r181
     public class RecentActivityViewModel {
         public TweetSharp.TwitterStatus tweet { get; set; }
         public string Tag { get; set; }
         [Column(TypeName = "DateTime2")]
         public DateTime ActivityDate { get; set; }
         public string Activity { get; set; }
     }



     public class Coordinates {
         public double longitude{ get; set; }
         public double latitude { get; set; }
     
     }

     public class ConversionStats {
         public string date { get; set; }
         public int count { get; set; }
     }

<<<<<<< .mine
=======

     public class InstaRecentActivityViewModel
     {
         
         public string Tag { get; set; }
         [Column(TypeName = "DateTime2")]
         public DateTime ActivityDate { get; set; }
         public string Activity { get; set; }
     }






>>>>>>> .r181
}
