using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeamCheddarSharp.Models;
using System.Data;

namespace TeamCheddarSharp.Controllers
{
    public class AdminController : Controller
    {
        private HomeController home = new HomeController();

        private TeamCheddarSharpEntities db = new TeamCheddarSharpEntities();

        // GET: Admin
        public ActionResult Index()
        {
            return RedirectToAction("UserPanel", "Portal");
        }

        public ActionResult ViewArchive()
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            return View(db.Archives.ToList());
        }

        public ActionResult ArchiveDetails(int? id)
        {
            if(!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            Archive archive = db.Archives.Find(id);
            if (id == null)
            {
                return HttpNotFound();
            }
            return View(archive);
        }

        public ActionResult ListUsers(string name)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            if (!String.IsNullOrEmpty(name))
            {
                return View(db.Users.Where(u => u.Name.Contains(name)).ToList());
            }
            return View(db.Users.ToList());
        }

        public ActionResult ListAmbassadors()
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            return View(db.Users.Where(u => u.Role == db.Codes.Where(
                c => c.Code_def == "Ambassador").Select(c => c.Code_id).FirstOrDefault()).ToList());
        }

        public ActionResult ListContributors()
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            return View(db.Users.Where(u => u.Role == db.Codes.Where(
                c => c.Code_def == "Contributor").Select(c => c.Code_id).FirstOrDefault()).ToList());
        }

        [HttpGet]
        public ActionResult Appoint(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            if (id == null)
            {
                return RedirectToAction("ListUsers", "Admin");
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.roles = new SelectList(db.Codes.Where(c => c.Code_def == "Ambassador" ||
                c.Code_def == "Mentor" || c.Code_def == "Contributor" || c.Code_def == "Admin"),
                "Code_id", "Code_def", user.Role);
            
            return View(user);
        }

        [HttpPost, ActionName("Appoint")]
        public ActionResult AppointUser(int? id, int? roles)
        {
            User user = db.Users.Find(id);
            if (roles != null)
            {
                if (TryUpdateModel(user))
                {
                    user.Role = (int)roles;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("ListUsers", "Admin");
        }

        public ActionResult ViewAllAssignedIdeas(string search_idea)
        {
            IEnumerable<IdeaView> model = null;
            model = (from i in db.Ideas
                     join ai in db.Assigned_Idea on i.Idea_num equals ai.Idea_num
                     join s in db.Schools on ai.School_id equals s.School_id
                     select new IdeaView
                     {
                         Idea_num = ai.Idea_num,
                         Title = i.Title,
                         Summary = i.Summary,
                         SchoolName = s.Name,
                         School_id = s.School_id,
                         Status = ai.Status,
                         Assigned_id = ai.Assigned_id,
                         User_id = ai.Ambassador_id,
                         AmbassadorName = db.Users.Where(u => u.User_id == ai.Ambassador_id).Select(
                            u => u.Name).FirstOrDefault()
                     });
            if(!String.IsNullOrEmpty(search_idea))
            {
                model = model.Where(m => m.Title.Contains(search_idea) || m.Summary.Contains(search_idea));
            }
            return View(model.ToList());
        }

        public ActionResult ChangeStatus(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assigned_Idea ai = db.Assigned_Idea.Find(id);
            if (ai == null)
            {
                return HttpNotFound();
            }
            //Idea idea = db.Ideas.Find(ai.Idea_num);
            IdeaView ideaView = home.GenerateSingleIdeaView(ai.Idea_num);
            ideaView.Assigned_id = ai.Assigned_id;
            ViewBag.currentUserId = db.Users.Where(u => u.Name == User.Identity.Name).Select(u => u.User_id).FirstOrDefault();
            return View(ideaView);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int? id, string Status)
        {
            if (Status.Trim() == "")
            {
                return RedirectToAction("ChangeStatus", new { id = (int)id });
            }
            Assigned_Idea ai = db.Assigned_Idea.Find(id);
            ai.Status = Status;
            db.SaveChanges();
            return RedirectToAction("AssignedIdeas", "Home", new { id = ai.Idea_num});
        }

        public ActionResult AuditTrail(DateTime? start_date, DateTime? end_date)
        {
            ViewBag.start_date = start_date;
            ViewBag.end_date = end_date;

            IEnumerable<AuditTrailView> model = null;
            model = (from e in db.Event_Log
                     select new AuditTrailView
                     {
                         UserName = db.Users.Where(u => u.User_id == e.User_id).Select(u => u.Name).FirstOrDefault(),
                         Action = db.Codes.Where(c => c.Code_id == e.Action).Select(c => c.Code_def).FirstOrDefault(),
                         Title = e.Title,
                         Date = e.Access_date,
                         Assigned_id = e.Assigned_id
                     });


            DateTime end = new DateTime();
            if (start_date != null && end_date != null)
            {
                end = ((DateTime)end_date).AddDays(1);
                model = model.Where(m => m.Date >= start_date && m.Date <= end);
            }
            if (start_date != null && end_date == null)
            {
                model = model.Where(m => m.Date >= start_date);
            }
            if (start_date == null && end_date != null)
            {
                end = ((DateTime)end_date).AddDays(1);
                model = model.Where(m => m.Date <= end);
            }
            return View(model.ToList());
        }

        public ActionResult GenerateReport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateReport(ReportView reportView)
        {
            //only validate if All reports or Project reports are chosed
            if (reportView.ReportCategory == "All" || reportView.ReportCategory == "Project")
            {
                if (ModelState.IsValid)
                {
                    ReportView reports = ProjectReportByDate(reportView);
                    return View(reports);
                }
            }

            //for User and School reports, don't need to validate the date
            else
            {
                ReportView reports = ProjectReportByDate(reportView);
                return View(reports);
            }
            return View();
        }

        public PartialViewResult ListUser(string name)
        {
            List<User> result = db.Users.Where(u => u.Name.Contains(name)).ToList();
            return PartialView("_UserListPartial", result);
        }
        public PartialViewResult ListAmbassador(string name)
        {
            List<User> result = db.Users.Where(u => u.Name.Contains(name) && u.Role == db.Codes.Where(
                c => c.Code_def == "Ambassador").Select(c => c.Code_id).FirstOrDefault()).ToList();
            return PartialView("_UserListPartial", result);
        }
        public PartialViewResult ListContributor(string name)
        {
            List<User> result = db.Users.Where(u => u.Name.Contains(name) && u.Role == db.Codes.Where(
                c => c.Code_def == "Contributor").Select(c => c.Code_id).FirstOrDefault()).ToList();
            return PartialView("_UserListPartial", result);
        }

        /*********************************************************************************************************
         * 
         * 
         * 
         * *******************************************************************************************************/

        private IEnumerable<AuditTrailView> FilterByDate(DateTime start, DateTime end, IEnumerable<AuditTrailView> model)
        {
            end = end.AddDays(1);
            if (start != null && end != null)
            {
                model = model.Where(m => m.Date >= start && m.Date <= end);
            }
            if (start != null && end == null)
            {
                model = model.Where(m => m.Date >= start);
            }
            if (start == null && end != null)
            {
                model = model.Where(m => m.Date <= end);
            }
            return model;
        }

        private ReportView ProjectReportByDate(ReportView reportView)
        {
            ReportView reports = new ReportView();
            DateTime end_date = reportView.EndDate.AddDays(1);

            //projects report
            reports.ReportCategory = reportView.ReportCategory;
            reports.NumIdeas = db.Ideas.Count(i => i.Date_submitted >= reportView.StartDate &&
                i.Date_submitted <= end_date);
            reports.NumAssigned = db.Ideas.Count(i => i.Date_submitted >= reportView.StartDate &&
                i.Date_submitted <= end_date && i.Assigned == true);
            reports.NumSubmitted = db.Ideas.Count(i => i.Date_submitted >= reportView.StartDate &&
                i.Date_submitted <= end_date && i.Assigned == false);
            
            //project archived are based on creation date. Is it supposed to be based on the date it was archived?
            reports.NumArchived = db.Archives.Count(i => i.Date_submitted >= reportView.StartDate &&
                i.Date_submitted <= reportView.EndDate);

            var model = (from i in db.Ideas
                         join ai in db.Assigned_Idea on i.Idea_num equals ai.Idea_num
                         select new IdeaView
                         {
                             Date_submitted = i.Date_submitted,
                             Status = ai.Status
                         });
            reports.NumCompleted = model.Count(i => i.Date_submitted >= reportView.StartDate &&
                i.Date_submitted <= reportView.EndDate && i.Status == "Completed");
            reports.NumProg = model.Count(i => i.Date_submitted >= reportView.StartDate &&
                i.Date_submitted <= reportView.EndDate && i.Status == "In Progress");

            //school reports
            reports.NumSchools = db.Schools.Count();
            var modelTest = db.Assigned_Idea.Select(ai => ai.School_id);
            reports.NumSchoolsBusy = modelTest.Distinct().Count();

            //user reports
            reports.NumUsers = db.Users.Count();
            reports.NumContr = db.Users.Count(u => u.Role == db.Codes.Where(
                c => c.Code_def == "Contributor").Select(c => c.Code_id).FirstOrDefault());
            reports.NumAmbas = db.Users.Count(u => u.Role == db.Codes.Where(
                c => c.Code_def == "Ambassador").Select(c => c.Code_id).FirstOrDefault());
            reports.NumAdmins = db.Users.Count(u => u.Role == db.Codes.Where(
                c => c.Code_def == "Admin").Select(c => c.Code_id).FirstOrDefault());
            var userModel = db.Ideas.Select(i => i.User_id);
            reports.NumUserParticipate = userModel.Distinct().Count();
            return reports;
        }
    }
}