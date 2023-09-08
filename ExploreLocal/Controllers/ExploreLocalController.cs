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

            using (var db = new ExploreLocalEntities())
            {
                List<Tbl_Venue> venues = db.Tbl_Venue.ToList();
                List<Tbl_Destination> destinations = db.Tbl_Destination.Where(t => t.TourStatus == true).ToList(); 

                var viewModel = new IndexViewModel
                {
                    User = user,
                    Venues = venues,
                    Destinations = destinations
                };
                return View(viewModel);
            }
        }

        public ActionResult SearchDestinations(int? selectedVenueId, string searchQuery)
        {
            if (selectedVenueId == null)
            {
                return RedirectToAction("Index");
            }

            var selectedVenueTours = db.Tbl_Destination
                .Where(t => t.FK_Venue_Id == selectedVenueId && t.TourStatus == true &&
                    (t.DestinationName.ToLower().Contains(searchQuery.ToLower()) ||
                     t.Description.ToLower().Contains(searchQuery.ToLower()) || t.Destination_Highlights.ToLower().Contains(searchQuery.ToLower()) ))
                .ToList();

            var selectedVenueName = db.Tbl_Venue
                .Where(v => v.Venue_id == selectedVenueId)
                .Select(v => v.Venue_name)
                .FirstOrDefault();

            var venues = db.Tbl_Venue.ToList();

            var viewModel = new SearchViewModel
            {
                SelectedVenueTours = selectedVenueTours,
                SelectedVenueName = selectedVenueName,
                Venues = venues
            };

            return View("SearchResults", viewModel);
        }


        //public ActionResult Destinations(int id)
        //{
        //    var selectedVenueTours = db.Tbl_Destination.Where(t => t.FK_Venue_Id == id && t.TourStatus == true).ToList();
        //    var selectedVenue = db.Tbl_Venue.FirstOrDefault(v => v.Venue_id == id);
        //var mostPopularTours = db.Tbl_Destination
        //    .GroupJoin(
        //        db.Tbl_Bookings,
        //        destination => destination.DestinationID,
        //        booking => booking.DestinationId,
        //        (destination, bookings) => new
        //        {
        //            Destination = destination,
        //            BookingsCount = bookings.Count()
        //        })
        //    .Where(x => x.Destination.TourStatus == true)
        //    .OrderByDescending(x => x.BookingsCount)
        //    .Select(x => x.Destination)
        //    .Take(4)
        //    .ToList();
        //var trendingTours = db.Tbl_Destination
        //    .GroupJoin(
        //        db.Tbl_Bookings,
        //        destination => destination.DestinationID,
        //        booking => booking.DestinationId,
        //        (destination, bookings) => new
        //        {
        //            Destination = destination,
        //            LatestBookingDate = bookings.Max(b => b.BookingDate)
        //        })
        //    .Where(x => x.Destination.TourStatus == true)
        //    .OrderByDescending(x => x.LatestBookingDate)
        //    .Select(x => x.Destination)
        //    .Take(4)
        //    .ToList();
        //var announcements = db.Tbl_Announcement.Where(t => t.Announcement_status == null).ToList();
        //var viewModel = new ToursViewModel
        //{
        //    SelectedVenueTours = selectedVenueTours,
        //    MostPopularTours = mostPopularTours,
        //    TrendingTours = trendingTours,
        //    SelectedVenueName = selectedVenue?.Venue_name,
        //    SelectedVenueDescription = selectedVenue?.Venue_Description,
        //    Announcement = announcements
        //};
        //    return View(viewModel);
        //}

        public ActionResult Destinations(int id, decimal? minPrice, decimal? maxPrice, string country, string language, string duration)
        {
            var selectedVenue = db.Tbl_Venue.FirstOrDefault(v => v.Venue_id == id);
            var selectedVenueTours = db.Tbl_Destination.Where(t => t.FK_Venue_Id == id && t.TourStatus == true).ToList();

            // Apply filters based on the provided parameters
            if (minPrice != null)
            {
                selectedVenueTours = selectedVenueTours.Where(tour => tour.Price >= minPrice).ToList();
            }

            if (maxPrice != null)
            {
                selectedVenueTours = selectedVenueTours.Where(tour => tour.Price <= maxPrice).ToList();
            }

            if (!string.IsNullOrEmpty(country))
            {
                selectedVenueTours = selectedVenueTours.Where(tour => tour.Country != null && tour.Country.IndexOf(country, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrEmpty(language))
            {
                selectedVenueTours = selectedVenueTours.Where(tour => tour.Language != null && tour.Language.IndexOf(language, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrEmpty(duration))
            {
                selectedVenueTours = selectedVenueTours.Where(tour => tour.Destination_Duration == duration).ToList();
            }

            var mostPopularTours = db.Tbl_Destination
                .GroupJoin(
                    db.Tbl_Bookings,
                    destination => destination.DestinationID,
                    booking => booking.DestinationId,
                    (destination, bookings) => new
                    {
                        Destination = destination,
                        BookingsCount = bookings.Count()
                    })
                .Where(x => x.Destination.TourStatus == true)
                .OrderByDescending(x => x.BookingsCount)
                .Select(x => x.Destination)
                .Take(4)
                .ToList();

            var trendingTours = db.Tbl_Destination
                .GroupJoin(
                    db.Tbl_Bookings,
                    destination => destination.DestinationID,
                    booking => booking.DestinationId,
                    (destination, bookings) => new
                    {
                        Destination = destination,
                        LatestBookingDate = bookings.Max(b => b.BookingDate)
                    })
                .Where(x => x.Destination.TourStatus == true)
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
                Announcement = announcements,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Country = country,
                Language = language,
                Duration = duration,
                NoToursFound = !selectedVenueTours.Any() // Set the flag
            };

            return View(viewModel);
        }




        public ActionResult DestinationDetails(int? id)
        {
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
                Destination_Highlights = p.Destination_Highlights,
                Comments = db.Tbl_Comments.Where(c => c.DestinationId == p.DestinationID).ToList()
            };
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
            return View(destinationDetailsViewModel);
        }

        [HttpGet]
        public ActionResult BookingForm(int destinationId, int expertId, int userId)
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }
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

                        db.Tbl_Bookings.Add(booking);
                        db.SaveChanges();

                        return RedirectToAction("BookingSuccess", new { id = booking.BookingId });
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "An error occurred while submitting the booking: " + ex.Message;
                }
            }
            return View("BookingForm", viewModel);
        }

        public ActionResult BookingSuccess(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error");
            }
            using (var db = new ExploreLocalEntities())
            {
                var booking = db.Tbl_Bookings.Find(id);
                if (booking != null)
                {
                    var user = db.Tbl_User.Find(booking.UserId);

                    var destination = db.Tbl_Destination.Find(booking.DestinationId);

                    var expert = db.Tbl_Expert.Find(booking.ExpertID);

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
                return RedirectToAction("Error");
            }
            int bookingIdValue = bookingId.Value;
            using (var db = new ExploreLocalEntities())
            {
                var booking = db.Tbl_Bookings.Find(bookingIdValue);
                if (booking == null)
                {
                    return RedirectToAction("Error");
                }

                var user = db.Tbl_User.Find(booking.UserId);
                var destination = db.Tbl_Destination.Find(booking.DestinationId);

                var expert = db.Tbl_Expert.Find(booking.ExpertID);

                if (user != null && destination != null && expert != null)
                {
                    ViewBag.Booking = booking;
                    ViewBag.User = user;
                    ViewBag.Destination = destination;
                    ViewBag.Expert = expert;

                    string htmlContent = RenderViewToString("InvoiceTemplate");

                    byte[] invoicePdf = GenerateInvoicePdf(htmlContent);
                    return File(invoicePdf, "application/pdf", "Invoice.pdf");
                }

            }
            return RedirectToAction("Error"); 
        }

        private byte[] GenerateInvoicePdf(string htmlContent)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ConverterProperties converterProperties = new ConverterProperties();
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdfDoc = new PdfDocument(writer);
                HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, converterProperties);

                pdfDoc.Close();
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
                return RedirectToAction("Error");
            }

            using (var db = new ExploreLocalEntities())
            {
                var booking = db.Tbl_Bookings.Find(bookingId);
                if (booking != null)
                {
                    var user = db.Tbl_User.Find(booking.UserId);
                    var destination = db.Tbl_Destination.Find(booking.DestinationId);

                    var expert = db.Tbl_Expert.Find(booking.ExpertID);

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
                Session["u_image"] = us.ProfileImage;
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
                TourStatus = false 
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

            List<Tbl_Bookings> userBookings = db.Tbl_Bookings
                .Where(b => b.UserId == id)
                .ToList();

            foreach (var booking in userBookings)
            {
                booking.Tbl_Destination = db.Tbl_Destination.Find(booking.DestinationId);

                booking.TourState = GetTourState(booking.Tbl_Destination.StartDate, booking.Tbl_Destination.EndDate);
            }

            ViewBag.ProfileNotFound = false; 
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
                expertId = Convert.ToInt32(Session["expert_id"]);
            }
            else
            {
                expertId = (int)id;
            }

            Tbl_Expert profileUser = db.Tbl_Expert.FirstOrDefault(u => u.ExpertId == expertId);

            if (profileUser == null)
            {
                ViewBag.ProfileNotFound = true;
                return View();
            }

            var expertBookings = db.Tbl_Bookings
                .Where(b => b.ExpertID == expertId)
                .ToList();

            foreach (var booking in expertBookings)
            {
                booking.Tbl_Destination = db.Tbl_Destination.Find(booking.DestinationId);
                booking.TourState = GetTourState(booking.Tbl_Destination.StartDate, booking.Tbl_Destination.EndDate);
            }

            var expertUploadedTours = db.Tbl_Destination
                .Where(t => t.FK_Expert_Id == expertId)
                .ToList();

            int totalUploadedTours = expertUploadedTours.Count;

            decimal totalBookingAmount = expertBookings.Sum(b => b.Tbl_Destination?.Price ?? 0);

            ViewBag.TotalUploadedTours = totalUploadedTours;
            ViewBag.TotalBookingAmount = totalBookingAmount;

            ViewBag.ProfileUser = profileUser;
            ViewBag.ExpertBookings = expertBookings;
            ViewBag.ExpertUploadedTours = expertUploadedTours;

            return View(profileUser);
        }


        public ActionResult DeleteTour(int id)
        {
            var tourToDelete = db.Tbl_Destination.FirstOrDefault(t => t.DestinationID == id);

            if (tourToDelete == null)
            {
                TempData["ErrorMessage"] = "Tour not found.";
                return RedirectToAction("ExpertDashboard");
            }

            var hasDependencies = db.Tbl_Bookings.Any(b => b.DestinationId == id);

            if (hasDependencies)
            {
                TempData["ErrorMessage"] = "Cannot delete the tour. It has associated bookings.";
                return RedirectToAction("ExpertDashboard");
            }

            db.Tbl_Destination.Remove(tourToDelete);
            db.SaveChanges();
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
        public ActionResult Add_Wishlist(int? id)
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            Tbl_Destination p = db.Tbl_Destination.Where(x => x.DestinationID == id).SingleOrDefault();
            if (p == null)
            {
                return RedirectToAction("Index");
            }

            List<Wishlist> userItems = Session["wishlist"] as List<Wishlist>;

            if (userItems == null)
            {
                userItems = new List<Wishlist>();
            }

            Wishlist existingItem = userItems.FirstOrDefault(item => item.DestinationID == id);

            if (existingItem != null)
            {
                existingItem.o_qty++;
                existingItem.o_bill = existingItem.Price * existingItem.o_qty;
            }
            else
            {
                Wishlist ca = new Wishlist();
                ca.DestinationID = p.DestinationID;
                ca.DestinationName = p.DestinationName;
                ca.Price = Convert.ToInt32(p.Price);
                ca.Image = p.Image;
                ca.Destination_Duration = p.Destination_Duration;
                ca.Country = p.Country;
                ca.Language = p.Language;


                userItems.Add(ca);
            }

            int cartCount = userItems.Count;
            ViewBag.CartCount = cartCount;

            Session["wishlist"] = userItems;

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult View_Wishlist()
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            List<Wishlist> userItems = Session["Wishlist"] as List<Wishlist>;

            if (userItems == null)
            {
                userItems = new List<Wishlist>();
            }

            ViewBag.WishlistCount = userItems.Count;

            return View(userItems);
        }

        public ActionResult RemoveFromWishlist(int? id)
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            List<Wishlist> userItems = Session["wishlist"] as List<Wishlist>;

            if (userItems != null)
            {
                Wishlist itemToRemove = userItems.FirstOrDefault(item => item.DestinationID == id);
                if (itemToRemove != null)
                {
                    userItems.Remove(itemToRemove);
                }
            }

            Session["wishlist"] = userItems;

            return RedirectToAction("View_Wishlist");
        }

        [HttpPost]
        public ActionResult AddComment(int recipeId, string commentText)
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["u_id"].ToString());

            Tbl_Comments comment = new Tbl_Comments
            {
                DestinationId = recipeId,
                UserId = userId,
                CommentText = commentText,
                CommentDate = DateTime.Now
            };

            db.Tbl_Comments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("DestinationDetails", new { id = recipeId });
        }

        [HttpPost]
        public ActionResult AddReply(int commentId, string replyText)
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["u_id"].ToString());

            Tbl_Replies reply = new Tbl_Replies
            {
                CommentId = commentId,
                UserId = userId,
                ReplyText = replyText,
                ReplyDate = DateTime.Now
            };

            Tbl_Comments comment = db.Tbl_Comments.Find(commentId);
            if (comment != null)
            {
                reply.Tbl_Comments = comment;
                db.Tbl_Replies.Add(reply);
                db.SaveChanges();
                return RedirectToAction("DestinationDetails", new { id = comment.DestinationId });
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Delete(int? id)
        {
            Tbl_Comments ca = db.Tbl_Comments.Where(x => x.CommentId == id).SingleOrDefault();
            return View(ca);
        }

        [HttpPost]
        public ActionResult Delete(int? id, Tbl_Venue cat)
        {
            Tbl_Comments ca = db.Tbl_Comments.Include("Tbl_Replies").SingleOrDefault(x => x.CommentId == id);

            foreach (var reply in ca.Tbl_Replies.ToList())
            {
                db.Tbl_Replies.Remove(reply);
            }

            db.Tbl_Comments.Remove(ca);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteReply(int? id)
        {
            Tbl_Replies ca = db.Tbl_Replies.Where(x => x.ReplyId == id).SingleOrDefault();
            return View(ca);
        }

        [HttpPost]
        public ActionResult DeleteReply(int? id, Tbl_Venue cat)
        {
            Tbl_Replies reply = db.Tbl_Replies.Include("Tbl_Comments").SingleOrDefault(x => x.ReplyId == id);

            db.Tbl_Replies.Remove(reply);
            db.SaveChanges();

            return RedirectToAction("Index");
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