using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PayPaymentRequestOutput : FinOperationResponse
	{
		public long TransactionId { get; set; }

		public long Barcode
		{
			get
			{
				return 100000000000 + TransactionId;
			}
		}

		public int ClientId { get; set; }

		public string ClientUserName { get; set; }

		public string DocumentNumber { get; set; }

		public decimal Amount { get; set; }

		public DateTime PayDate { get; set; }
	}
}