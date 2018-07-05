using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SellVMDemo.Startup))]
namespace SellVMDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
