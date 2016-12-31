using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Cars.Startup))]
namespace Cars
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
