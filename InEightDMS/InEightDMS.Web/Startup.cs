using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(InEightDMS.Web.Startup))]

namespace InEightDMS.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
