using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeamCheddarSharp.Models;

namespace TeamCheddarSharp.Views
{
    public class SchoolController : Controller
    {
        TeamCheddarSharpEntities db = new TeamCheddarSharpEntities();

        // GET: School
        public ActionResult Index()
        {
            IEnumerable<SchoolModel> model = null;
            model = (from s in db.Schools
                     select new SchoolModel
                     {
                         School_id = s.School_id,
                         Name = s.Name,
                         Address = s.Address,
                         Phone = s.Phone,
                         Contact = s.Contact
                     });
            return View(model.ToList());
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name, address, phone, contact")] SchoolModel schoolModel)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                School school = new School();
                school.Name = schoolModel.Name;
                school.Address = schoolModel.Address;
                school.Phone = schoolModel.Phone;
                school.Contact = schoolModel.Contact;

                db.Schools.Add(school);
                db.SaveChanges();
                return RedirectToAction("Index", "School");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            School school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }
            SchoolModel schoolModel= new SchoolModel();
            schoolModel.School_id = school.School_id;
            schoolModel.Name = school.Name;
            schoolModel.Address = school.Address;
            schoolModel.Phone = school.Phone;
            schoolModel.Contact = school.Contact;
            return View(schoolModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, [Bind(Include="name, address, phone, contact")]SchoolModel schoolModel)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                var school = db.Schools.Find(id);
                if (TryUpdateModel(schoolModel))
                {
                    school.Name= schoolModel.Name;
                    school.Address = schoolModel.Address;
                    school.Phone = schoolModel.Phone;
                    school.Contact = schoolModel.Contact;
                    db.SaveChanges();
                    return RedirectToAction("Index", "School");
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            School school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }
            return View(school);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSchool(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            School school = db.Schools.Find(id);
            db.Schools.Remove(school);
            db.SaveChanges();
            return RedirectToAction("Index", "School");
        }

        //param: school_id
        public ActionResult IdeasAssignedToSchool(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.SchoolName = db.Schools.Where(s => s.School_id == id).Select(s => s.Name).FirstOrDefault();
            IEnumerable<IdeaView> model = null;
            model = (from i in db.Ideas
                     join ai in db.Assigned_Idea on i.Idea_num equals ai.Idea_num
                     join s in db.Schools on ai.School_id equals s.School_id
                     where ai.School_id == id
                     select new IdeaView
                     {
                         Idea_num = ai.Idea_num,
                         Title = i.Title,
                         Summary = i.Summary,
                         SchoolName = s.Name,
                         Status = ai.Status,
                         Assigned_id = ai.Assigned_id,
                         User_id = ai.Ambassador_id,
                         AmbassadorName = db.Users.Where(u => u.User_id == ai.Ambassador_id).Select(
                            u => u.Name).FirstOrDefault()
                     });
            return View(model.ToList());
        }
    }
}