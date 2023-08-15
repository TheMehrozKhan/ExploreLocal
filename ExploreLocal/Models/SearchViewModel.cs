using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class SearchViewModel
    {
        public List<Tbl_Destination> SelectedVenueTours { get; set; }
        public string SelectedVenueName { get; set; } 
        public List<Tbl_Venue> Venues { get; set; }
    }

}