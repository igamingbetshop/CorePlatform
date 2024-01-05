namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class SendVerificationCodeInput
    {
        public int ClientId { get; set; }

        public string MobileNumber { get; set; }

        public string Email { get; set; }
    }
}