namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ChangeClientPasswordInput : ApiClientSecurityModel
    {
        public int ClientId { get;set; }
        public string NewPassword { get;set; }
        public string OldPassword { get;set; }
        public string NewPin { get; set; }
        public string OldPin { get; set; }
    }
}