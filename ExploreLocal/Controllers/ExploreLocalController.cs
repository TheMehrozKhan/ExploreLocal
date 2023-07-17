using ExploreLocal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExploreLocal.Controllers
{
    public class ExploreLocalController : Controller
    {
        ExploreLocalEntities db = new ExploreLocalEntities();
        public ActionResult Index(Tbl_User us)
        {
            TempData["ToastMessage"] = "Hi, " + us.FirstName + " " +  us.LastName + " You Successfully Logged In!";
            ViewBag.ToastMessage = TempData["ToastMessage"];
            return View();
        }

        public ActionResult Destinations()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Expert()
        {
            return View();
        }

        public ActionResult HelpCenter()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Tbl_User uvm)
        {
            Tbl_User us = db.Tbl_User.FirstOrDefault(x => x.Email == uvm.Email && x.Password == uvm.Password);
            if (us != null)
            {
                Session["u_id"] = us.UserID.ToString();
                Session["u_email"] = us.Email;
                TempData["ToastMessage"] = "Hi, " + us.FirstName + us.LastName + " You Successfully Logged In!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Invalid Email or Password.");
                ViewBag.Error = "Invalid Email or Password.";
            }

            return View(uvm);
        }


        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Tbl_User us, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't Uploaded Try Again";
            }
            else
            {
                Tbl_User u = new Tbl_User();
                u.FirstName = us.FirstName;
                u.LastName = us.LastName;
                u.Email = us.Email;
                u.Password = us.Password;
                u.Location = us.Location;
                u.ProfileImage = path;
                db.Tbl_User.Add(u);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


        public string uploadimage(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {
                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                }
            }
            else
            {
                Response.Write("<script>alert('Please select a file'); </script>");
                path = "-1";

            }
            return path;
        }

        public ActionResult Terms()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
    }
}