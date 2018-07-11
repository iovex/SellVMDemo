using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SellVMDemo.Models
{
    public class VmParams
    {
        public string osType { get; set; }
        public int popOsImage { get; set; }
        public int dataDisks { get; set; }
        public List<DataDiskParams> dataDisksDetails { get; set; }
        public string vmSzie { get; set; }
        public string region { get; set; }
    }
}