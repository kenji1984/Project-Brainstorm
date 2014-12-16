using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TeamCheddarSharp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostMapRequestHandler(object sender, EventArgs e)
        {
            if (Request.Cookies["ASP.NET_SessionIdTemp"] != null)
            {
                if (Request.Cookies["ASP.NET_SessionId"] == null)
                    Request.Cookies.Add(new HttpCookie("ASP.NET_SessionId",
                        Request.Cookies["ASP.NET_SessionIdTemp"].Value));
                else
                    Request.Cookies["ASP.NET_SessionId"].Value = Request.Cookies["ASP.NET_SessionIdTemp"].Value;
            }
        }

        protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            try
            {
                HttpCookie cookie = new HttpCookie("ASP.NET_SessionIdTemp", Session.SessionID);
                cookie.Expires = DateTime.Now.AddMinutes(Session.Timeout);
                Response.Cookies.Add(cookie);
            }
            catch (System.Web.HttpException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}