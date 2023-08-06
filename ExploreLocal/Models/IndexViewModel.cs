using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class IndexViewModel
    {
        public Tbl_User User { get; set; }
        public List<Tbl_Venue> Venues { get; set; }
    }

}