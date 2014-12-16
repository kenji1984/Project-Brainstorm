using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TeamCheddarSharp.Models;

namespace TeamCheddarSharp.Controllers
{
    public class AmbassadorController : Controller
    {
        private TeamCheddarSharpEntities db = new TeamCheddarSharpEntities();
        private HomeController home = new HomeController();
        // GET: Ambassador
        public ActionResult Index()
        {
            return RedirectToAction("UserPanel", "Portal");
        }

        //param = ambassador_id
        public ActionResult AmbassadorIdeas(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            //keep track of current ambassadorIdea so we can return here on "back to list" link
            ViewBag.currAmbassadorID = db.Users.Where(u => u.Name == User.Identity.Name).Select(u => u.User_id).FirstOrDefault();
            ViewBag.AmbassadorName = db.Users.Where(u => u.User_id == id).Select(u => u.Name).FirstOrDefault();

            IEnumerable<IdeaView> model = null;
            model = (from i in db.Ideas
                     join ai in db.Assigned_Idea on i.Idea_num equals ai.Idea_num
                     join s in db.Schools on ai.School_id equals s.School_id
                     where ai.Ambassador_id == id
                     select new IdeaView
                     {
                         Assigned_id = ai.Assigned_id,
                         Title = i.Title,
                         Summary = i.Summary,
                         SchoolName = s.Name,
                         School_id = s.School_id,
                         Status = ai.Status,
                         Idea_num = ai.Idea_num
                     });
            return View(model.ToList());
        }

        //param = assigned project id
        public ActionResult Append(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (HttpContext.Session["userLoggedIn"] == null)
            {
                FormsAuthentication.SignOut();
            }
            Assigned_Idea idea = db.Assigned_Idea.Find(id);
            if (idea == null)
            {
                return HttpNotFound();
            }
            IdeaView ideaView = home.GenerateSingleIdeaView(idea.Idea_num);
            ViewBag.AttachedFiles = db.Files.Where(f => f.Idea_num == idea.Idea_num).ToList();
            ViewBag.Appends = GenerateAppendLogs(idea.Assigned_id).ToList();
            ViewBag.Ambassador_id = idea.Ambassador_id;
            return View(ideaView);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Append(int? id, DateTime appendDate, string appendText, string am)
        {
            if (!string.IsNullOrEmpty(appendText))
            {
                //get the ID of the user who appends the text
                int ambassador_id = db.Users.Where(u => u.Name == User.Identity.Name).Select(
                          u => u.User_id).FirstOrDefault();

                Append_Log append = new Append_Log();
                append.Idea_num = (int)id;
                append.Date_append = appendDate;
                append.Append_trail = appendText;
                append.User_id = ambassador_id; 
                db.Append_Log.Add(append);
                db.SaveChanges();
                return RedirectToAction("AmbassadorIdeas", "Ambassador", new { id = ambassador_id});
            }
            return RedirectToAction("Append", "Ambassador", new { id = id});
        }

        /*
         * Pre: assigned_id
         * Post: return the assigned project details as an IdeaView object
         * Note: assigned project has additional information such as school, ambassador, status, and appends
         *      in addition to all the attributes from idea. So need to join AssignedIdea, 
         *      Idea, User, School into IdeaView.
         *     Append_Log can't be join as it has N-1 relationship so Appends will be saved to ViewBag.Appends
         * */
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (HttpContext.Session["userLoggedIn"] == null)
            {
                FormsAuthentication.SignOut();
            }
            Assigned_Idea ai= db.Assigned_Idea.Find(id);
            if (ai == null)
            {
                return HttpNotFound();
            }
            
            //Create an IdeaView model
            IdeaView model = home.GenerateSingleIdeaView(ai.Idea_num);
            model.Assigned_id = ai.Assigned_id;

            //add school, ambassador, status to IdeaView model
            model.Status = ai.Status;
            model.AmbassadorName = db.Users.Where(u => u.User_id == ai.Ambassador_id).Select(
                            u => u.Name).FirstOrDefault();
            model.SchoolName = db.Schools.Where(s => s.School_id == ai.School_id).Select(
                s => s.Name).FirstOrDefault();

            //save attached files to ViewBag
            ViewBag.AttachedFiles = db.Files.Where(f => f.Idea_num == id).ToList();

            //save Append_Logs to ViewBag
            //ViewBag.Appends = db.Append_Log.Where(a => a.Idea_num == ai.Assigned_id).ToList();
            ViewBag.Appends = GenerateAppendLogs((int)id);
            ViewBag.currAmbassadorID = db.Users.Where(u => u.Name == User.Identity.Name).Select(u => u.User_id).FirstOrDefault();
            return View(model);
        }

        /*
         * Pre: assigned_id
         * Post: return list of AppendView of all the append logs for the assigned project with assigned_id param
         * */
        private IEnumerable<AppendView> GenerateAppendLogs(int assigned_id)
        {
            IEnumerable<AppendView> model = null;
            model = (from a in db.Append_Log
                     join u in db.Users on a.User_id equals u.User_id
                     where a.Idea_num == assigned_id
                     select new AppendView { 
                        Ambassador_id = u.User_id,
                        AmbassadorName = u.Name,
                        AppendDate = a.Date_append,
                        AppendText = a.Append_trail,
                        AssignedID = assigned_id
                     }).OrderBy(m => m.AppendDate);
            return model.ToList();
        }
    }
}