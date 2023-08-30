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
using System.Globalization;


namespace ExploreLocal.Controllers
{
    public class AdminController : Controller
    {
        ExploreLocalEntities2 db = new ExploreLocalEntities2();
        public ActionResult Layout()
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
        public ActionResult Admin_Panel()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }

            List<Tbl_Bookings> bookings = db.Tbl_Bookings.ToList();

            var revenueData = CalculateRevenueData(bookings);
            var expenseData = CalculateExpenseData(bookings);

            ViewBag.ToastMessage = TempData["ToastMessage"];

            ViewBag.RevenueData = revenueData;
            ViewBag.ExpenseData = expenseData;

            CultureInfo pkCulture = new CultureInfo("en-PK");
            ViewBag.Culture = pkCulture;

            double totalEarnings = CalculateTotalEarnings(bookings);
            ViewBag.TotalEarnings = totalEarnings.ToString("C", pkCulture); // Add "C" to format as currency

            double earningsThisMonth = CalculateEarningsThisMonth(bookings);
            ViewBag.EarningsThisMonth = earningsThisMonth.ToString("C", pkCulture); // Add "C" to format as currency

            double expenseThisMonth = CalculateExpenseThisMonth(bookings);
            ViewBag.ExpenseThisMonth = expenseThisMonth.ToString("C", pkCulture); // Add "C" to format as currency

            int currentYear = DateTime.Now.Year;
            double totalYearlyBookings = CalculateTotalYearlyBookings(bookings, currentYear);
            ViewBag.TotalYearlyBookings = totalYearlyBookings.ToString("C", pkCulture); // Add "C" to format as currency

