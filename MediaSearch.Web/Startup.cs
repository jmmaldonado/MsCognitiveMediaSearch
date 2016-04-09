using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MediaSearch.Web.Startup))]
namespace MediaSearch.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
