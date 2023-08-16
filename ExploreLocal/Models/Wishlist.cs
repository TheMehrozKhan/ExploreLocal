using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class Wishlist
    {
        public int DestinationID { get; set; }
        public string DestinationName { get; set; }
        public Nullable<int> Price { get; set; }
        public Nullable<int> o_qty { get; set; }
        public Nullable<int> o_bill { get; set; }
        public string Image { get; set; }
        public string Destination_Duration { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string StartDate { get; set; }

    }
}