            List<Tbl_Expert> ExpertList = db.Tbl_Expert.ToList();
            return View(ExpertList);
        }



        private double CalculateTotalYearlyBookings(List<Tbl_Bookings> bookings, int targetYear)
        {
            var yearlyBookings = bookings.Where(booking => booking.BookingDate.HasValue && booking.BookingDate.Value.Year == targetYear).ToList();
            double totalYearlyBookings = CalculateTotalEarnings(yearlyBookings);
            return totalYearlyBookings;
        }


        private double CalculateTotalEarnings(List<Tbl_Bookings> bookings)
        {
            var totalEarnings = bookings.Sum(booking =>
            {
                var destinationPrice = db.Tbl_Destination
                    .Where(d => d.DestinationID == booking.DestinationId)
                    .Select(d => d.Price)
                    .FirstOrDefault();

                return ((booking.NumberOfAdults ?? 0) * (double)destinationPrice) + ((booking.NumberOfChildren ?? 0) * (double)destinationPrice);
            });

            return totalEarnings;
        }

        private double CalculateEarningsThisMonth(List<Tbl_Bookings> bookings)
        {
            DateTime currentDate = DateTime.Now;
            var earningsThisMonth = bookings
                .Where(booking => booking.BookingDate.HasValue && booking.BookingDate.Value.Month == currentDate.Month)
                .Sum(booking =>
                {
                    var destinationPrice = db.Tbl_Destination
                        .Where(d => d.DestinationID == booking.DestinationId)
                        .Select(d => d.Price)
                        .FirstOrDefault();

                    return ((booking.NumberOfAdults ?? 0) * (double)destinationPrice) + ((booking.NumberOfChildren ?? 0) * (double)destinationPrice);
                });

            return earningsThisMonth;
        }

        private double CalculateExpenseThisMonth(List<Tbl_Bookings> bookings)
        {
            DateTime currentDate = DateTime.Now;
            double fixedExpenseRate = 10.0; // Replace with your actual fixed expense rate

            var expenseThisMonth = bookings
                .Where(booking => booking.BookingDate.HasValue && booking.BookingDate.Value.Month == currentDate.Month)
                .Sum(booking => (booking.NumberOfAdults ?? 0) + (booking.NumberOfChildren ?? 0) * fixedExpenseRate);

            return expenseThisMonth;
        }



        private List<double> CalculateRevenueData(List<Tbl_Bookings> bookings)
        {
            List<double> revenueData = new List<double>();

            foreach (var booking in bookings)
            {
                var destinationPrice = db.Tbl_Destination.Where(d => d.DestinationID == booking.DestinationId).Select(d => d.Price).FirstOrDefault();

                double revenue = ((booking.NumberOfAdults ?? 0) * (double)destinationPrice) + ((booking.NumberOfChildren ?? 0) * (double)destinationPrice);
                revenueData.Add(revenue);
            }

            return revenueData;
        }

        private List<double> CalculateExpenseData(List<Tbl_Bookings> bookings)
        {
            List<double> expenseData = new List<double>();

            // Define the fixed expense rate (modify this according to your needs)
            double fixedExpenseRate = 10.0; // Replace with your actual fixed expense rate

            foreach (var booking in bookings)
            {
                double expense = (booking.NumberOfAdults ?? 0) + (booking.NumberOfChildren ?? 0) * fixedExpenseRate;
                expenseData.Add(expense);
            }

            return expenseData;
        }

        public ActionResult AllExperts()
        {
            List<Tbl_Expert> ExpertList = db.Tbl_Expert.ToList();
            return View(ExpertList);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult ExpertRequests()
        {
            using (var db = new ExploreLocalEntities2())
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

        public ActionResult ApproveExpert(int expertId)
        {
            Tbl_Expert expert = db.Tbl_Expert.Find(expertId);
            if (expert != null)
            {
                expert.ExpertStatus = true;
                db.SaveChanges();
                SendExpertApprovalEmail(expert.ExpertEmail);
            }

            return RedirectToAction("ExpertTourRequests");
        }

        public ActionResult RejectExpert(int expertId)
        {
            using (var db = new ExploreLocalEntities2())
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
            using (var db = new ExploreLocalEntities2())
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

        public ActionResult ExpertDelete(int expertId)
        {
            using (var db = new ExploreLocalEntities2())
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

        public ActionResult View_Annoucement(int? page)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int pageSize = 6;
            int pageIndex = 1;

            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            var list = db.Tbl_Announcement.Where(x => x.Announcement_status == null).OrderByDescending(x => x.Announcement_id).ToList();
            IPagedList<Tbl_Announcement> cateList = list.ToPagedList(pageIndex, pageSize);

            return View(cateList);
        }

        [HttpGet]
        public ActionResult Edit_Annoucement(int id)
        {
            Tbl_Announcement category = db.Tbl_Announcement.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost]
        public ActionResult Save_Annoucement(Tbl_Announcement category, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't Uploaded Try Again";
            }
            else
            {
                Tbl_Announcement ca = db.Tbl_Announcement.Find(category.Announcement_id);
                ca.Announcement_headline = category.Announcement_headline;
                ca.Announcement_description = category.Announcement_description;
                if (imgfile != null)
                {
                    ca.Announcement_image = path;
                }
                db.SaveChanges();
                return RedirectToAction("View_Annoucement");
            }
            return View(category);
        }

        [HttpGet]
        public ActionResult TheDelete(int? id)
        {
            Tbl_Announcement ca = db.Tbl_Announcement.Where(x => x.Announcement_id == id).SingleOrDefault();
            return PartialView(ca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAnnoucementConfirmed(int? id)
        {
            Tbl_Announcement ca = db.Tbl_Announcement.FirstOrDefault(x => x.Announcement_id == id);
            if (ca != null)
            {
                db.Tbl_Announcement.Remove(ca);
                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
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

        public ActionResult Comments()
        {
            List<Tbl_Comments> userList = db.Tbl_Comments.ToList();
            return View(userList);
        }

        public ActionResult Replies()
        {
            List<Tbl_Replies> userList = db.Tbl_Replies.ToList();
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