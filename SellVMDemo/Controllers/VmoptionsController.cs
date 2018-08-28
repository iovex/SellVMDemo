using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Disk.Update;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine.Definition;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Store.PartnerCenter;
using Microsoft.Store.PartnerCenter.Extensions;
using Microsoft.Store.PartnerCenter.Models.RateCards;
using Newtonsoft.Json;
using SellVMDemo.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
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

        //post
        public JsonResult create(string tenantID, string subID, [FromBody]VmParams vmParams)
        {
            AzureCredentials credentials = new AzureCredentials(getUserLoginInformation(), tenantID, AzureEnvironment.AzureGlobalCloud);
            var azure = Azure.Authenticate(credentials).WithSubscription(subID);

            var vnetrgname = "defaultNetworkGp-" + vmParams.region; 

            var vnet = azure.Networks.GetByResourceGroup(vnetrgname, "defaultNetwork");

            if (vnet == null)
            {
                var regionList = Region.Values.ToList();
                int regionIndex = -1;
                for (var i = 0; i < regionList.Capacity; i++) {
                    if(regionList[i].Name == vmParams.region) { regionIndex = i; break; }
                }

                azure.Networks.Define("defaultNetwork").WithRegion(vmParams.region).WithNewResourceGroup(vnetrgname).WithAddressSpace("222."+ regionIndex +".0.0/16").WithSubnet("defaultSubnet", "222."+ regionIndex + "." + regionIndex + ".0/24").Create();
                vnet = azure.Networks.GetByResourceGroup(vnetrgname, "defaultNetwork");
            }

            var vmName = (vmParams.osType == "windows" ? "WinVM" : "LinVM") + Guid.NewGuid();
            var vmrgname = vmName + "-Gp";
            var vmrg = azure.ResourceGroups.Define(vmrgname).WithRegion(vmParams.region).Create();

            var newVm = azure
                .VirtualMachines.Define(vmName)
                .WithRegion(vmParams.region)
                .WithExistingResourceGroup(vmrg)
                .WithExistingPrimaryNetwork(vnet)
                .WithSubnet("defaultSubnet")
                .WithPrimaryPrivateIPAddressDynamic()
                .WithNewPrimaryPublicIPAddress(vmName);


            if (vmParams.osType == "windows")
                newVm.WithPopularWindowsImage((KnownWindowsVirtualMachineImage)vmParams.popOsImage).WithAdminUsername("manager").WithAdminPassword("Password123!");
            else
                newVm.WithPopularLinuxImage((KnownLinuxVirtualMachineImage)vmParams.popOsImage).WithRootUsername("manager").WithRootPassword("Password123!");


            var newVMwithDisk = addDataDisks(azure, (IWithManagedDataDisk)newVm, vmParams, vmrgname);
            var result =  ((IWithCreate)newVMwithDisk).WithSize(vmParams.vmSzie).Create();

            var DNSLabel = vmName + "." + vmParams.region + ".cloudapp.azure.com";
            
            return new JsonResult() { Data = "VM created and initializing ,you can access it in 5 mins with public IP:" + result.GetPrimaryPublicIPAddress().IPAddress + " or DNS label:" + DNSLabel + "   ADMIN ACCOUNT : manager , PASSWORD : Password123! . " };
        }


        private IWithManagedDataDisk addDataDisks(IAzure azure , IWithManagedDataDisk vm, VmParams vmParams,string rgname) {
            if (vmParams.dataDisks > 0)
            {
                foreach (var disk in vmParams.dataDisksDetails)
                {
                    var dataDiskCreatable = azure.Disks.Define(disk.id)
                       .WithRegion(vmParams.region)
                       .WithExistingResourceGroup(rgname)
                       .WithData()
                       .WithSizeInGB(disk.size);

                    vm.WithNewDataDisk(dataDiskCreatable);
                }
            }

            return vm;
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

        //post
        public JsonResult getVMPricing([FromBody] VmParams vmParams) {
            IPartner partner = PartnerService.Instance.CreatePartnerOperations(GetPartnerCenterTokenWithUserCredentials().Credentials);
            var azureRateCardMeters = partner.RateCards.Azure.Get().Meters.ToList();
        
            var vmSizeParams = vmParams.vmSzie.ToLower().Split('_');

            var vmSize = vmSizeParams[1].Split('-')[0];

            if (vmSizeParams.Length > 2)
                vmSize += (" " + vmSizeParams[2].Split('-')[0]);

            var regionMap = "[{\"Value\":\"AU East\",\"Key\":\"australiaeast\"},\r\n{\"Value\":\"AU Southeast\",\"Key\":\"australiasoutheast\"},\r\n{\"Value\":\"AP Southeast\",\"Key\":\"southeastasia\"},\r\n{\"Value\":\"AP East\",\"Key\":\"eastasia\"},\r\n{\"Value\":\"EU North\",\"Key\":\"northeurope\"},\r\n{\"Value\":\"EU West\",\"Key\":\"westeurope\"},\r\n{\"Value\":\"BR South\",\"Key\":\"brazilsouth\"},\r\n{\"Value\":\"US West Central\",\"Key\":\"westcentralus\"},\r\n{\"Value\":\"US South Central\",\"Key\":\"southcentralus\"},\r\n{\"Value\":\"US North Central\",\"Key\":\"northcentralus\"},\r\n{\"Value\":\"US East\",\"Key\":\"eastus\"},\r\n{\"Value\":\"US East 2\",\"Key\":\"eastus2\"},\r\n{\"Value\":\"US West\",\"Key\":\"westus\"},\r\n{\"Value\":\"US West 2\",\"Key\":\"westus2\"},\r\n{\"Value\":\"US Central\",\"Key\":\"centralus\"},\r\n{\"Value\":\"JA West\",\"Key\":\"japanwest\"},\r\n{\"Value\":\"JA East\",\"Key\":\"japaneast\"},\r\n{\"Value\":\"CA East\",\"Key\":\"canadaeast\"},\r\n{\"Value\":\"CA Central\",\"Key\":\"canadacentral\"},\r\n{\"Value\":\"KR Central\",\"Key\":\"koreacentral\"},\r\n{\"Value\":\"KR South\",\"Key\":\"koreasouth\"},\r\n{\"Value\":\"UK West\",\"Key\":\"ukwest\"},\r\n{\"Value\":\"UK South\",\"Key\":\"uksouth\"},\r\n{\"Value\":\"IN Central\",\"Key\":\"centralindia\"},\r\n{\"Value\":\"IN West\",\"Key\":\"westindia\"},\r\n{\"Value\":\"IN South\",\"Key\":\"southindia\"}]";

            var regionPairs =JsonConvert.DeserializeObject<List<KeyValuePair<string,string>>>(regionMap);
            var region = "";
            foreach (var pair in regionPairs) {
                if (pair.Key == vmParams.region) { region = pair.Value;break; }
                    
            }

            var vmlist = new List<AzureMeter>();
            foreach (var meter in azureRateCardMeters) {
                if (meter.Category == "Virtual Machines" && 
                    meter.Name.ToLower().IndexOf(vmSize) >=0 &&
                    meter.Subcategory.ToLower().IndexOf("low priority") <0 &&
                    meter.Region == region ) { vmlist.Add(meter); }
            }

            var vmlistAfterOs = new List<AzureMeter>();
            if (vmParams.osType == "windows")  foreach (var meter in vmlist) { if (meter.Subcategory.ToLower().IndexOf("windows") > 0) { vmlistAfterOs.Add(meter); } } 
            else { foreach (var meter2 in vmlist) if (meter2.Subcategory.ToLower().IndexOf("windows") < 0) vmlistAfterOs.Add(meter2); }

            return new JsonResult() { Data = vmlistAfterOs.Select(x => new { name = x.Subcategory, rates = (x.Rates[0]*new Decimal(1.4)).ToString(), region = x.Region , vmRateUnit=x.Unit ,diskRating ="0.05" ,diskRateUnit="month/GB"}), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private static AuthenticationResult LoginToAad()
        {
            // auth from azure ad 
            var addAuthority = new UriBuilder(ConfigurationManager.AppSettings["aad:authority"] + ConfigurationManager.AppSettings["csp:partnerId"]);
            UserCredential userCredentials = new UserCredential(ConfigurationManager.AppSettings["csp:admin"], ConfigurationManager.AppSettings["csp:admin-password"]);
            AuthenticationContext authContext = new AuthenticationContext(addAuthority.Uri.AbsoluteUri);
            return authContext.AcquireToken(ConfigurationManager.AppSettings["partnercenter-endpoint"], ConfigurationManager.AppSettings["aad:client-id"], userCredentials);
        }

        public static IAggregatePartner GetPartnerCenterTokenWithUserCredentials()
        {
            // Get a user Azure AD Token.
            var aadAuthResult = LoginToAad();
            PartnerService.Instance.ApiRootUrl = ConfigurationManager.AppSettings["partnercenter-endpoint"];
            var partnerCredentials =
                PartnerCredentials.Instance.GenerateByUserCredentials(getUserLoginInformation().ClientId,
                    new AuthenticationToken(aadAuthResult.AccessToken, aadAuthResult.ExpiresOn), async delegate
                    {
                // Token Refresh callback.
                aadAuthResult = LoginToAad();
                        return await Task.FromResult<AuthenticationToken>(new AuthenticationToken(aadAuthResult.AccessToken, aadAuthResult.ExpiresOn));
                    });

            // Get operations instance with partnerCredentials.
            return PartnerService.Instance.CreatePartnerOperations(partnerCredentials);
        }




        private static UserLoginInformation getUserLoginInformation()
        {

            var uli = new UserLoginInformation();
            uli.ClientId = ConfigurationManager.AppSettings["aad:client-id"];
            uli.UserName = ConfigurationManager.AppSettings["csp:admin"];
            uli.Password = ConfigurationManager.AppSettings["csp:admin-password"];

            return uli;
        }

    }
}