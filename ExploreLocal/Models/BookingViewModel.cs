using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExploreLocal.Models
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public int DestinationId { get; set; }
        public int ExpertID { get; set; }
        public int UserId { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public DateTime BookingDate { get; set; }
        public int CreditCardNumber { get; set; }
        public int PIN { get; set; }
        public int CVV { get; set; }
        public string CardHolderName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int ContactNumber { get; set; }
    }
}