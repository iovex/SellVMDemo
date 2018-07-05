using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace SellVMDemo.Controllers
{
    public class BillController : BaseController
    {
        public ActionResult Index()
        {
      //      var creds = new AzureCredentialsFactory().FromUser(SettingsHelper.UserId, SettingsHelper.UserPassword, SettingsHelper.ClientId, "", AzureEnvironment.AzureGlobalCloud);
        //    var azure = Azure.Authenticate(creds).WithSubscription("");
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}