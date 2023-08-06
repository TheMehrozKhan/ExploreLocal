using ExploreLocal.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iText.Kernel.Pdf;
using iText.Html2pdf;

namespace ExploreLocal.Controllers
{
    public class ExploreLocalController : Controller
    {
        ExploreLocalEntities db = new ExploreLocalEntities();
        public ActionResult Index(Tbl_User user)
        {
            TempData["ToastMessage"] = "Hi, " + user.FirstName + " " + user.LastName + " You Successfully Logged In!";
            ViewBag.ToastMessage = TempData["ToastMessage"];

            // Retrieve the list of venues from the database
            using (var db = new ExploreLocalEntities())
            {
                List<Tbl_Venue> venues = db.Tbl_Venue.ToList();

                // Create the view model and pass the user and venues to the view
                var viewModel = new IndexViewModel
                {
                    User = user,
                    Venues = venues
                };

                return View(viewModel);
            }
        }


        public ActionResult Destinations(int id)
        {
            // Fetch the selected venue tours and venue details
            var selectedVenueTours = db.Tbl_Destination.Where(t => t.FK_Venue_Id == id && t.TourStatus == true).ToList();
            var selectedVenue = db.Tbl_Venue.FirstOrDefault(v => v.Venue_id == id);

            // Fetch most popular tours
            var mostPopularTours = db.Tbl_Destination
                .GroupJoin(
                    db.Tbl_BookingHistory,
                    destination => destination.DestinationID,
                    booking => booking.DestinationId,
                    (destination, bookings) => new
                    {
                        Destination = destination,
                        BookingsCount = bookings.Count()
                    })
                .Where(x => x.Destination.TourStatus == true) // Add condition for approved tours
                .OrderByDescending(x => x.BookingsCount)
                .Select(x => x.Destination)
                .Take(4)
                .ToList();

            // Fetch trending tours
            var trendingTours = db.Tbl_Destination
                .GroupJoin(
                    db.Tbl_BookingHistory,
                    destination => destination.DestinationID,
                    booking => booking.DestinationId,
                    (destination, bookings) => new
                    {
                        Destination = destination,
                        LatestBookingDate = bookings.Max(b => b.BookingDate)
                    })
                .Where(x => x.Destination.TourStatus == true) // Add condition for approved tours
                .OrderByDescending(x => x.LatestBookingDate)
                .Select(x => x.Destination)
                .Take(4)
                .ToList();

            var announcements = db.Tbl_Announcement.Where(t => t.Announcement_status == null).ToList();

            var viewModel = new ToursViewModel
            {
                SelectedVenueTours = selectedVenueTours,
                MostPopularTours = mostPopularTours,
                TrendingTours = trendingTours,
                SelectedVenueName = selectedVenue?.Venue_name,
                SelectedVenueDescription = selectedVenue?.Venue_Description,
                Announcement = announcements
            };

            return View(viewModel);
        }



        public ActionResult DestinationDetails(int id)
        {
            // Retrieve destination details
            Tbl_Destination p = db.Tbl_Destination.Where(x => x.DestinationID == id).SingleOrDefault();

            var destinationDetailsViewModel = new DestinationDetailsViewModel
            {
                DestinationID = p.DestinationID,
                DestinationName = p.DestinationName,
                Country = p.Country,
                Description = p.Description,
                Image = p.Image,
                Price = p.Price,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                GoogleStreetViewURL = p.GoogleStreetViewURL,
                MeetingPoint = p.MeetingPoint,
                Language = p.Language,
                Destination_Duration = p.Destination_Duration,
                Destination_Highlights = p.Destination_Highlights
            };

            // Retrieve expert details
            Tbl_Expert expert = db.Tbl_Expert.Where(e => e.ExpertId == p.FK_Expert_Id).SingleOrDefault();
            if (expert != null)
            {
                destinationDetailsViewModel.ExpertName = expert.ExpertName;
                destinationDetailsViewModel.ExpertProfileImage = expert.ExpertProfileImage;
                destinationDetailsViewModel.ExpertId = expert.ExpertId;
            }
            else
            {
                destinationDetailsViewModel.ExpertName = "No Expert Found";
                destinationDetailsViewModel.ExpertProfileImage = "./Content/img/Default.png";
            }

            // Retrieve venue details
            Tbl_Venue venue = db.Tbl_Venue.Where(x => x.Venue_id == p.FK_Venue_Id).SingleOrDefault();
            if (venue != null)
            {
                destinationDetailsViewModel.Venue_name = venue.Venue_name;
            }
            else
            {
                destinationDetailsViewModel.Venue_name = "No Venue Found";
            }

            var randomTours = db.Tbl_Destination.Where(x => x.DestinationID != id).OrderBy(x => Guid.NewGuid()).Take(4).ToList();
            ViewBag.RandomTours = randomTours;

            // Pass the view model to the view
            return View(destinationDetailsViewModel);
        }

