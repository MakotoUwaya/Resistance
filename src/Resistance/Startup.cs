using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Resistance.Startup))]
namespace Resistance
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
