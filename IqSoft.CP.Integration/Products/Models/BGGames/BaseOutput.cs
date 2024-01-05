using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.BGGames
{
	public class BaseOutput
	{
		public List<Response> response { get; set; }
		public Error error { get; set; }
	}

	public class Response
	{
		public string gamecode { get; set; }
		public string name { get; set; }
		public string imagePath { get; set; }
		public string brand { get; set; }
		public string type { get; set; }
		public List<string> languages { get; set; }
		public List<string> platforms { get; set; }
		public List<string> currencies { get; set; }
		public List<string> modes { get; set; }
	}

	public class Error
	{
		public int code { get; set; }
		public string description { get; set; }
	}


}
