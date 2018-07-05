using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SellVMDemo.Controllers
{
    public class BaseController : Controller
    {
        public async Task<AuthenticationResult> GetAADToken(string tenantId, string resource)
        {
            // auth from azure ad 
            var addAuthority = new UriBuilder(SettingsHelper.AadAuthority + tenantId);
            UserCredential userCredentials = new UserCredential(SettingsHelper.UserId,
                                                    SettingsHelper.UserPassword);
            AuthenticationContext authContext = new AuthenticationContext(addAuthority.Uri.AbsoluteUri);
            return await authContext.AcquireTokenAsync(resource,
                                                       SettingsHelper.ClientId,
                                                       userCredentials);
        }
    }
}