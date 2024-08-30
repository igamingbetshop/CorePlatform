using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace IqSoft.CP.Integration.Products.Models.ImperiumGames
{
	public class LaunchOutput
	{
		public string status { get; set; }
		public string error { get; set; }
		public Content content { get; set; }
	}

	public class Content
	{
		public object game { get; set; }
		public object gameList { get; set; }
	}

	public class LaunchUrl
	{
		public string url { get; set; }
	}

	public class Games
	{
		public List<Game> gameList { get; set; }
	}
	public class Game
	{
		public string id { get; set; }
		public string name { get; set; }
		public string name_cn { get; set; }
		public string name_kr { get; set; }
		public string img { get; set; }
		public string label { get; set; }
		public string device { get; set; }
		public string title { get; set; }
		public string categories { get; set; }
		public string flash { get; set; }
		public string vertical { get; set; }
		public string bm { get; set; }
		public string demo { get; set; }
		public string localhost { get; set; }
		public string rewriterule { get; set; }
		public string lines { get; set; }
		public string width { get; set; }
		public string wager { get; set; }
		public string bonus { get; set; }
		public string exitButton { get; set; }
		public string disableReload { get; set; }
		public string menu { get; set; }
		public string system_name2 { get; set; }
	}
}
