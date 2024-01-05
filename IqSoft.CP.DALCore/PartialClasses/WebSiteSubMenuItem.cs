using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{	
	public partial class WebSiteSubMenuItem
	{
		[NotMapped]
		public int PartnerId { get; set; }
		[NotMapped]
		public string MenuItemName { get; set; }
		[NotMapped]
		public string Image { get; set; }
	}
}