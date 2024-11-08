﻿using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.SoftLand
{
	public class AuthorizationOutput
	{
		[JsonProperty(PropertyName = "access_token")]
		public string AccessToken { get; set; }

		[JsonProperty(PropertyName = "expires_in")]
		public int ExpiresIn { get; set; }

		[JsonProperty(PropertyName = "token_type")]
		public string TokenType { get; set; }

		[JsonProperty(PropertyName = "refresh_token")]
		public string RefreshToken { get; set; }

		[JsonProperty(PropertyName = "scope")]
		public string Scope { get; set; }
	}
}
