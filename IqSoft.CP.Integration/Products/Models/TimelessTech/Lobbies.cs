using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.TimelessTech
{
	public class Lobbies
	{
		public List<Lobby> lobbies { get; set; }
	}

	public class Lobby
	{
		public string lobby_id { get; set; }
		public string vendor { get; set; }
		public string platform { get; set; }
		public string subtype { get; set; }
		public Details details { get; set; }
	}
}
