using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetShopBetsOutput : ClientRequestResponseBase
	{
		public List<BetShopBet> Bets { get; set; }
	}
}