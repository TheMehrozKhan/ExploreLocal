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
    
    public partial class Tbl_Bookings
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int DestinationId { get; set; }
        public int ExpertID { get; set; }
        public Nullable<int> NumberOfAdults { get; set; }
        public Nullable<int> NumberOfChildren { get; set; }
        public Nullable<System.DateTime> BookingDate { get; set; }
        public Nullable<int> CreditCardNumber { get; set; }
        public Nullable<int> PIN { get; set; }
        public Nullable<int> CVV { get; set; }
        public string CardHolderName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Nullable<int> ContactNumber { get; set; }
        public string TourState { get; set; }
    
        public virtual Tbl_Destination Tbl_Destination { get; set; }
        public virtual Tbl_Expert Tbl_Expert { get; set; }
        public virtual Tbl_User Tbl_User { get; set; }
    }
}
