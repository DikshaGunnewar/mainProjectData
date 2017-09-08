using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace ServiceLayer.Helper
{
    public class WebServiceHelper
    {

        public static IRestResponse WebRequest(RestRequest request,string url)
        {

            var client = new RestClient(url);
             
            IRestResponse response = client.Execute(request);

            //if(response.ResponseStatus == "OK")
            Console.WriteLine(response.Content);
            return response;

        }
        public static bool CheckTokenInValid(IRestResponse response) {
            try
            {
                var temp = (JObject)JsonConvert.DeserializeObject(response.Content);
                var Code = temp["meta"]["code"].Value<string>();
                if (Code == "400")
                {
                    return true;
                }else{
                return true;
                }

            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
