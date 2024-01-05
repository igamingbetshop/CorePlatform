namespace IqSoft.CP.ProductGateway.Helpers
{
	public class TimelessTechHelpers
	{
		public static class Methods
		{
			public const string Authenticate = "authenticate";
			public const string Balance = "balance";
			public const string Changebalance = "changebalance";
			public const string Status = "status";
			public const string Cancel = "cancel";
			public const string Finishround = "finishround";
		}

		public static class TransactionType
		{
			public const string BET = "BET";
			public const string WIN = "WIN";
			public const string REFUND = "REFUND";
		}

		public static class TransactionStatus
		{
			public const string OK = "OK";
			public const string ERROR = "ERROR";
			public const string CANCELED = "CANCELED";
		}
	}
}