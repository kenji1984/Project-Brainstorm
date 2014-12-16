using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TeamCheddarSharp.Models;

namespace TeamCheddarSharp.Controllers
{
    public class PortalController : Controller
    {
        TeamCheddarSharpEntities db = new TeamCheddarSharpEntities();

        // GET: Portal
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserPanel()
        {
            var currentUser = HttpContext.Session["userLoggedIn"];
            if (!Request.IsAuthenticated || currentUser == null)
            {
                FormsAuthentication.SignOut();
                HttpContext.Session.Abandon();
                return RedirectToAction("Login", "Account");
            }
            return View(currentUser);
        }
    }
}