using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
	public partial class fnTranslation
	{
		[NotMapped]
		public int PartnerId { get; set; }
	}
}
