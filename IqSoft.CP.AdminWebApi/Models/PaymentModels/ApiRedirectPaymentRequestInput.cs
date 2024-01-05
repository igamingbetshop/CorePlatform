using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
	public class ApiRedirectPaymentRequestInput
	{
		public int PaymentRequestId { get; set; }

		public int PaymentSystemId { get; set; }
	}
}