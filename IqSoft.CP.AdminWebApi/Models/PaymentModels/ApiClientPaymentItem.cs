namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
	public class ApiClientPaymentItem
	{
		public int Id { get; set; }

		public int ClientId { get; set; }

		public int PartnerPaymentSettingId { get; set; }

		public int Type { get; set; }
		public int State { get; set; }

		public string PaymentSystem { get; set; }

		public string CurrencyId { get; set; }
	}
}