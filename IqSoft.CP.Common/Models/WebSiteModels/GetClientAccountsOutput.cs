using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetClientAccountsOutput
    {
        public List<AccountModel> Accounts { get; set; }
    }

	public class AccountModel
	{
		public long Id { get; set; }
		public int TypeId { get; set; }
		public decimal Balance { get; set; }
		public decimal WithdrawableBalance { get; set; }
		public string CurrencyId { get; set; }
        public string AccountTypeName { get; set; }
		public int? BetShopId { get; set; }
		public int? PaymentSystemId { get; set; }
		public string PaymentSystemName { get; set; }
		public System.DateTime CreationTime { get; set; }
    }
}