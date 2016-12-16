using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcClient.Startup))]
namespace MvcClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
