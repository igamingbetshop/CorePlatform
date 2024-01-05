using System;
using System.ComponentModel;
using System.Xml.Serialization;
namespace IqSoft.CP.PaymentGateway.Models.BRPay
{
	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("response", Namespace = "", IsNullable = false)]
	public class PaymentResponse
	{
		[XmlElement(ElementName = "sp_salt")]
		public string Salt { get; set; }

		[XmlElement(ElementName = "sp_status")]
		public string Status { get; set; }

		[XmlElement(ElementName = "sp_sig")]
		public string Signature { get; set; }

		[XmlElement(ElementName = "sp_error_description ")]
		public string ErrorDescription { get; set; }
	}
}