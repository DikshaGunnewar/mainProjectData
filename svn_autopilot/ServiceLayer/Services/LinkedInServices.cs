using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using RestSharp;
using RestSharp.Deserializers;
using ServiceLayer.EnumStore;
using ServiceLayer.Helper;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class LinkedInServices:ILinkedInServices
    {

       private readonly IRepository<SocialMedia> _socialMedia;
       private readonly IRepository<AccessDetails> _accessDetail;  

       private readonly IUnitOfWork _unitOfWork;
       private readonly string consumerKey;
       private readonly string consumerSecret;
       private readonly string ApiURL ;
       private readonly string RedirectURL;

       public LinkedInServices(IRepository<SocialMedia> socialMedia, IUnitOfWork unitOfWork, IRepository<AccessDetails> accessDetail)
       {
           _socialMedia = socialMedia;
           _unitOfWork = unitOfWork;
           _accessDetail = accessDetail;
           consumerKey = ConfigurationSettings.AppSettings["linkID"];
           consumerSecret = ConfigurationSettings.AppSettings["linkSecret"];
           RedirectURL = ConfigurationSettings.AppSettings["linkRedirectUrl"];
           ApiURL = "https://www.linkedin.com/";

      }

       /// <summary>
       /// Method to get the uri for authorizing user in Linkedin
       /// </summary>
       /// <returns></returns>
       public string Authorize() {
           try
           {
                var request = new RestRequest("oauth/v2/authorization/", Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("scope", "r_basicprofile r_emailaddress rw_company_admin w_share r_emailaddress");
                request.AddParameter("client_id", consumerKey);
                request.AddParameter("redirect_uri", RedirectURL);
                request.AddParameter("response_type", "code");
                // request.AddParameter("host_id", "QwJrNRmITnC3ZenQzXtedg");
                var response = WebServiceHelper.WebRequest(request, ApiURL);
                var uri = response.ResponseUri.AbsoluteUri;
                
               return uri;
           }
           catch (Exception)
           {
               
               throw;
           }
           
       }

        /// <summary>
       /// Method to get OAuth token for authenticate user
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
       public OAuthTokens GetToken(string Code)
       {
           //var token = Session["FacebookAccessToken"];
           var request = new RestRequest("oauth/v2/accessToken", Method.POST);
           request.AddHeader("Content-Type", "application/json");
           //request.AddHeader("Authorization", "OAuth " + token);
           request.AddParameter("client_id", consumerKey);
           request.AddParameter("redirect_uri", RedirectURL);
           request.AddParameter("client_secret", consumerSecret);
           request.AddParameter("grant_type", "authorization_code");
           request.AddParameter("code", Code);

           // request.AddParameter("host_id", "QwJrNRmITnC3ZenQzXtedg");
           var response =WebServiceHelper.WebRequest(request, ApiURL);
           JsonDeserializer deserial = new JsonDeserializer();
           OAuthTokens result = deserial.Deserialize<OAuthTokens>(response);
           return result;
       }

       public bool SaveAccountDeatils(OAuthTokens tokens, string userId,string Email)
       {
           try
           {
               AccessDetails accessToken = new AccessDetails() { AccessToken = tokens.access_token };
               LinkedInUser profile = GetUserprofile(accessToken);
               SocialMedia socialDetails = new SocialMedia()
                 {
                     UserId = userId,
                     Provider = SocialMediaProviders.LinkedIn.ToString(),
                     AccessDetails = new AccessDetails { AccessToken = tokens.access_token},
                     ProfilepicUrl = profile.pictureUrl,
                     SMId = profile.id,
                     Status = true,
                     UserName = profile.emailAddress,
                     Followers = profile.numConnections,
                     AccSettings = new AccSettings()
                 };
               socialDetails.AccSettings.UserManagement.Add(new UserManagement { Email = Email, userId = userId, Role = "Owner" });
               _socialMedia.Add(socialDetails);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       public LinkedInUser GetUserprofile(AccessDetails Token)
       {
           try
           {
              
               //var token = Session["FacebookAccessToken"];

               var request = new RestRequest("/v1/people/~:(id,num-connections,picture-url,public-profile-url,email-address)", Method.GET);
               request.AddParameter("format", "json");
               request.AddParameter("oauth2_access_token", Token.AccessToken);
                var response = WebServiceHelper.WebRequest(request, ApiURL);
               JsonDeserializer deserial = new JsonDeserializer();
               LinkedInUser result = deserial.Deserialize<LinkedInUser>(response);
               return result;
           }
           catch (Exception)
           {

               throw;
           }
       }
      
    }
    }

