using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class ClientRegistrationInput
    {
        public Client ClientData { get; set; }
        public string PromoCode { get; set; }
        public int? ReferralType { get; set; }
        public int? BetShopId { get; set; }
        public AffiliateReferral ReferralData { get; set; }
        public int RegistrationType { get; set; }
        public bool GeneratedUsername { get; set; }
        public bool IsFromAdmin { get; set; }
        public string ReCaptcha { get; set; }
        public byte[] PassportDocumentByte { get; set; }
        public byte[] DriverLicenseDocumentByte { get; set; }
        public byte[] IdCardDocumentByte { get; set; }
        public byte[] UtilityBillDocumentByte { get; set; }
        public int CommunicationType { get; set; }
        public string CommunicationTypeValue { get; set; }
        public List<Common.Models.WebSiteModels.SecurityQuestion> SecurityQuestions { get; set; }
		public int?[] BetShopPaymentSystems { get; set; }
	}
}