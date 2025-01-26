using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PetrZatloukalUkol.Startup))]
namespace PetrZatloukalUkol
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
