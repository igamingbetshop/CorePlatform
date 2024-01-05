using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetPaymentRequestsOutput : ClientRequestResponseBase
	{
		public List<PaymentRequest> PaymentRequests { get; set; }
	}
}