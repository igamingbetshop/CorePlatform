using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace IqSoft.CP.ProductGateway.Models.Mahjong
{
	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "response_data", Namespace = "", IsNullable = false)]
	public class LoginResponse
	{
		[XmlElement(ElementName = "partner_id")]
		public string Partner_id { get; set; }

		[XmlElement(ElementName = "user_id")]
		public string User_id { get; set; }

		[XmlElement(ElementName = "currency")]
		public string Currency { get; set; }

		[XmlElement(ElementName = "k_y_c")]
		public bool K_y_c { get; set; }

		[XmlElement(ElementName = "first_name")]
		public string First_name { get; set; }

		[XmlElement(ElementName = "last_name")]
		public string Last_name { get; set; }

		[XmlElement(ElementName = "email")]
		public string Email { get; set; }

		[XmlElement(ElementName = "address")]
		public string Address { get; set; }

		[XmlElement(ElementName = "state")]
		public string State { get; set; }

		[XmlElement(ElementName = "country_code")]
		public string Country_code { get; set; }

		[XmlElement(ElementName = "language_code")]
		public string Language_code { get; set; }

		[XmlElement(ElementName = "birth_date")]
		public string Birth_date { get; set; }

		[XmlElement(ElementName = "username")]
		public string Username { get; set; }

		[XmlElement(ElementName = "signup_code")]
		public string Signup_code { get; set; }

		[XmlElement(ElementName = "registration_date")]
		public string Registration_date { get; set; }
	}

	[Serializable]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "login_response", Namespace = "", IsNullable = false)]
	public class LoginOutput
	{
		[XmlElement(ElementName = "api_version")]
		public string ApiVersion { get; set; }

		[XmlElement(ElementName = "response_data")]
		public LoginResponse LoginResponse { get; set; }

		[XmlElement(ElementName = "echo")]
		public string Echo { get; set; }

		[XmlElement(ElementName = "error_code")]
		public string ErrorCode { get; set; } = "200";
	}

}