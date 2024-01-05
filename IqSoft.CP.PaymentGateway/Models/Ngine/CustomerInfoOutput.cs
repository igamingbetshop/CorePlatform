using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class CustomerInfoOutput
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public int SSN { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public DateTime DOB { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string CurrencyCode { get; set; }
        public string CustomerProfile { get; set; }
        public decimal Balance { get; set; }
        public bool DocOnFile { get; set; }
    }
}