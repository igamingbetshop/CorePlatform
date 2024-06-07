using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.QuikiPay
{
	public class PayoutWalletInput
	{
		public string name { get; set; }
		public string crypto_currency { get; set; }
		public string crypto_address { get; set; }
		public string crypto_tag { get; set; }
		public string signature { get; set; }
	}
}
