using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using TweetSharp;

namespace Autopilot.Controllers
{

    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class TwitterController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            string Key = "kWEWr13tCz4lCTV8qntCpamVK";
            string Secret = "gh7DcNDojT96RKuV6ZAqExdIuBolguEwpKDjf6ycBjQiYWwACg";

            TwitterService service = new TwitterService(Key, Secret);

            //Obtaining a request token
            OAuthRequestToken requestToken = service.GetRequestToken
                                             ("http://localhost:12952/api/System/TwitterCallback");

            Uri uri = service.GetAuthenticationUrl(requestToken);

            //Redirecting the user to Twitter Page
            return Ok(uri.ToString());
        }
        [HttpGet]
        public IHttpActionResult TwitterCallback(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            string Key = "kWEWr13tCz4lCTV8qntCpamVK";
            string Secret = "gh7DcNDojT96RKuV6ZAqExdIuBolguEwpKDjf6ycBjQiYWwACg";

            try
            {

                TwitterService service = new TwitterService(Key, Secret);

                //Get Access Tokens
                OAuthAccessToken accessToken =
                           service.GetAccessToken(requestToken, oauth_verifier);

                service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
                var profile = service.GetUserProfile(new GetUserProfileOptions { IncludeEntities = true });
             

                //var profile = service.GetUserProfile(new GetUserProfileOptions {  IncludeEntities = true });
                VerifyCredentialsOptions option = new VerifyCredentialsOptions();

                //According to Access Tokens get user profile details
                TwitterUser user = service.VerifyCredentials(option);

                return Ok();
            }
            catch
            {
                throw;
            }
        }
    }
}
