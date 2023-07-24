using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class ExpertViewModel
    {
        public string ExpertName { get; set; }
        public string ExpertEmail { get; set; }
        public string ExpertBio { get; set; }
        public string ExperPassword { get; set; }
        public string ExpertLocation { get; set; }
        public bool ExpertStatus { get; set; }

    }

}