using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class AuthorizationOutput : ResponseBase
    {
        public string Token { get; set; }
        public string ClientId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string NickName { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public string BetShopCurrencyId { get; set; }
		public string UserName { get; set; }
		public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Gender { get; set; }
        public decimal AvailableBalance { get; set; }
        public DateTime BirthDate { get; set; }
        public int? BetShopId { get; set; }
        public int? CashDeskId { get; set; }
        public string BetShopName { get; set; }
        public string BetShopAddress { get; set; }
        public int DepositCount { get; set; }
        public int? UserId { get; set; }
        public List<ApiBonus> Bonuses { get; set; }
    }
}