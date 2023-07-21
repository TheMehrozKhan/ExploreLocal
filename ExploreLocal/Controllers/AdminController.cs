using ExploreLocal.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpGet]
        public ActionResult Add_Venue()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Add_Venue(Tbl_Venue cat, HttpPostedFileBase imgfile)
        {
            Tbl_Admin ad = new Tbl_Admin();
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't Uploaded Try Again";
            }
            else
            {
                Tbl_Venue ca = new Tbl_Venue();
                ca.Venue_name = cat.Venue_name;
                ca.Venue_Country = cat.Venue_Country; // We'll set this value based on user input

                ca.Venue_img = path;
                ca.Admin_id = Convert.ToInt32(Session["ad_id"].ToString());
                db.Tbl_Venue.Add(ca);
                db.SaveChanges();
                return RedirectToAction("View_Venue");
            }
            return View();
        }



        public ActionResult View_Venue(int? page)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int pageSize = 6;
            int pageIndex = 1;

            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            var list = db.Tbl_Venue.Where(x => x.Venue_status == null).OrderByDescending(x => x.Venue_id).ToList();
            IPagedList<Tbl_Venue> cateList = list.ToPagedList(pageIndex, pageSize);

            return View(cateList);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            Tbl_Venue ca = db.Tbl_Venue.Where(x => x.Venue_id == id).SingleOrDefault();
            return PartialView(ca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            Tbl_Venue ca = db.Tbl_Venue.FirstOrDefault(x => x.Venue_id == id);
            if (ca != null)
            {
                db.Tbl_Venue.Remove(ca);
                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
        public ActionResult Edit_Venue(int id)
        {
            Tbl_Venue category = db.Tbl_Venue.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost]
        public ActionResult Save_Venue(Tbl_Venue category, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't Uploaded Try Again";
            }
            else
            {
                Tbl_Venue ca = db.Tbl_Venue.Find(category.Venue_id);
                ca.Venue_name = category.Venue_name;
                if (imgfile != null)
                {
                    ca.Venue_img = path;
                }
                db.SaveChanges();
                return RedirectToAction("View_Venue");
            }
            return View(category);
        }

        [HttpGet]
        public ActionResult Create_Destination()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            List<Tbl_Venue> li = db.Tbl_Venue.ToList();
            ViewBag.categorylist = new SelectList(li, "Venue_id", "Venue_name");
            return View();
        }

        [HttpPost]
        public ActionResult Create_Destination(Tbl_Destination pr, HttpPostedFileBase[] imgfiles)
        {
            List<Tbl_Venue> li = db.Tbl_Venue.ToList();
            ViewBag.categorylist = new SelectList(li, "Venue_id", "Venue_name");
            List<string> imagePaths = new List<string>();

            if (imgfiles != null && imgfiles.Length > 0)
            {
                foreach (HttpPostedFileBase imgfile in imgfiles)
                {
                    string path = uploadimage(imgfile);

                    if (path.Equals(-1))
                    {
                        ViewBag.Error = "Image Couldn't Uploaded Try Again";
                        return View();
                    }

                    imagePaths.Add(path);
                }
            }

            Tbl_Destination pro = new Tbl_Destination();
            pro.DestinationName = pr.DestinationName;
            pro.Country = pr.Country;
            pro.Description = pr.Description;
            pro.Price = pr.Price;
            pro.FK_Venue_Id = pr.FK_Venue_Id;
            pro.US_Id_fk = Convert.ToInt32(Session["ad_id"].ToString());
            pro.GoogleStreetViewURL = pr.GoogleStreetViewURL;

            if (imagePaths.Count > 0)
            {
                pro.Image = string.Join(",", imagePaths);
            }

            db.Tbl_Destination.Add(pro);
            db.SaveChanges();

            return View();
        }
        public ActionResult RegisteredUser(int? id)
        {
            List<Tbl_User> userList = db.Tbl_User.ToList();
            return View(userList);
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
    }
}