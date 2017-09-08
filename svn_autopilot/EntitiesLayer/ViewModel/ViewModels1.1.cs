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

    public class ApplicationUserVM{
        public string Email { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime RegisterationDate { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime PasswordUpdateDate { get; set; }
    }

    public class SocialMediaVM {

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Provider { get; set; }
        public bool Status { get; set; }
        public int Followers { get; set; }
        public string SMId { get; set; }
        public bool IsInvalid { get; set; }
        public bool IsTrail{ get; set; }
        public bool IsSubscribed { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime ExpireOn { get; set; }
        public string planId { get; set; }
        public string planName{ get; set; }
        public string UserName { get; set; }
        public string ProfilepicUrl { get; set; }
        public AccessDetails AccessDetails { get; set; }
        public AccSettings AccSettings { get; set; }
    }


    public class OAuthTokens
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        //public string[] scope { get; set; }expires_in
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }


    public class ListOAuthTokens
    {
        public List<OAuthTokens> token { get; set; }
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
        public InstaFollower counts { get; set; }
        //public InstaUserList data { get; set; }
        
    }

    public class location
    {
        public int id { get; set; }
        public string UserName { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
    }

    public class InstaSearchTag
    {
        public string Id { get; set; }
        public InstaTagList user { get; set; }
        public bool user_has_liked { get; set; }

        public InstaSearchTag()
        {
            user = new InstaTagList();
        }
    }

    public class InstaTagList
    {
        public string id { get; set; }
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string full_name { get; set; }

    }



    public class InstaUserFollow
    {
        public List<InstaUserList> data { get; set; }
    }


    public class InstaSearchTagList
    {
        public List<InstaSearchTag> data { get; set; }
        public InstaSearchTagList()
        {
            data = new List<InstaSearchTag>();
        }
    }

    public class InstaUserProfile
    {
        public InstaUserList data { get; set; }
    }
    
    public class InstaUserList
    {
        public long id { get; set; }
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string full_name { get; set; }
        public string bio { get; set; }
        public bool follows { get; set; }
        public bool IsAccountValid { get; set; }
        public InstaFollower counts { get; set; }
        public InstaUserList()
        {
            counts = new InstaFollower();
        }
    }

    public class InstaFollower
    {
        public int media{ get; set; }
        public int follows{ get; set; }
        public int followed_by{get;set;}
    }


    public class InstaSearchUserList
    {
        public List<InstaUserList> data { get; set; }
    }

     public class RecentActivityViewModel {
         public TweetSharp.TwitterStatus tweet { get; set; }
         public string Content { get; set; }
         public string Tag { get; set; }
         public string ProfilePic { get; set; }
         public string UserName { get; set; }
         [Column(TypeName = "DateTime2")]
         public DateTime ActivityDate { get; set; }
         public string Activity { get; set; }
       
     }

     public class Coordinates {
         public double longitude{ get; set; }
         public double latitude { get; set; }
     }



     public class ConversionStats {
         public string label { get; set; }
         public int count { get; set; }
         public int total { get; set; }
        
     }

     public class ConversionCount
     {
         public int todayConversion { get; set; }
         public int weekConversion { get; set; }
         public int totalConversion { get; set; }
     }
     public class ConversionsVM
     {
         public int Id { get; set; }
         public string SMId { get; set; }
         public string Username { get; set; }
         public int socialId { get; set; }
         public string Tag { get; set; }
         public string ProfilePic { get; set; }
         public string ConvertDate { get; set; }
     }
     public class InstaRecentActivityViewModel
     {
         public string details { get; set; }
         public int socialId { get; set; }
         public string Tag { get; set; }
         [Column(TypeName = "DateTime2")]
         public DateTime ActivityDate { get; set; }
         public string Activity { get; set; }
     }

     public class UserProfileVM {
         public string UserId { get; set; }
         public string Email { get; set; }
         public bool EmailVerified { get; set; }
         public string Username { get; set; }
         public UserBillingAddress BillingAddress { get; set; }
         public List<SocialMediaVM> UserAccounts { get; set; }
     }

<<<<<<< .mine


     public class Subscriptions
     {
         public int Id { get; set; }
         public string Title { get; set; }
         public double Price { get; set; }
         public string BillingFrequency { get; set; }
         public string NoOfAccounts { get; set; }
         public bool LimitedTagService { get; set; }
         public bool LowSpeedOfInteraction { get; set; }
         public bool AllowSuperTargeting { get; set; }
         public bool AllowNegativeTags { get; set; }
         public string TagLimit { get; set; }
         public bool IsDeleted { get; set; }
         public bool IsTrail { get; set; }
         public string TrailDays { get; set; }
     }




     public class SubscribeViewModel
     {
        
         public List<SocialMediaVM> social { get; set; }
         public List<SubscriptionsPlan> subscribe { get; set; }
        
     }  

=======
    public class PaymentViewModel{
        public string TransactionId { get; set; }
        public string planId { get; set; }
        public string userId { get; set; }
        public string socialIds{ get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string InvoiceId { get; set; }
        public string Status { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime Date { get; set; }        
        public SubscriptionsPlan planDetails { get; set; }
        }

>>>>>>> .r322
}