        [HttpGet]
        public ActionResult BookingForm(int destinationId, int expertId, int userId)
        {
            // Check if the user is logged in
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            // Create the BookingViewModel with the destinationId, expertId, and userId
            var viewModel = new BookingViewModel
            {
                DestinationId = destinationId,
                ExpertID = expertId,
                UserId = userId
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SubmitBooking(BookingViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new ExploreLocalEntities())
                    {
                        // Create a new instance of the Tbl_Bookings model and set the BookingId
                        var booking = new Tbl_Bookings
                        {
                            DestinationId = viewModel.DestinationId,
                            ExpertID = viewModel.ExpertID,
                            UserId = viewModel.UserId,
                            NumberOfAdults = viewModel.NumberOfAdults,
                            NumberOfChildren = viewModel.NumberOfChildren,
                            BookingDate = viewModel.BookingDate,
                            CreditCardNumber = viewModel.CreditCardNumber,
                            PIN = viewModel.PIN,
                            CVV = viewModel.CVV,
                            CardHolderName = viewModel.CardHolderName,
                            FullName = viewModel.FullName,
                            Email = viewModel.Email,
                            ContactNumber = viewModel.ContactNumber
                        };

                        // Save the booking to the database
                        db.Tbl_Bookings.Add(booking);
                        db.SaveChanges();

                        // Redirect the user to the BookingSuccess view
                        return RedirectToAction("BookingSuccess", new { id = booking.BookingId });
                    }
                }
                catch (Exception ex)
                {
                    // If there's an error, you can handle it here or display an error message to the user
                    ViewBag.Error = "An error occurred while submitting the booking: " + ex.Message;
                }
            }

            // If the model is not valid or there's an error, return the BookingForm view with the provided data so the user can correct any issues
            return View("BookingForm", viewModel);
        }

        public ActionResult BookingSuccess(int? id)
        {
            if (id == null)
            {
                // If the booking ID is not provided, return a redirect to an error page or handle it as you prefer
                return RedirectToAction("Error");
            }

            using (var db = new ExploreLocalEntities())
            {
                var booking = db.Tbl_Bookings.Find(id);
                if (booking != null)
                {
                    // Retrieve the user's information based on the UserId associated with the booking
                    var user = db.Tbl_User.Find(booking.UserId);

                    // Retrieve the destination information based on the DestinationId associated with the booking
                    var destination = db.Tbl_Destination.Find(booking.DestinationId);

                    // Retrieve the expert information based on the ExpertID associated with the booking
                    var expert = db.Tbl_Expert.Find(booking.ExpertID);

                    // Controller code
                    if (user != null && destination != null && expert != null)
                    {
                        ViewBag.Booking = booking;
                        ViewBag.TheBookingid = booking.BookingId;
                        string bookingDateStr = ((DateTime)booking.BookingDate).ToString("yyyy-MM-dd");
                        ViewBag.BookingDateStr = bookingDateStr;
                        ViewBag.User = user;
                        ViewBag.Destination = destination;
                        ViewBag.Expert = expert;
                    }
                }
            }
            return View();
        }

