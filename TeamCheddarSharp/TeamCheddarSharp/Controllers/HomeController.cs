using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using TeamCheddarSharp.Models;
using PagedList;

namespace TeamCheddarSharp.Controllers
{
    public class HomeController : Controller
    {
        private TeamCheddarSharpEntities db = new TeamCheddarSharpEntities();

        /*
         * Index page generate a list of IdeaView model that combines properties of Idea and User table
         * then filter the list based on search filter, page, and sort order
         * currentFilter is used to keep track of the filter while changing pages
         * sortOrder is passed as ViewBag to index view, which is send back to index controller
         * in controller, sortOrder switches to the other value based on the value passed 
         */
        public ActionResult Index(string sortOrder, string currentFilter, string search_idea, int? page)
        {
            //switch sortOrder's value if it's not null. if it's null, it takes the default sort
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSort = sortOrder == "title" ? "title_desc" : "title";
            ViewBag.DateSort = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.ContrSort = sortOrder == "contributor" ? "contributor_desc" : "contributor";

            //go back to page 1 if user performs a new search, else use the previous filter for search filter
            if (search_idea != null)
            {
                page = 1;
            }
            else
            {
                search_idea = currentFilter;
            }

            ViewBag.CurrentFilter = search_idea;

            IEnumerable<IdeaView> model = null;
            model = GenerateIdeaView(null);

            ViewBag.SearchResult = null;
            if (!String.IsNullOrEmpty(search_idea))
            {
                model = model.Where(m => m.Title.ToLower().Contains(search_idea.ToLower()) ||
                    m.Summary.ToLower().Contains(search_idea.ToLower()) ||
                    m.Description.ToLower().Contains(search_idea.ToLower()));
                ViewBag.SearchResult = search_idea;
            }

            model = SortBy(sortOrder, model);
          
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(model.ToPagedList(pageNumber, pageSize));
        }

        /*
         * Pre: user_id
         * Post: return a list of IdeaView for the user_id
         */
        public ActionResult ListProjectByUser(int? id)
        {
            IEnumerable<IdeaView> model = null;
            model = GenerateIdeaView(id);
            return View(model.ToList());
        }

