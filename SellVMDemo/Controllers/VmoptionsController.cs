using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SellVMDemo.Controllers
{

    public class VmoptionsController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.resions = regions();
            return View();
        }



        // GET: Vmoptions
        public JsonResult avaliable(string tenantID ,string subID , string region)
        {

            AzureCredentials credentials = new AzureCredentials(getUserLoginInformation(), "e4c9ab4e-bd27-40d5-8459-230ba2a757fb", AzureEnvironment.AzureGlobalCloud);
            var azure = Azure.Authenticate(credentials).WithSubscription(subID);

            var sizeList = azure.VirtualMachines.Sizes.ListByRegion(region);

            return new JsonResult() { Data = sizeList.ToList().Select(x => new { Name = x.Name, MaxDataDiskCount = x.MaxDataDiskCount, MemboryInMB = x.MemoryInMB ,Cores = x.NumberOfCores , ResouceDiskInMB = x.ResourceDiskSizeInMB ,OSDiskSzieInMB = x.OSDiskSizeInMB}) , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
         
        }

        //GET:regions
        public JsonResult regions()
        {

            return new JsonResult() { Data = Region.Values, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }


        private static UserLoginInformation getUserLoginInformation() {

            var uli = new UserLoginInformation();
            uli.ClientId = ConfigurationManager.AppSettings["aad:client-id"] ;
            uli.UserName = ConfigurationManager.AppSettings["csp:admin"];
            uli.Password = ConfigurationManager.AppSettings["csp:admin-password"];

            return uli;
        }
    }
}