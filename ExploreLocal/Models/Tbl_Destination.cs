//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Web.Mvc;
namespace ExploreLocal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tbl_Destination
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_Destination()
        {
            this.Tbl_Bookings = new HashSet<Tbl_Bookings>();
            this.Tbl_Comments = new HashSet<Tbl_Comments>();
        }
    
        public int DestinationID { get; set; }
        public string DestinationName { get; set; }
        public string Country { get; set; }
        public string Image { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> FK_Venue_Id { get; set; }
        [AllowHtml]
        public string GoogleStreetViewURL { get; set; }
        public string MeetingPoint { get; set; }
        public string Language { get; set; }
        public Nullable<int> FK_Expert_Id { get; set; }
        public string Destination_Duration { get; set; }
        [AllowHtml]
        public string Destination_Highlights { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public Nullable<bool> TourStatus { get; set; }
        public Tbl_Expert Expert { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_Bookings> Tbl_Bookings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_Comments> Tbl_Comments { get; set; }
        public virtual Tbl_Expert Tbl_Expert { get; set; }
        public virtual Tbl_Venue Tbl_Venue { get; set; }
    }
}
