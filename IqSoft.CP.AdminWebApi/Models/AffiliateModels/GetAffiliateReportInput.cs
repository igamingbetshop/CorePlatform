using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.Integration.Payments.Models.PayBox;
using System;

namespace IqSoft.CP.AdminWebApi.Models.AffiliateModels
{
	public class GetAffiliateReportInput : RequestInfo
	{
		public int? UserId { get; set; }
		public string ApiKey { get; set; }
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }
	}
}