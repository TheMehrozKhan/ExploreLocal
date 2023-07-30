using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class DestinationBookingViewModel
    {
        public DestinationDetailsViewModel DestinationDetails { get; set; }
        public int ExpertId { get; set; }
        public string ExpertName { get; set; }
        public int DestinationID { get; set; }
    }
}