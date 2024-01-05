using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.SoftLand
{
	public class OperationInput
	{
		[JsonProperty(PropertyName = "RoundOperations")]
		public List<RoundOperation> RoundOperations { get; set; }

		[JsonProperty(PropertyName = "OperationType")]
		public string OperationType { get; set; }

		[JsonProperty(PropertyName = "TransactionId")]
		public long TransactionId { get; set; }

		[JsonProperty(PropertyName = "Amount")]
		public decimal Amount { get; set; }

		[JsonProperty(PropertyName = "CreationDate")]
		public DateTime CreationDate { get; set; }

		[JsonProperty(PropertyName = "RoundId")]
		public string RoundId { get; set; }

		[JsonProperty(PropertyName = "Currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "GameId")]
		public int GameId { get; set; }

		[JsonProperty(PropertyName = "SiteId")]
		public string SiteId { get; set; }

		[JsonProperty(PropertyName = "PlayerId")]
		public string PlayerId { get; set; }

		[JsonProperty(PropertyName = "IsRoundFinished")]
		public bool? IsRoundFinished { get; set; }

		[JsonProperty(PropertyName = "OriginalTransactionId")]
		public string OriginalTransactionId { get; set; }

		[JsonProperty(PropertyName = "GameType")]
		public string GameType { get; set; }
	}
	public class RoundOperation
	{
		[JsonProperty(PropertyName = "OperationTypeId")]
		public int OperationTypeId { get; set; }

		[JsonProperty(PropertyName = "TransactionId")]
		public int TransactionId { get; set; }

		[JsonProperty(PropertyName = "Amount")]
		public double Amount { get; set; }

		[JsonProperty(PropertyName = "CreationDate")]
		public DateTime CreationDate { get; set; }

		[JsonProperty(PropertyName = "PlatformTransactionId")]
		public string PlatformTransactionId { get; set; }
	}
}