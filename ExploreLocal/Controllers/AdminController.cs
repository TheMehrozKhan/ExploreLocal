using ExploreLocal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExploreLocal.Controllers
{
    public class AdminController : Controller
    {
        ExploreLocalEntities db = new ExploreLocalEntities();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Tbl_Admin adm)
        {
            Tbl_Admin ad = db.Tbl_Admin.Where(x => x.Admmin_Username == adm.Admmin_Username && x.Admin_Password == adm.Admin_Password).SingleOrDefault();
            if (ad != null)
            {
                Session["ad_id"] = ad.Admin_Id.ToString();
                Session["ad_name"] = ad.Admmin_Username;
                TempData["ToastMessage"] = "Hi, " + ad.Admmin_Username + " You Successfully Logged In!";
                return RedirectToAction("Admin_Panel");
            }
            else
            {
                ViewBag.Error = "Invalid Username or Password";
            }
            return View();
        }

        [HttpGet]
        public ActionResult Admin_Panel()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ToastMessage = TempData["ToastMessage"];
            return View();
        }
    }
}