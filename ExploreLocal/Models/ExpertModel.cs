using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class ExpertModel
    {
        public int Id { get; set; }
        public string ExpertName { get; set; }
        public string ExpertEmail { get; set; }
        public string ExpertLocation { get; set; }
        public string ExpertPassword { get; set; }
        public string ExpertBio { get; set; }
        public string ExpertProfileImage { get; set; }
    }
}