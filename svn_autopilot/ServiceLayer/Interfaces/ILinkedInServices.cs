using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface ILinkedInServices
    {
        string Authorize();
        OAuthTokens GetToken(string Code);
        bool SaveAccountDeatils(OAuthTokens tokens, string userId, string Email);
        LinkedInUser GetUserprofile(AccessDetails Token);
    }
}