        public ActionResult DownloadInvoice(int? bookingId)
        {
            if (bookingId == null)
            {
                // If the booking ID is not provided, return a redirect to an error page or handle it as you prefer
                return RedirectToAction("Error");
            }

            int bookingIdValue = bookingId.Value;

            // Retrieve the booking details and associated data from the database
            using (var db = new ExploreLocalEntities())
            {
                var booking = db.Tbl_Bookings.Find(bookingIdValue); // Use the extracted int value
                if (booking == null)
                {
                    // If the booking is not found, return a redirect to an error page or handle it as you prefer
                    return RedirectToAction("Error");
                }

                // Retrieve the user's information based on the UserId associated with the booking
                var user = db.Tbl_User.Find(booking.UserId);

                // Retrieve the destination information based on the DestinationId associated with the booking
                var destination = db.Tbl_Destination.Find(booking.DestinationId);

                // Retrieve the expert information based on the ExpertID associated with the booking
                var expert = db.Tbl_Expert.Find(booking.ExpertID);

                // Controller code
                // ...
                if (user != null && destination != null && expert != null)
                {
                    ViewBag.Booking = booking;
                    ViewBag.User = user;
                    ViewBag.Destination = destination;
                    ViewBag.Expert = expert;

                    // Render the view to a string without passing the viewModel
                    string htmlContent = RenderViewToString("InvoiceTemplate");

                    // Generate the PDF from the HTML content
                    byte[] invoicePdf = GenerateInvoicePdf(htmlContent);
                    return File(invoicePdf, "application/pdf", "Invoice.pdf");
                }
                // ...

            }

            return RedirectToAction("Error"); // If something went wrong, redirect to an error page or handle it as you prefer
        }


