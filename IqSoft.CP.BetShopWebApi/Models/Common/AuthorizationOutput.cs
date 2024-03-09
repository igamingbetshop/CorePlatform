using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class AuthorizationOutput : ApiLoginResponse
    {
		public long Id { get; set; }
		public int CashierId { get; set; }
		public int UserId { get; set; }
		public string CashierFirstName { get; set; }
		public string CashierLastName { get; set; }
		public int CashDeskId { get; set; }
		public string CashDeskName { get; set; }
		public int PartnerId { get; set; }
		public string BetShopCurrencyId { get; set; }
		public int BetShopId { get; set; }
		public string BetShopAddress { get; set; }
		public string BetShopName { get; set; }
		private decimal _balance;
		public decimal Balance
		{
			get { return Math.Floor(_balance * 100) / 100; }
			set { _balance = value; }
		}
		public decimal CurrentLimit { get; set; }
		public long? ParentId { get; set; }
		public int State { get; set; }
		public bool PrintLogo { get; set; }
		public bool? AnonymousBet { get; set; }
		public bool? AllowCashout { get; set; }
		public bool? AllowLive { get; set; }
		public bool? UsePin { get; set; }
		public int? Restrictions { get; set; }
		public List<ApiPaymentSystem> PaymentSystems { get; set; }
	}
}