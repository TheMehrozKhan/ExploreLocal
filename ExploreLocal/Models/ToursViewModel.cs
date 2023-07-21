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
    }

}