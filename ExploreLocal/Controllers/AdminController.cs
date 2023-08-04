using ExploreLocal.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
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
        ExploreLocalEntities5 db = new ExploreLocalEntities5();
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
       
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult ExpertRequests()
        {
            using (var db = new ExploreLocalEntities5())
            {
                var pendingExperts = db.Tbl_Expert.Where(e => e.ExpertStatus == false).ToList();
                return View(pendingExperts);
            }
        }

        public void SendExpertApprovalEmail(string recipientEmail)
        {
            string senderEmail = "khanmehroz245@gmail.com";
            string apiKey = "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";
            string subject = "Congratulations! You're Now an ExploreLocal Expert";
            string body = "Dear expert, congratulations! Your request to become an ExploreLocal expert has been approved. You can now add your services at ExploreLocal.";


            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail);
            var to = new EmailAddress(recipientEmail);
            var plainTextContent = body;
            var htmlContent = "<strong>" + body + "</strong>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg).Result;

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                Console.WriteLine($"Failed to send email: {response.StatusCode}");
            }
        }

        public ActionResult RejectExpert(int expertId)
        {
            using (var db = new ExploreLocalEntities5())
            {
                var expert = db.Tbl_Expert.Find(expertId);
                if (expert != null)
                {
                    db.Tbl_Expert.Remove(expert);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("ExpertRequests");
        }

        public ActionResult ExpertTourRequests()
        {
            using (var db = new ExploreLocalEntities5())
            {
                var pendingTourExperts = db.Tbl_Destination
                    .Where(e => e.TourStatus == false)
                    .ToList();

                // Fetch the associated Tbl_Expert for each pendingTourExpert
                foreach (var expert in pendingTourExperts)
                {
                    expert.Expert = db.Tbl_Expert.FirstOrDefault(e => e.ExpertId == expert.FK_Expert_Id);
                }

                return View(pendingTourExperts);
            }
        }

        public void SendExpertTourApprovalEmail(string recipientEmail)
        {
            string senderEmail = "khanmehroz245@gmail.com";
            string apiKey = "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";
            string subject = "Congratulations! To Our ExploreLocal Expert";
            string body = "Dear expert, congratulations! Your uploaded tour on ExploreLocal has been approved.";


            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail);
            var to = new EmailAddress(recipientEmail);
            var plainTextContent = body;
            var htmlContent = "<strong>" + body + "</strong>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg).Result;

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                Console.WriteLine($"Failed to send email: {response.StatusCode}");
            }
        }

        public ActionResult ApproveTour(int tourId)
        {
            Tbl_Destination tour = db.Tbl_Destination.Find(tourId);
            if (tour != null)
            {
                tour.TourStatus = true;
                var expert = db.Tbl_Expert.Find(tour.FK_Expert_Id);
                db.SaveChanges();
                SendExpertTourApprovalEmail(expert.ExpertEmail);
            }

            return RedirectToAction("ExpertTourRequests");
        }

        public ActionResult RejectTour(int tourId)
        {
            Tbl_Destination tour = db.Tbl_Destination.Find(tourId);
            if (tour != null)
            {
                db.Tbl_Destination.Remove(tour); // Remove the tour from the database (reject)
                db.SaveChanges();
            }

            return RedirectToAction("ExpertTourRequests");
        }

        public ActionResult AllExperts()
        {
            List<Tbl_Expert> ExpertList = db.Tbl_Expert.ToList();
            return View(ExpertList);
        }

        public ActionResult ExpertDelete(int expertId)
        {
            using (var db = new ExploreLocalEntities5())
            {
                var expert = db.Tbl_Expert.Find(expertId);
                if (expert != null)
                {
                    db.Tbl_Expert.Remove(expert);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("AllExperts");
        }

        public ActionResult ExpertBookings()
        {
            List<Tbl_Bookings> ExpertBookings = db.Tbl_Bookings.ToList();
            return View(ExpertBookings);
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
                ca.Venue_Country = cat.Venue_Country; 
                ca.Venue_img = path;
                ca.Venue_Description = cat.Venue_Description;
                ca.Admin_id = Convert.ToInt32(Session["ad_id"].ToString());
                db.Tbl_Venue.Add(ca);
                db.SaveChanges();
                return RedirectToAction("View_Venue");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Add_Annoucement()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Add_Annoucement(Tbl_Announcement ann, HttpPostedFileBase imgfile)
        {

            Tbl_Announcement an = new Tbl_Announcement();
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't Uploaded Try Again";
            }
            an.Announcement_headline = ann.Announcement_headline;
            an.Announcement_description = ann.Announcement_description;
            an.Announcement_image = path;
            db.Tbl_Announcement.Add(an);
            db.SaveChanges();

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
                ca.Venue_Country = category.Venue_Country;
                ca.Venue_Description = category.Venue_Description;
                if (imgfile != null)
                {
                    ca.Venue_img = path;
                }
                db.SaveChanges();
                return RedirectToAction("View_Venue");
            }
            return View(category);
        }

        public ActionResult RegisteredUser()
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