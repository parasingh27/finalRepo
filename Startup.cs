using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LearningManagement.Startup))]
namespace LearningManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