        // Helper method to generate the invoice PDF
        private byte[] GenerateInvoicePdf(string htmlContent)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ConverterProperties converterProperties = new ConverterProperties();
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdfDoc = new PdfDocument(writer);
                HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, converterProperties);

                // Close the document after converting the HTML content
                pdfDoc.Close();

                // Return the contents of the MemoryStream
                return ms.ToArray();
            }
        }


        private string RenderViewToString(string viewName)
        {
            ViewData.Model = this.ViewData.Model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult InvoiceTemplate(int? bookingId)
        {
            if (bookingId == null)
            {
                // If the booking ID is not provided, return a redirect to an error page or handle it as you prefer
                return RedirectToAction("Error");
            }

            using (var db = new ExploreLocalEntities())
            {
                var booking = db.Tbl_Bookings.Find(bookingId);
                if (booking != null)
                {
                    // Retrieve the user's information based on the UserId associated with the booking
                    var user = db.Tbl_User.Find(booking.UserId);

                    // Retrieve the destination information based on the DestinationId associated with the booking
                    var destination = db.Tbl_Destination.Find(booking.DestinationId);

                    // Retrieve the expert information based on the ExpertID associated with the booking
                    var expert = db.Tbl_Expert.Find(booking.ExpertID);

                    // Controller code
                    if (user != null && destination != null && expert != null)
                    {
                        ViewBag.Booking = booking;
                        ViewBag.User = user;
                        ViewBag.Destination = destination;
                        ViewBag.Expert = expert;
                    }
                }
            }

            ViewBag.TheBookingid = bookingId;

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
                Session["u_name"] = us.FirstName;
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
        public ActionResult ExpertLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExpertLogin(Tbl_Expert adm)
        {
            Tbl_Expert ad = db.Tbl_Expert
                            .Where(x => x.ExpertEmail == adm.ExpertEmail && x.ExperPassword == adm.ExperPassword)
                            .SingleOrDefault();

            if (ad != null)
            {
                if (ad.ExpertStatus.HasValue && ad.ExpertStatus.Value == false)
                {
                    ViewBag.NeedsApproval = true;
                    return View();
                }

                Session["expert_id"] = ad.ExpertId.ToString();
                Session["expert_name"] = ad.ExpertName;
                TempData["ToastMessage"] = "Hi, " + ad.ExpertName + " You Successfully Logged In!";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = "Invalid Username or Password";
                ViewBag.NeedsApproval = false;
            }

            return View();
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

        [HttpGet]
        public ActionResult ExpertRegistration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExpertRegistration(ExpertViewModel model)
        {
            if (ModelState.IsValid)
            {
                Tbl_Expert expert = new Tbl_Expert
                {
                    ExpertName = model.ExpertName,
                    ExpertEmail = model.ExpertEmail,
                    ExpertBio = model.ExpertBio,
                    ExperPassword = model.ExperPassword,
                    ExpertLocation = model.ExpertLocation,
                    ExpertStatus = Convert.ToBoolean(0)
                };

                using (var db = new ExploreLocalEntities())
                {
                    db.Tbl_Expert.Add(expert);
                    db.SaveChanges();
                }

                return RedirectToAction("ExpertRegistrationSuccess");
            }

            return View(model);
        }

        public ActionResult ExpertRegistrationSuccess()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create_Tour()
        {
            if (Session["expert_id"] == null)
            {
                return RedirectToAction("ExpertLogin");
            }

            List<Tbl_Venue> li = db.Tbl_Venue.ToList();
            ViewBag.categorylist = new SelectList(li, "Venue_id", "Venue_name");
            return View();
        }

        [HttpPost]
        public ActionResult Create_Tour(Tbl_Destination pr, HttpPostedFileBase[] imgfiles)
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

            Tbl_Destination pro = new Tbl_Destination
            {
                DestinationName = pr.DestinationName,
                Price = pr.Price,
                Country = pr.Country,
                Description = pr.Description,
                MeetingPoint = pr.MeetingPoint,
                Language = pr.Language,
                FK_Venue_Id = pr.FK_Venue_Id,
                GoogleStreetViewURL = pr.GoogleStreetViewURL,
                StartDate = pr.StartDate,
                EndDate = pr.EndDate,
                Destination_Duration = pr.Destination_Duration,
                Destination_Highlights = pr.Destination_Highlights,
                FK_Expert_Id = Convert.ToInt32(Session["expert_id"].ToString()),
                TourStatus = false // Set the status to false (pending) when the tour is uploaded
            };

            if (imagePaths.Count > 0)
            {
                pro.Image = string.Join(",", imagePaths);
            }

            db.Tbl_Destination.Add(pro);
            db.SaveChanges();

            return View();
        }

        public ActionResult Venues(int? page)
        {
            int pageSize = 8;
            int pageIndex = 1;

            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            var list = db.Tbl_Venue.Where(x => x.Venue_status == null).OrderByDescending(x => x.Venue_id).ToList();
            IPagedList<Tbl_Venue> cateList = list.ToPagedList(pageIndex, pageSize);

            return View(cateList);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult UserProfile(int? id)
        {
            int userId = Convert.ToInt32(Session["u_id"]);

            if (id == null)
            {
                id = userId;
            }

            Tbl_User profileUser = db.Tbl_User.FirstOrDefault(u => u.UserID == id);

            if (profileUser == null)
            {
                ViewBag.ProfileNotFound = true;
                return View();
            }

            // Retrieve booked tours for the user
            List<Tbl_Bookings> userBookings = db.Tbl_Bookings
                .Where(b => b.UserId == id)
                .ToList();

            // Fetch the associated Destination for each booking
            foreach (var booking in userBookings)
            {
                booking.Tbl_Destination = db.Tbl_Destination.Find(booking.DestinationId);

                // Determine the tour state based on start and end dates
                booking.TourState = GetTourState(booking.Tbl_Destination.StartDate, booking.Tbl_Destination.EndDate);
            }

            // Pass the user and their bookings to the view
            ViewBag.ProfileNotFound = false; // Set to false when profileUser is found
            ViewBag.UserBookings = userBookings;
            return View(profileUser);
        }

        private string GetTourState(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
            {
                return "N/A";
            }

            DateTime currentDate = DateTime.Now;

            if (currentDate < startDate)
            {
                return "Upcoming";
            }
            else if (currentDate >= startDate && currentDate <= endDate)
            {
                return "Ongoing";
            }
            else
            {
                return "Completed";
            }
        }

        [HttpGet]
        public ActionResult Edit_Profile(int id)
        {
            Tbl_User user = db.Tbl_User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Save_Edit(Tbl_User user, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't be Uploaded. Please try again.";
            }
            else
            {
                Tbl_User us = db.Tbl_User.Find(user.UserID);
                if (us != null)
                {
                    us.FirstName = user.FirstName;
                    us.LastName = user.LastName;
                    us.Email = user.Email;
                    if (imgfile != null)
                    {
                        us.ProfileImage = path;
                    }
                    us.Location = user.Location;
                    db.SaveChanges();
                    return RedirectToAction("UserProfile", new { us.UserID });
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(user);
        }

        public ActionResult ExpertDashboard(int? id)
        {
            int expertId;
            if (id == null || (int)id == Convert.ToInt32(Session["expert_id"]))
            {
                // If id is not provided or it's the currently logged-in expert, use the logged-in expert's ID
                expertId = Convert.ToInt32(Session["expert_id"]);
            }
            else
            {
                // Otherwise, use the provided expert ID
                expertId = (int)id;
            }

            // Retrieve the expert's profile data
            Tbl_Expert profileUser = db.Tbl_Expert.FirstOrDefault(u => u.ExpertId == expertId);

            if (profileUser == null)
            {
                ViewBag.ProfileNotFound = true;
                return View();
            }

            // Retrieve the expert's bookings
            var expertBookings = db.Tbl_Bookings
                .Where(b => b.ExpertID == expertId)
                .ToList();

            // Fetch the associated Destination for each booking and determine the tour state
            foreach (var booking in expertBookings)
            {
                booking.Tbl_Destination = db.Tbl_Destination.Find(booking.DestinationId);
                booking.TourState = GetTourState(booking.Tbl_Destination.StartDate, booking.Tbl_Destination.EndDate);
            }

            // Retrieve the expert's uploaded tours
            var expertUploadedTours = db.Tbl_Destination
                .Where(t => t.FK_Expert_Id == expertId)
                .ToList();

            int totalUploadedTours = expertUploadedTours.Count;

            // Calculate the total booking amount for the expert
            decimal totalBookingAmount = expertBookings.Sum(b => b.Tbl_Destination?.Price ?? 0);

            // Pass the additional data to the view
            ViewBag.TotalUploadedTours = totalUploadedTours;
            ViewBag.TotalBookingAmount = totalBookingAmount;

            // Pass the expert's profile data, bookings, and uploaded tours to the view
            ViewBag.ProfileUser = profileUser;
            ViewBag.ExpertBookings = expertBookings;
            ViewBag.ExpertUploadedTours = expertUploadedTours;

            return View(profileUser);
        }


        public ActionResult DeleteTour(int id)
        {
            // Find the tour with the given DestinationId
            var tourToDelete = db.Tbl_Destination.FirstOrDefault(t => t.DestinationID == id);

            if (tourToDelete == null)
            {
                // Tour not found, show an error message or redirect to an error page
                // For simplicity, let's redirect to the ExpertDashboard with an error message
                TempData["ErrorMessage"] = "Tour not found.";
                return RedirectToAction("ExpertDashboard");
            }

            // Check for data dependencies (e.g., bookings) before deleting the tour
            var hasDependencies = db.Tbl_Bookings.Any(b => b.DestinationId == id);

            if (hasDependencies)
            {
                // If there are bookings or any other dependent data, prevent deletion and show an error message
                TempData["ErrorMessage"] = "Cannot delete the tour. It has associated bookings.";
                return RedirectToAction("ExpertDashboard");
            }

            // Now, remove the tour itself
            db.Tbl_Destination.Remove(tourToDelete);

            // Save changes to the database
            db.SaveChanges();

            // Redirect to the ExpertDashboard with a success message
            TempData["SuccessMessage"] = "Tour successfully deleted.";
            return RedirectToAction("ExpertDashboard");
        }



        [HttpGet]
        public ActionResult Edit_Expert(int id)
        {
            Tbl_Expert user = db.Tbl_Expert.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Save_Expert(Tbl_Expert user, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't be Uploaded. Please try again.";
            }
            else
            {
                Tbl_Expert us = db.Tbl_Expert.Find(user.ExpertId);
                if (us != null)
                {
                    us.ExpertName = user.ExpertName;
                    us.ExpertEmail = user.ExpertEmail;
                    us.ExperPassword = user.ExperPassword;
                    us.ExpertBio = user.ExpertBio;
                    if (imgfile != null)
                    {
                        us.ExpertProfileImage = path;
                    }
                    us.ExpertLocation = user.ExpertLocation;
                    db.SaveChanges();
                    return RedirectToAction("ExpertDashboard", new { us.ExpertId });
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(user);
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