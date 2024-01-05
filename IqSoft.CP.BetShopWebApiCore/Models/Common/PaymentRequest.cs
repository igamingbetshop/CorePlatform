using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PaymentRequest
	{
		public long Id { get; set; }

		public long Barcode { get; set; }

		public int ClientId { get; set; }

		public decimal Amount { get; set; }

		public string ClientFirstName { get; set; }

		public string ClientLastName { get; set; }

		public string UserName { get; set; }

		public string DocumentNumber { get; set; }

		public string ClientEmail { get; set; }

		public int Type { get; set; }

		public string CurrencyId { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime CreationTime { get; set; }

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime LastUpdateTime { get; set; }
	}
}