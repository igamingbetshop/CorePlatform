namespace IqSoft.CP.ProductGateway.Helpers
{
	public class BGGamesHelpers
	{
		public static class Methods
		{
			public const string GetBalance = "get_balance";
			public const string GetUser = "get_user";
			public const string PlaceBet = "place_bet";
			public const string AddWins = "add_wins";
			public const string BetWin = "bet_and_wins";
			public const string RollbackBet = "rollback_bet";
			public const string JackpotWin = "jackpot_win";
			public const string Betslip = "betslip";
			public const string Results = "results";
			public const string Rollback = "rollback";
		}
		public static class Statuses
		{
			public const string Win = "W";
			public const string Cashout = "CS";
			public const string Lost = "L";
			public const string Cancele = "C";
			public const string Rollback = "R";
		}
	}
}