using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Mahjong
{
	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "response_data", Namespace = "", IsNullable = false)]
	public class TransferResponse
	{
		[XmlElement(ElementName = "user_id")]
		public string UserId { get; set; }

		[XmlElement(ElementName = "partner_transaction_id")]
		public string PartnerTransactionId { get; set; }

		[XmlElement(ElementName = "amount")]
		public string Amount { get; set; }
		[XmlElement(ElementName = "currency")]
		public string Currency { get; set; }

		[XmlElement(ElementName = "direction")]
		public string Direction { get; set; }

		[XmlElement(ElementName = "mahjong_transaction_id")]
		public string MahjongTransactionId { get; set; }
		
		[XmlElement(ElementName = "error_reason")]
		public string ErrorReason { get; set; }
	}

	[XmlRoot(ElementName = "transfer_response")]
	public class TransferOutput
	{
		[XmlElement(ElementName = "api_version")]
		public string ApiVersion { get; set; }

		[XmlElement(ElementName = "response_data")]
		public TransferResponse TransferResponse { get; set; }

		[XmlElement(ElementName = "error_code")]
		public string ErrorCode { get; set; } = "200";

		[XmlElement(ElementName = "echo")]
		public string Echo { get; set; }
	}
}