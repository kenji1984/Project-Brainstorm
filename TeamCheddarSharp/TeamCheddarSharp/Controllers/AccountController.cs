using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeamCheddarSharp.Models;
using System.Web.Security;

namespace TeamCheddarSharp.Controllers
{
    public class AccountController : Controller
    {
        TeamCheddarSharpEntities db = new TeamCheddarSharpEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserModel user)
        {
            if (ModelState.IsValid)
            {
                if (isValid(user.Name, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.Name, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login failed. Make sure username and password is correct.");
                }
            }
            return View(user);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                if (!alreadyExisted(user.Name))
                {
                    var crypto = new SimpleCrypto.PBKDF2();
                    var encryptPassword = crypto.Compute(user.Password);
                    var role = db.Codes.Where(c => c.Code_def == "Contributor").Select(
                        r => r.Code_id);
                    User userModel = db.Users.Create();

                    userModel.Name = user.Name;
                    if (user.Email != null)
                        userModel.Email = user.Email;
                    userModel.Password = encryptPassword;
                    userModel.PasswordSalt = crypto.Salt;
                    userModel.Role = role.FirstOrDefault();

                    db.Users.Add(userModel);
                    db.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username already taken.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Registration failed. Try a different name.");
            }
            return View(user);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            HttpContext.Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        private bool isValid(string name, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();
            bool isValid = false;

            var user = db.Users.SingleOrDefault(u => u.Name == name);
            {
                if (user != null)
                {
                    if (user.Password == crypto.Compute(password, user.PasswordSalt))
                    {
                        isValid = true;
                        //Problems here. Session["userLoggedIn"] is lost after a certain time 
                        //but Session["Role"] persisted.
                        UserModel currentUser = new UserModel();
                        currentUser.User_id = user.User_id;
                        currentUser.Name = name;
                        currentUser.Role = db.Codes.Where(
                            c => c.Code_id == user.Role).Select(r => r.Code_def).FirstOrDefault();
                        HttpContext.Session["userLoggedIn"] = currentUser;
                    }
                }
            }
            return isValid;
        }

        private bool alreadyExisted(string name)
        {
            bool found = false;
            var user = db.Users.SingleOrDefault(u => u.Name == name);

            if (user != null)
            {
                found = true;
            }
            else
            {
                found = false;
            }
            return found;
        }
    }
}