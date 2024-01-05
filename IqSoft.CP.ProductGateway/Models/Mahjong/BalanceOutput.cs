using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Mahjong
{
    [Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "response_data", Namespace = "", IsNullable = false)]
	public class BalanceResponse
	{
		[XmlElement(ElementName = "amount")]
		public string Amount { get; set; }

		[XmlElement(ElementName = "currency")]
		public string Currency { get; set; }
	}

	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "balance_response", Namespace = "", IsNullable = false)]
	public class BalanceOutput
	{
		[XmlElement(ElementName = "api_version")]
		public string ApiVersion { get; set; }

		[XmlElement(ElementName = "user_id")]
		public string UserId { get; set; }

		[XmlElement(ElementName = "response_data")]
		public BalanceResponse BalanceResponse { get; set; }

		[XmlElement(ElementName = "echo")]
		public string Echo { get; set; }

		[XmlElement(ElementName = "error_code")]
		public string ErrorCode { get; set; } = "200";
	}

}