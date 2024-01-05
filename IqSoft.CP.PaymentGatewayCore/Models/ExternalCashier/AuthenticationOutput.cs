namespace IqSoft.CP.PaymentGateway.Models.ExternalCashier
{
    public class AuthenticationOutput : BaseOutput
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string BirthDate { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string Language { get; set; }
        public string State { get; set; }
    }
}