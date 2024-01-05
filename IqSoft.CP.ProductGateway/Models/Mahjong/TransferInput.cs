using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Mahjong
{
	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "transfer", Namespace = "", IsNullable = false)]
	public class TransferInput
	{
		[XmlElement(ElementName = "secret_key")]
		public string SecretKey { get; set; }

		[XmlElement(ElementName = "api_version")]
		public string ApiVersion { get; set; }

		[XmlElement(ElementName = "partner_id")]
		public string PartnerId { get; set; }

		[XmlElement(ElementName = "user_id")]
		public string UserId { get; set; }

		[XmlElement(ElementName = "mahjong_transaction_id")]
		public string MahjongTransactionId { get; set; }

		[XmlElement(ElementName = "amount")]
		public string Amount { get; set; }

		[XmlElement(ElementName = "game_id")]
		public string GameId { get; set; }

		[XmlElement(ElementName = "currency")]
		public string Currency { get; set; }

		[XmlElement(ElementName = "direction")]
		public string Direction { get; set; }

		[XmlElement(ElementName = "token")]
		public string Token { get; set; }

		[XmlElement(ElementName = "echo")]
		public string Echo { get; set; }

		[XmlElement(ElementName = "rake")]
		public Rake Rake { get; set; }
	}

	[XmlRoot(ElementName = "rake")]
	public class Rake
	{

		[XmlElement(ElementName = "amount")]
		public int Amount { get; set; }

		[XmlElement(ElementName = "currency")]
		public string Currency { get; set; }
	}
}