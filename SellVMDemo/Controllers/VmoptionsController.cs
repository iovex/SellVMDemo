using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections;
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

            AzureCredentials credentials = new AzureCredentials(getUserLoginInformation(), tenantID, AzureEnvironment.AzureGlobalCloud);
            var azure = Azure.Authenticate(credentials).WithSubscription(subID);

            var sizeList = azure.VirtualMachines.Sizes.ListByRegion(region);

            return new JsonResult() { Data = sizeList.ToList().Select(x => new { Name = x.Name, MaxDataDiskCount = x.MaxDataDiskCount, MemoryInMB = x.MemoryInMB ,Cores = x.NumberOfCores , ResouceDiskInMB = x.ResourceDiskSizeInMB ,OSDiskSizeInMB = x.OSDiskSizeInMB}) , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
         
        }

        public JsonResult popularos()
        {
            var oslist = new List<IEnumerable>();
            
            var winPopularImages = Enum.GetValues(typeof(KnownWindowsVirtualMachineImage)).Cast<KnownWindowsVirtualMachineImage>().Select(e =>new KeyValuePair<string, int>(e.ToString(), (int)e));
            
            var linPopularImages = Enum.GetValues(typeof(KnownLinuxVirtualMachineImage)).Cast<KnownLinuxVirtualMachineImage>().Select(e => new KeyValuePair<string, int>(e.ToString(), (int)e));

            oslist.Add(winPopularImages);
            oslist.Add(linPopularImages);
          
            return new JsonResult() { Data = oslist, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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