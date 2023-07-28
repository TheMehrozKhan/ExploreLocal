﻿using ExploreLocal.Models;
using PagedList;
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
        ExploreLocalEntities1 db = new ExploreLocalEntities1();
        public ActionResult Index(Tbl_User us)
        {
            TempData["ToastMessage"] = "Hi, " + us.FirstName + " " +  us.LastName + " You Successfully Logged In!";
            ViewBag.ToastMessage = TempData["ToastMessage"];
            return View();
        }

        public ActionResult Destinations(int id)
        {
            // Get the selected venue tours
            var selectedVenueTours = db.Tbl_Destination.Where(t => t.FK_Venue_Id == id).ToList();
            var selectedVenue = db.Tbl_Venue.FirstOrDefault(v => v.Venue_id == id);

            // Fetch most popular tours
            var mostPopularTours = db.Tbl_Destination
                .GroupJoin(
                    db.Tbl_BookingHistory,
                    destination => destination.DestinationID,
                    booking => booking.DestinationID,
                    (destination, bookings) => new
                    {
                        Destination = destination,
                        BookingsCount = bookings.Count()
                    })
                .OrderByDescending(x => x.BookingsCount)
                .Select(x => x.Destination)
                .Take(4) // You can change the number as per your requirement
                .ToList();

            // Fetch trending tours
            var trendingTours = db.Tbl_Destination
                .GroupJoin(
                    db.Tbl_BookingHistory,
                    destination => destination.DestinationID,
                    booking => booking.DestinationID,
                    (destination, bookings) => new
                    {
                        Destination = destination,
                        LatestBookingDate = bookings.Max(b => b.Date)
                    })
                .OrderByDescending(x => x.LatestBookingDate)
                .Select(x => x.Destination)
                .Take(4) // You can change the number as per your requirement
                .ToList();

            // Create a ViewModel to store both sets of tours
            var viewModel = new ToursViewModel
            {
                SelectedVenueTours = selectedVenueTours,
                MostPopularTours = mostPopularTours,
                TrendingTours = trendingTours,
                SelectedVenueName = selectedVenue?.Venue_name,
                SelectedVenueDescription = selectedVenue?.Venue_Description
            };

            return View(viewModel);
        }

        public ActionResult DestinationDetails(int id)
        {
            Tbl_Destination p = db.Tbl_Destination.Where(x => x.DestinationID == id).SingleOrDefault();

            var pr = new DestinationDetailsViewModel
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
                Language = p.Language
            };

            // Assuming that the Tbl_Destination has a foreign key to Tbl_Expert table, you can get the expert's name and profile image like this:
            Tbl_Expert expert = db.Tbl_Expert.Where(e => e.ExpertId == p.FK_Expert_Id).SingleOrDefault();
            if (expert != null)
            {
                pr.ExpertName = expert.ExpertName;
                pr.ExpertProfileImage = expert.ExpertProfileImage;
                pr.ExpertId = expert.ExpertId;
            }
            else
            {
                // Set default values or handle the case when the expert is not found.
                pr.ExpertName = "No Expert Found";
                pr.ExpertProfileImage = "./Content/img/Default.png"; // Replace this with the path to your default expert image
            }

            Tbl_Venue venue = db.Tbl_Venue.Where(x => x.Venue_id == p.FK_Venue_Id).SingleOrDefault();
            if (venue != null)
            {
                pr.Venue_name = venue.Venue_name;
            }
            else
            {
                pr.ExpertName = "No Expert Found"; 
            }

            return View(pr);
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

                using (var db = new ExploreLocalEntities1()) 
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

            Tbl_Destination pro = new Tbl_Destination();
            pro.DestinationName = pr.DestinationName;
            pro.Country = pr.Country;
            pro.Description = pr.Description;
            pro.Price = pr.Price;
            pro.MeetingPoint = pr.MeetingPoint;
            pro.Language = pr.Language;
            pro.FK_Venue_Id = pr.FK_Venue_Id;
            pro.GoogleStreetViewURL = pr.GoogleStreetViewURL;
            pro.StartDate = pr.StartDate;
            pro.EndDate = pr.EndDate;
            pro.FK_Expert_Id = Convert.ToInt32(Session["expert_id"].ToString());

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

            ViewBag.ProfileNotFound = true;
            return View(profileUser);
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
            int userId = Convert.ToInt32(Session["expert_id"]);

            if (id == null)
            {
                id = userId;
            }

            Tbl_Expert profileUser = db.Tbl_Expert.FirstOrDefault(u => u.ExpertId == id);

            if (profileUser == null)
            {
                ViewBag.ProfileNotFound = true;
                return View();
            }

            ViewBag.ProfileNotFound = true;
            return View(profileUser);
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