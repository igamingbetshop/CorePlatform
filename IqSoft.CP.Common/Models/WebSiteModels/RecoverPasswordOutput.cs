namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class RecoverPasswordOutput : ApiResponseBase
    {
        public int ClientId { get; set; }

        public string ClientEmail { get; set; }

        public string ClientFirstName { get; set; }

        public string ClientLastName { get; set; }
    }
}