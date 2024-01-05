namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiSendRecoveryTokenInput : ApiRequestBase
    {
        public string EmailOrMobile { get; set; }

		public string ReCaptcha { get; set; }
	}
}