using System;

namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
	public class EmailModel
	{
		public long Id { get; set; }

		public int PartnerId { get; set; }

		public string Message { get; set; }

		public int Type { get; set; }

		public int? Status { get; set; }

		public DateTime? CreationTime { get; set; }

		public string Subject { get; set; }
		public string Receiver { get; set; }
		public long? ObjectId { get; set; }
		public int? ObjectTypeId { get; set; }
	}
}