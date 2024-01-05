using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Products
{
	public class PartnerProduct
	{
		public string WebImageUrl { get; set; }
		public string MobileImageUrl { get; set; }
		public string BackgroundImageUrl { get; set; }
		public long TranslationId { get; set; }
		public string NickName { get; set; }
		public int ProviderId { get; set; }
		public string ExternalId { get; set; }
		public int ProductId { get; set; }
		public string ProviderName { get; set; }
		public bool IsForMobile { get; set; }
		public bool IsForDesktop { get; set; }
		public bool HasDemo { get; set; }
		public string Jackpot { get; set; }
		public decimal Rating { get; set; }
		public int? SubproviderId { get; set; }
		public string SubproviderName { get; set; }
        public int? OpenMode { get; set; }
    }
}
