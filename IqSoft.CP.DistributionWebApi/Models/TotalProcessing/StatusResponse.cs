using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.DistributionWebApi.Models.TotalProcessing
{
	public class StatusResponse
	{
		public string merchantTransactionId { get; set; }

		public RequestResult result { get; set; }
	}
}