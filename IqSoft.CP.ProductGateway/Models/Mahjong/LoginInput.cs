using System;
using System.ComponentModel;
using System.Xml.Serialization;
namespace IqSoft.CP.ProductGateway.Models.Mahjong
{
	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("login", Namespace = "", IsNullable = false)]
	public class LoginInput
	{
		[XmlElement(ElementName = "secret_key")]
		public string SecretKey { get; set; }

		[XmlElement(ElementName = "api_version")]
		public string ApiVersion { get; set; }

		[XmlElement(ElementName = "partner_id")]
		public string PartnerId { get; set; }

		[XmlElement(ElementName = "token")]
		public string Token { get; set; }

		[XmlElement(ElementName = "ip")]
		public string Ip { get; set; }

		[XmlElement(ElementName = "echo")]
		public string Echo { get; set; }
	}

}