        [HttpGet]
        public ActionResult Contribute()
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Contribute(IdeaView ideaView)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                int idea_num = addIdea(ideaView);
                ideaView.Idea_num = idea_num;
                LogEvent(ideaView, "Create");
                return RedirectToAction("Index");
            }
            return View();
        }

        /*
         * Pre: Idea_num (Project_id)
         * return the Idea with the same Idea_num together with all the attached files.
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
            Idea idea = db.Ideas.Find(id);
            if (idea == null)
            {
                return HttpNotFound();
            }
            ViewBag.AttachedFiles = db.Files.Where(f => f.Idea_num == id).ToList();
            return View(idea);
        }

        //param = file_id
        public FileContentResult Download(int? id)
        {
            byte[] fileData;
            string fileName;
            File file = db.Files.Find(id);
            fileData = (byte[])file.File_data.ToArray();
            fileName = file.File_name;

            return File(fileData, "Text", fileName);
        }

        //param: Idea_num
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
            Idea idea = db.Ideas.Find(id);
            if (idea == null)
            {
                return HttpNotFound();
            }
            if (isAlreadyAssigned(idea.Idea_num))
            {
                return RedirectToAction("Index", "Home");
            }
            IdeaView ideaView = GenerateSingleIdeaView((int)id);
            ViewBag.AttachedFiles = db.Files.Where(f => f.Idea_num == id).ToList();
            return View(ideaView);

        }

        /*
         * Find the idea from the database based on Idea_num
         * update its values to the new values
         * save database and add any files included during edit
         * */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IdeaView idea)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                var updatedIdea = db.Ideas.Find(idea.Idea_num);
                if (TryUpdateModel(idea))
                {
                    updatedIdea.Title = idea.Title;
                    updatedIdea.Summary = idea.Summary;
                    updatedIdea.Description = idea.Description;
                    updatedIdea.Justification = idea.Justification;
                    db.SaveChanges();
                    if (idea.Files.FirstOrDefault() != null)
                    {
                        SaveFiles(idea.Files, idea.Idea_num);
                    }
                    LogEvent(idea, "Edit");
                    return RedirectToAction("Index");
                }
            }
            ViewBag.AttachedFiles = db.Files.Where(f => f.Idea_num == idea.Idea_num).ToList();
            return View(idea);
        }

        //Param: file_id
        public ActionResult RemoveFile(int? id)
        {
            int proj_id = RemoveFile((int)id);
            return RedirectToAction("Edit", new { id = proj_id });
        }

        /*
         * Pre: Idea_num
         * Post: if an idea has been assigned
         *         return a list of all the assigned projects
         *       if it has not been assigned, go to Delete
         * ViewBag.id is used to pass the id to AssignedIdeas action method from ConfirmDelete view.
         * Why is ViewBag.assigned used? for malicious address poking? I don't remember
         */
        public ActionResult ConfirmDelete(int? id)
        {
            if (isAlreadyAssigned((int)id))
            {
                //ViewBag.assigned = true;
                ViewBag.id = (int)id;
                return View(db.Assigned_Idea.Where(ai => ai.Idea_num == (int)id).ToList());
            }
            return RedirectToAction("Delete", "Home", new { id = id });
        }

        /*
         * Pre: Idea_num
         * Post: The first time delete is clicked, confirm is null
         *      if a project has been assigned and user hasn't confirm, go to ConfirmDelete
         *      it it hasn't been assigned or user has confirm, go to DeletePost
         * */
        public ActionResult Delete(int? id, string confirm)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (isAlreadyAssigned((int)id) && confirm == null)
            {
                return RedirectToAction("ConfirmDelete", "Home");
                //return RedirectToAction("AssignedIdeas", "Home", new { id = (int)id });
            }
            Idea idea = db.Ideas.Find(id);
            if (idea == null)
            {
                return HttpNotFound();
            }
            IdeaView ideaView = GenerateSingleIdeaView((int)id);
            return View(ideaView);
        }

        /*
         * Pre: Idea_num, Archive checkbox value
         * Post: Remove any foreign key references: Files, Assigned Ideas, Append Logs
         *      then delete the main project. Also add project to Archive based on Archive value.
         * Note: This action method deletes the main project (which will deleted all assigned project referencing this)
         *      it does not delete assigned project separately.
         * */
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int? id, string remove)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            //create an IdeaView to log the event (LogEvent accepts IdeaView as parameter)
            IdeaView ideaView = new IdeaView();

            if (!String.IsNullOrEmpty(remove))
            {
                var AssignedIdeaList = db.Assigned_Idea.Where(ai => ai.Idea_num == (int)id).ToList();
                foreach (var assignIdea in AssignedIdeaList)
                {
                    ideaView.Idea_num = assignIdea.Idea_num;
                    ideaView.Assigned_id = assignIdea.Assigned_id;
                    ideaView.School_id = assignIdea.School_id;

                    //delete clones
                    db.Database.ExecuteSqlCommand("Delete from Append_Log where Idea_num = @p0", assignIdea.Assigned_id);
                    db.Assigned_Idea.Remove(assignIdea);
                    db.SaveChanges();

                    //log the clones
                    if (remove == "Archive")
                    {
                        LogEvent(ideaView, "Archive");
                    }
                    else
                    {
                        LogEvent(ideaView, "Delete");
                    }
                }
                db.Database.ExecuteSqlCommand("Delete from [File] where Idea_num = @p0", id);

                ideaView = new IdeaView();
                Idea idea = db.Ideas.Find(id);
                ideaView.Idea_num = idea.Idea_num;

                if (remove == "Archive")
                {
                    Archive(idea.Idea_num);
                    LogEvent(ideaView, "Archive");
                }
                else
                {
                    LogEvent(ideaView, "Delete");
                }

                //delete main
                db.Ideas.Remove(idea);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }

        /*
         * Pre: Idea_num
         * Post: Return a generated IdeaView of all the assigned projects that has the same ID as the main project
         * ViewBag.id(Idea_num) is passed to go back to to Details page from View
         * */
        public ActionResult AssignedIdeas(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            IEnumerable<IdeaView> model = null;
            model = (from i in db.Ideas
                     join ai in db.Assigned_Idea on i.Idea_num equals ai.Idea_num
                     join s in db.Schools on ai.School_id equals s.School_id
                     where ai.Idea_num == (int)id
                     select new IdeaView
                     {
                         Assigned_id = ai.Assigned_id,
                         Title = i.Title,
                         Summary = i.Summary,
                         SchoolName = s.Name,
                         School_id = s.School_id,
                         Status = ai.Status,
                         Idea_num = ai.Idea_num,
                         User_id = ai.Ambassador_id,
                         AmbassadorName = db.Users.Where(u => u.User_id == ai.Ambassador_id).Select(
                            u => u.Name).FirstOrDefault()
                     });
            ViewBag.id = id;
            return View(model.ToList());
        }

        /*
         * Pre: Assign_id
         * Post: return the IdeaView that contains properties from both Idea and Assigned Idea tables
         * */
        public ActionResult DeleteAssignedIdea(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            Assigned_Idea ai = db.Assigned_Idea.Find(id);
            Idea idea = db.Ideas.Find(ai.Idea_num);

            IdeaView ideaView = GenerateSingleIdeaView(ai.Idea_num);
            ideaView.Status = ai.Status;
            ideaView.SchoolName = db.Schools.Where(s => s.School_id == ai.School_id).Select(
                s => s.Name).SingleOrDefault();
            return View(ideaView);
        }

        /*
         * Pre: assign_id, Archive
         * Post: remove foreign key references: Append_Log.
         * remove assigned project, add to Archive if option is checked
         * if assigned project is the last one assigned, change the main project as not assigned
         * */
        [HttpPost, ActionName("DeleteAssignedIdea")]
        public ActionResult DeleteAssignedIdeaPost(int? id, string remove)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            IdeaView ideaView = new IdeaView();
            Assigned_Idea ai = db.Assigned_Idea.Find(id);
            if (!String.IsNullOrEmpty(remove))
            {
                ideaView.Idea_num = ai.Idea_num;
                ideaView.Assigned_id = ai.Assigned_id;
                ideaView.School_id = ai.School_id;

                db.Database.ExecuteSqlCommand("Delete from Append_Log where Idea_num = @p0", id);
                db.Assigned_Idea.Remove(ai);
                if (remove == "Archive")
                {
                    ArchiveAssigned((int)id);
                    LogEvent(ideaView, "Archive");
                }
                else
                {
                    LogEvent(ideaView, "Delete");
                }
                db.SaveChanges();

                //after delete an assigned project, if it's the last project deleted. mark original idea as not assigned.
                if (!isAlreadyAssigned(ai.Idea_num))
                {
                    Idea idea = db.Ideas.Find(ai.Idea_num);
                    idea.Assigned = false;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("AssignedIdeas", new { id = ai.Idea_num });
        }

        /*
         * Pre: Idea_num
         * Post: return to view the found project together with a list of schools it hasn't been assigned to
         *      and a list of all the ambassadors
         * */
        public ActionResult Assign(int? id)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Idea idea = db.Ideas.Find(id);
            if (idea == null)
            {
                return HttpNotFound();
            }
            //if project is already assigned to a school. don't show that school on assign list.
            if (isAlreadyAssigned(idea.Idea_num))
            {
                List<int> school_ids = db.Assigned_Idea.Where(a => a.Idea_num == id).Select(a => a.School_id).ToList();
                ViewBag.schools = new SelectList(db.Schools.Where(s => !school_ids.Contains(s.School_id)), "School_id", "Name");
            }
            else
            {
                ViewBag.schools = new SelectList(db.Schools, "School_id", "Name");
            }
            ViewBag.ambassadors = new SelectList(db.Users.Where(u => u.Role == db.Codes.Where(
                c => c.Code_def == "Ambassador").Select(c => c.Code_id).FirstOrDefault()), "User_id", "Name");
            return View(idea);
        }

        /*
         * Pre: Idea_num, school_id, ambassador_id
         * Post: since schools and ambassadors are passed as ViewBag item instead of strongly-typed model
         *      validate by checking if either school or ambassador is null
         *      if either one is null, redirect back to AssignGet with the same Idea_num
         *      if both are not null, add a new record to AssignedIdea table and mark the main project "assigned"
         *      Redirect to the assigned project view
         * */
        [HttpPost]
        public ActionResult Assign(int? id, int? schools, int? ambassadors)
        {
            if (!Request.IsAuthenticated || HttpContext.Session["userLoggedIn"] == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            if (schools != null && ambassadors != null)
            {
                Assigned_Idea assignedIdea = new Assigned_Idea();
                assignedIdea.Idea_num = (int)id;
                assignedIdea.Ambassador_id = (int)ambassadors;
                assignedIdea.School_id = (int)schools;
                assignedIdea.Status = "Assigned";
                db.Assigned_Idea.Add(assignedIdea);

                //after assigned, mark the main idea as assigned.
                Idea idea = db.Ideas.Find(assignedIdea.Idea_num);
                idea.Assigned = true;

                db.SaveChanges();
                return RedirectToAction("AssignedIdeas", "Home", new { id = id });
            }
            return RedirectToAction("Assign", "Home", new { id = id });
        }


        /*
         ******************************************************************************************************
         ******************************************************************************************************
         */

        //convert an IdeaView object to an Idea object and add to the database
        private int addIdea(IdeaView ideaView)
        {
            Idea idea = new Idea();
            idea.Assigned = false;
            idea.Title = ideaView.Title;
            idea.Summary = ideaView.Summary;
            idea.Description = ideaView.Description;
            idea.Justification = ideaView.Justification;
            idea.Date_submitted = ideaView.Date_submitted;
            idea.User_id = ideaView.User_id;
            db.Ideas.Add(idea);
            db.SaveChanges();

            if (ideaView.Files.FirstOrDefault() != null)
            {
                SaveFiles(ideaView.Files, idea.Idea_num);
            }
            return idea.Idea_num;
        }

        //returns true or false if the project with Idea_num is assigned
        private bool isAlreadyAssigned(int id)
        {
            var idea = db.Assigned_Idea.Where(ai => ai.Idea_num == id).FirstOrDefault();
            if (idea == null)
            {
                return false;
            }
            return true;
        }

        //Add all the properties of a project with certain Idea_num to the archive table
        //Note: idea_num is not saved inside Archive and Archive doesn't reference any non-archive tables
        private void Archive(int id)
        {
            Idea idea = db.Ideas.Find(id);
            Archive archive = new Archive();
            archive.Title = idea.Title;
            archive.Summary = idea.Summary;
            archive.Description = idea.Description;
            archive.Author = db.Users.Where(u => u.User_id == idea.User_id).Select(u => u.Name).SingleOrDefault();
            archive.Date_submitted = idea.Date_submitted;
            db.Archives.Add(archive);
            db.SaveChanges();
        }

        //Add all the properties of a project with certain assigned_id to the archive table
        //Note: assigned_id is not saved inside Archive and Archive doesn't reference any non-archive tables
        public void ArchiveAssigned(int id)
        {
            Assigned_Idea assignedIdea = db.Assigned_Idea.Find(id);
            Idea idea = db.Ideas.Find(assignedIdea.Idea_num);
            Archive archive = new Archive();
            archive.Title = idea.Title;
            archive.Summary = idea.Summary;
            archive.Description = idea.Description;
            archive.Author = db.Users.Where(u => u.User_id == idea.User_id).Select(u => u.Name).SingleOrDefault();
            archive.Date_submitted = idea.Date_submitted;
            archive.Status = assignedIdea.Status;
            archive.School = db.Schools.Where(s => s.School_id == assignedIdea.School_id).Select(
                s => s.Name).FirstOrDefault();
            db.Archives.Add(archive);
            db.SaveChanges();
        }

        /*
         * Pre: a list of files in any format, and idea_num of the project the file is attached with
         * Post: add each file as a separate record in File table with idea_num as foreign key
         * */
        private void SaveFiles(IEnumerable<HttpPostedFileBase> files, int idea_num)
        {
            File file = new File();
            foreach (var item in files)
            {
                byte[] uploadFile = new byte[item.InputStream.Length];
                item.InputStream.Read(uploadFile, 0, uploadFile.Length);

                file.File_name = System.IO.Path.GetFileName(item.FileName);
                file.File_data = uploadFile;
                file.Idea_num = idea_num;
                db.Files.Add(file);
                db.SaveChanges();
            }
        }

        //delete file and return id of the project containing that file
        private int RemoveFile(int file_id)
        {
            int project_id;
            File file = db.Files.Find(file_id);
            project_id = file.Idea_num;
            db.Files.Remove(file);
            db.SaveChanges();
            return project_id;
        }

        /*
         * Pre: a sort string and a list of ideas
         * Post: returns the sorted list according to sort string
         * */
        private IEnumerable<IdeaView> SortBy(string sortOrder, IEnumerable<IdeaView> model)
        {
            switch (sortOrder)
            {
                case "title": 
                    return(model.OrderBy(m => m.Title));
                case "title_desc": 
                    return (model.OrderByDescending(m => m.Title));
                case "date": 
                    return(model.OrderBy(m => m.Date_submitted));
                case "date_desc": 
                    return (model.OrderByDescending(m => m.Date_submitted));
                case "contributor": 
                    return(model.OrderBy(m => m.UserName));
                case "contributor_desc": 
                    return (model = model.OrderByDescending(m => m.UserName));
                default: 
                    return(model.OrderByDescending(m => m.Date_submitted));
            }
        }

        /*
         * Pre: idea_num for non assigned project, idea_num + assigned_id and school_id for assigned project.
         * */
        private void LogEvent(IdeaView ideaView, string action)
        {
            Event_Log log = new Event_Log();
            //get user_id from userLoggedIn session
            log.User_id = db.Users.Where(u => u.Name == User.Identity.Name).Select(u => u.User_id).FirstOrDefault();
            log.Idea_num = ideaView.Idea_num;
            log.Assigned_id = ideaView.Assigned_id;
            log.Title = db.Ideas.Where(i => i.Idea_num == ideaView.Idea_num).Select(i => i.Title).FirstOrDefault();
            log.School_id = ideaView.School_id;
            log.Access_date = DateTime.Now;
            log.Action = db.Codes.Where(c => c.Code_def == action).Select(c => c.Code_id).FirstOrDefault();
            db.Event_Log.Add(log);
            db.SaveChanges();
        }

        /*
         * Pre: idea_num
         * Post: Find the idea using idea_num
         *      copy idea's properties to IdeaView object and return IdeaView object
         *      Properties included are: Idea_num, Title, Summary, Description, Username,
         *       Date_submitted, User_id, Assigned
         * */
        public IdeaView GenerateSingleIdeaView(int id)
        {
            Idea idea = db.Ideas.Find(id);
            IdeaView ideaView = new IdeaView();
            ideaView.Idea_num = idea.Idea_num;
            ideaView.Title = idea.Title;
            ideaView.Summary = idea.Summary;
            ideaView.Description = idea.Description;
            ideaView.Justification = idea.Justification;
            ideaView.UserName = db.Users.Where(u => u.User_id == idea.User_id).Select(
                                    i => i.Name).FirstOrDefault();
            ideaView.Date_submitted = idea.Date_submitted;
            ideaView.User_id = idea.User_id;
            ideaView.Assigned = idea.Assigned;
            return ideaView;
        }

        /*
         * Pre: user_id
         * Post: generate a list of IdeaView objects with necessary properties (Ideas and Users)
         *      if user_id is null, return a list of all the objects
         *      if user_id is not null, return a list of all the objects that has the same user_id
         * */
        private IEnumerable<IdeaView> GenerateIdeaView(int? id)
        {
            IEnumerable<IdeaView> model = null;
            if (id != null)
            {
                model = (from i in db.Ideas
                         join u in db.Users on i.User_id equals u.User_id
                         where i.User_id == id
                         select new IdeaView
                         {
                             Idea_num = i.Idea_num,
                             Title = i.Title,
                             Summary = i.Summary,
                             Description = i.Description,
                             Justification = i.Justification,
                             Date_submitted = i.Date_submitted,
                             UserName = u.Name,
                             User_id = i.User_id,
                             Assigned = i.Assigned,
                             NumOfClones = db.Assigned_Idea.Count(ai => ai.Idea_num == i.Idea_num)
                         });
            }
            else
            {
                model = (from i in db.Ideas
                         join u in db.Users on i.User_id equals u.User_id
                         select new IdeaView
                         {
                             Idea_num = i.Idea_num,
                             Title = i.Title,
                             Summary = i.Summary,
                             Description = i.Description,
                             Justification = i.Justification,
                             Date_submitted = i.Date_submitted,
                             UserName = u.Name,
                             User_id = i.User_id,
                             Assigned = i.Assigned,
                             NumOfClones = db.Assigned_Idea.Where(ai => ai.Idea_num == i.Idea_num).Count()
                         });
            }
            return model;
        }

        public JsonResult SearchIdea(string term)
        {
            List<string> ideas = db.Ideas.Where(i => i.Title.Contains(term)).Select(i => i.Title).ToList();
            return Json(ideas, JsonRequestBehavior.AllowGet);
        }
    }
}
