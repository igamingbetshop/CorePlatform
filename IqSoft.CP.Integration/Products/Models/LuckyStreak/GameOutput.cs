using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Products.Models.LuckyStreak
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class BaccaratStatistic
	{
		public int main { get; set; }
		public List<int> winSideBets { get; set; }
	}

	public class BlackjackDetails
	{
		public int numberOfSeats { get; set; }
		public int occupiedSeats { get; set; }
		public List<object> occupiedSeatIds { get; set; }
	}

	public class Data
	{
		public List<Game> games { get; set; }
	}

	public class Dealer
	{
		public string name { get; set; }
		public string avatarUrl { get; set; }
		public string thumbnailAvatarURL { get; set; }
		public List<object> languages { get; set; }
	}

	public class Details
	{
		public int numberOfSeats { get; set; }
		public int occupiedSeats { get; set; }
		public List<object> occupiedSeatIds { get; set; }
	}

	public class Product : Game
	{
		public string provider { get; set; }
		public string externalId { get; set; }
		public string imageUrl { get; set; }
	}

		public class Game
	{
		public string id { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public bool isOpen { get; set; }
		public string launchUrl { get; set; }
		public string demoUrl { get; set; }
		public int providerId { get; set; }
		//public bool isPopular { get; set; }
		//public object pgwServerHostId { get; set; }
		public Dealer dealer { get; set; }
		//public List<OpenHour> openHours { get; set; }
		//public List<object> limitGroups { get; set; }
		//public object operatorIdsExcl { get; set; }
		//public BlackjackDetails blackjackDetails { get; set; }
		//public RouletteStatistics rouletteStatistics { get; set; }
		//public List<BaccaratStatistic> baccaratStatistics { get; set; }
		//public List<LastRoundsWinner> lastRoundsWinners { get; set; }
		//public object statistics { get; set; }
		//public Details details { get; set; }
	}

	public class LastRoundsWinner
	{
		public int roundId { get; set; }
		public List<RoundWinner> roundWinners { get; set; }
	}

	public class OpenHour
	{
		public string open { get; set; }
		public string close { get; set; }
	}

	public class GameOutput
	{
		public object data { get; set; }
		public object errors { get; set; }
	}

	public class Error
	{
		public string code { get; set; }
		public string title { get; set; }
		public string detail { get; set; }
		public object additional_fields { get; set; }
	}

	public class RouletteStatistics
	{
		public List<int> latestResults { get; set; }
	}

	public class RoundWinner
	{
		public string nickname { get; set; }
		public string currency { get; set; }
		public double winAmount { get; set; }
	}


}
