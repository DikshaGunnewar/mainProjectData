using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;

namespace Autopilot.Helper
{
    public class PaypalConfiguration
    {

        
        //these variables will store the clientID and clientSecret
        //by reading them from the web.config
        public readonly static string ClientId;
        public readonly static string ClientSecret;

        static PaypalConfiguration()
        {
            var config = GetConfig();
            ClientId = config["clientId"];
            ClientSecret = config["clientSecret"];
        }

        // getting properties from the web.config
        public static Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
            //return PayPal.Manager.ConfigManager.Instance.GetProperties();
        }

        private static string GetAccessToken()
        {
            // getting accesstocken from paypal                
            string accessToken = new OAuthTokenCredential
        (ClientId, ClientSecret, GetConfig()).GetAccessToken();

            return accessToken;
        }

        public static APIContext GetAPIContext()
        {
            // return apicontext object by invoking it with the accesstoken
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }

        //public static string ClientId;
        //public static string ClientSecret;
        //public static bool TakeApiKeyFromWebConfig = false;

        //// Static constructor for setting the readonly static members.
        //static PaypalConfiguration()
        //{
        //    var config = GetConfig();
        //    if (TakeApiKeyFromWebConfig)
        //    {
        //        ClientId = config["clientId"];
        //        ClientSecret = config["clientSecret"];
        //    }
        //    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        //}

        //// Create the configuration map that contains mode and other optional configuration details.
        //public static Dictionary<string, string> GetConfig()
        //{
        //    return PayPal.Api.ConfigManager.Instance.GetProperties();
        //}

        //// Create accessToken
        //private static string GetAccessToken(string _ClientId, string _ClientSecret)
        //{
        //    // ###AccessToken
        //    // Retrieve the access token from
        //    // OAuthTokenCredential by passing in
        //    // ClientID and ClientSecret
        //    // It is not mandatory to generate Access Token on a per call basis.
        //    // Typically the access token can be generated once and
        //    // reused within the expiry window                

        //    if (string.IsNullOrEmpty(_ClientId) || string.IsNullOrEmpty(_ClientSecret))
        //    {
        //        string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
        //        return accessToken;
        //    }
        //    else
        //    {
        //        string accessToken = new OAuthTokenCredential(_ClientId, _ClientSecret, GetConfig()).GetAccessToken();
        //        return accessToken;
        //    }

        //}

        //// Returns APIContext object
        //public static APIContext GetAPIContext(string _ClientId, string _ClientSecret)
        //{
        //    // ### Api Context
        //    // Pass in a `APIContext` object to authenticate 
        //    // the call and to send a unique request id 
        //    // (that ensures idempotency). The SDK generates
        //    // a request id if you do not pass one explicitly. 
        //    APIContext apiContext = new APIContext(GetAccessToken(_ClientId, _ClientSecret));
        //    apiContext.Config = GetConfig();

        //    // Use this variant if you want to pass in a request id  
        //    // that is meaningful in your application, ideally 
        //    // a order id.
        //    // String requestId = Long.toString(System.nanoTime();
        //    // APIContext apiContext = new APIContext(GetAccessToken(), requestId ));

        //    return apiContext;
        //}

    }
}