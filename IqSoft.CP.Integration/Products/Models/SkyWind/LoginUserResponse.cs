using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Products.Models.SkyWind
{
	public class LoginUserResponse
	{
		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		[JsonProperty(PropertyName = "accessToken")]
		public string AccessToken { get; set; }

		[JsonProperty(PropertyName = "key")]
		public string Key { get; set; }
	}
}
