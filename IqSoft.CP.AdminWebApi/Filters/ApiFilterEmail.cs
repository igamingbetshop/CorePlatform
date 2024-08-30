using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
	public class ApiFilterEmail : ApiFilterBase
	{
		public ApiFiltersOperation Ids { get; set; }

		public ApiFiltersOperation Subjects { get; set; }

		public ApiFiltersOperation PartnerIds { get; set; }

		public ApiFiltersOperation Statuses { get; set; }
		public ApiFiltersOperation Receiver { get; set; }

		public DateTime? CreatedFrom { get; set; }

		public DateTime? CreatedBefore { get; set; }
		public int? ObjectId { get; set; }
		public int? ObjectTypeId { get; set; }
	}
}