﻿using System;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetByDocumentIdOutput
	{
		public long Id { get; set; }

		public long DocumentId { get; set; }

		public string ExternalId { get; set; }

		public int GameId { get; set; }

		public long BarCode { get; set; }

		public int NumberOfPrints { get; set; }

		public DateTime? LastPrintTime { get; set; }

		public DateTime CreationTime { get; set; }
	}
}