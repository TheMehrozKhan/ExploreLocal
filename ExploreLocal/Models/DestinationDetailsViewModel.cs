using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace ExploreLocal.Models
{
    public class DestinationDetailsViewModel
    {
        public int DestinationID { get; set; }
        public string DestinationName { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        [AllowHtml]
        public string GoogleStreetViewURL { get; set; }
        public string MeetingPoint { get; set; }
        public string Language { get; set; }
        public string Venue_name { get; set; }
        public string ExpertName { get; set; }
        public string ExpertProfileImage { get; set; }
        public int ExpertId { get; set; }
        public string Destination_Duration { get; set; }
        public string Destination_Highlights { get; set; }
    }
}