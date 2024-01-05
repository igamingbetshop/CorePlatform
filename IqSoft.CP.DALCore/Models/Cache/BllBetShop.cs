using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllBetShop
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public int PartnerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

		public bool PrintLogo { get; set; }
        public string Ips { get; set; }
	}
}
