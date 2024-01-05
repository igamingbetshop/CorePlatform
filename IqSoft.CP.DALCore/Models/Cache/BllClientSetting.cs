using System;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BllClientSetting
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string Name { get; set; }
		public decimal? NumericValue { get; set; }
		public DateTime? DateValue { get; set; }
		public string StringValue { get; set; }
		public int? UserId { get; set; }
	}
}