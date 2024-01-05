namespace IqSoft.CP.ProductGateway.Models.EveryMatrix
{
    public class AccountOutput : BaseOutput
    {
        public AccountOutput(BaseOutput b)
        {
            ApiVersion = b.ApiVersion;
            ReturnCode = b.ReturnCode;
            Request = b.Request;
            SessionId = b.SessionId;
            Message = b.Message;
        }
        public object Details { get; set; }
        public string ExternalUserId { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
        public string Birthdate { get; set; }
        public int RCPeriod { get; set; }
    }
}