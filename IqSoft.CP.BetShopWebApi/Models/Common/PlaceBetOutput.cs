using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PlaceBetOutput : ClientRequestResponseBase
	{
		public string Token;

		public List<BetOutput> Bets { get; set; }

		private decimal _balance;
		public decimal Balance
		{
			get { return Math.Floor(_balance * 100) / 100; }
			set { _balance = value; }
		}

		private decimal _currentLimit;
		public decimal CurrentLimit
		{
			get { return Math.Floor(_currentLimit * 100) / 100; }
			set { _currentLimit = value; }
		}

		public PlaceBetOutput()
		{
			Bets = new List<BetOutput>();
		}
	}

	public class BetOutput : ClientRequestResponseBase
	{
		public decimal BetAmount { get; set; }
		public decimal AmountPerBet { get; set; }
		public long Barcode { get; set; }
		public DateTime BetDate { get; set; }
		public string Token { get; set; }
		public string TransactionId { get; set; }
		public string TicketNumber { get; set; }
		public int GameId { get; set; }
		public string GameName { get; set; }
		public decimal WinAmount { get; set; }
		public decimal PossibleBonus { get; set; }
		private decimal _balance;
		public decimal Balance
		{
			get { return Math.Floor(_balance * 100) / 100; }
			set { _balance = value; }
		}
		public decimal CurrentLimit { get; set; }
		public decimal Coefficient { get; set; }
		public string Info { get; set; }
		public decimal JackpotAmount { get; set; }
		public List<BllBetSelection> BetSelections { get; set; }
		public int TypeId { get; set; }
		public decimal MinPossibleWin { get; set; }
		public int NumberOfBets { get; set; }
		public int NumberOfMatches { get; set; }
		public decimal CommissionFee { get; set; }
		public decimal PossibleWin { get; set; }
		public int Status { get; set; }
		public List<int> SystemOutCounts { get; set; }
		public decimal CashoutAmount { get; set; }
		public bool BlockedForCashout { get; set; }
	}

	public class BllBetSelection
	{
		public int RoundId { get; set; }

		public int UnitId { get; set; }

		public string UnitName { get; set; }

		public string RoundName { get; set; }

		public int MarketTypeId { get; set; }

		public long MarketId { get; set; }

		public string MarketName { get; set; }

		public long SelectionTypeId { get; set; }

		public long SelectionId { get; set; }

		public int Status { get; set; }

		public string SelectionName { get; set; }

		public decimal Coefficient { get; set; }

		public DateTime? EventDate { get; set; }

		public string EventInfo { get; set; }

		public int ResponseCode { get; set; }

		public string Description { get; set; }

		public int ProductId { get; set; }
		
		public bool ForcedChosen { get; set; }
		
		public int MatchId { get; set; }
		
		public string SelectionHeader { get; set; }
		
		public string StatusName { get; set; }

		public string MatchState { get; set; }

		public string MatchStatisticsInfo { get; set; }
	}
}