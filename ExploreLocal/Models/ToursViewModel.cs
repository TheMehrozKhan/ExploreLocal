using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class ToursViewModel
    {
        public List<Tbl_Destination> SelectedVenueTours { get; set; }
        public List<Tbl_Destination> MostPopularTours { get; set; }
        public List<Tbl_Destination> TrendingTours { get; set; }
        public string SelectedVenueName { get; set; }
        public string SelectedVenueDescription { get; set; }
        public List<Tbl_Announcement> Announcement { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string Duration { get; set; }
        public bool NoToursFound { get; set; }

    }

}