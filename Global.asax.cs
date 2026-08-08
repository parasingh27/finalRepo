using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace LearningManagement
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

       
    }
}
//below is added by you
//protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
//{
//    var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
//    if (authCookie != null)
//    {
//        var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
//        string[] roles = { authTicket.UserData };
//        var userPrincipal = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);
//        HttpContext.Current.User = userPrincipal;
//    }
//}