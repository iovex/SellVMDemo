using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Disk.Update;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine.Definition;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using SellVMDemo.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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


        private static UserLoginInformation getUserLoginInformation() {

            var uli = new UserLoginInformation();
            uli.ClientId = ConfigurationManager.AppSettings["aad:client-id"] ;
            uli.UserName = ConfigurationManager.AppSettings["csp:admin"];
            uli.Password = ConfigurationManager.AppSettings["csp:admin-password"];

            return uli;
        }
    }
}