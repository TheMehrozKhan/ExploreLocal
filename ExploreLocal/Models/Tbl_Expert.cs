//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExploreLocal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tbl_Expert
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_Expert()
        {
            this.Tbl_Bookings = new HashSet<Tbl_Bookings>();
            this.Tbl_Destination = new HashSet<Tbl_Destination>();
        }
    
        public int ExpertId { get; set; }
        public string ExpertName { get; set; }
        public string ExpertEmail { get; set; }
        public string ExpertBio { get; set; }
        public Nullable<bool> ExpertStatus { get; set; }
        public string ExpertProfileImage { get; set; }
        public string ExpertLocation { get; set; }
        public string ExperPassword { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_Bookings> Tbl_Bookings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_Destination> Tbl_Destination { get; set; }
    }
}
