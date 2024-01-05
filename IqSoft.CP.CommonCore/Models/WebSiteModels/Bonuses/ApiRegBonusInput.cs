namespace IqSoft.CP.Common.Models.WebSiteModels.Bonuses
{
    public class ApiRegBonusInput : ApiRequestBase
    {
        public int Index { get; set; }

		public string ActivationKey { get; set; }
	}
}