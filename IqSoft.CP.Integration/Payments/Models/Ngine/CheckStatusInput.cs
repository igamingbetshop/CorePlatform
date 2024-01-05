using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Ngine
{
    [XmlRoot(ElementName = "CheckStatus")]
    public class CheckStatusInput
    {
		[XmlElement(ElementName = "TransactionID")]
		public long TransactionID { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Body")]
	public class Body
	{

		[XmlElement(ElementName = "CheckStatus")]
		public CheckStatusInput CheckStatus { get; set; }
	}

	[XmlRoot(ElementName = "Envelope")]
	public class Envelope
	{

		[XmlElement(ElementName = "Body")]
		public Body Body { get; set; }

		[XmlAttribute(AttributeName = "xsi")]
		public string Xsi { get; set; }

		[XmlAttribute(AttributeName = "xsd")]
		public string Xsd { get; set; }

		[XmlAttribute(AttributeName = "soap")]
		public string Soap { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

}
