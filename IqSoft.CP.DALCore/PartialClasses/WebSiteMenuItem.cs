using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class WebSiteMenuItem : IBase
    {
        [NotMapped]
        public int PartnerId { get; set; }

        [NotMapped]
        public string Image { get; set; }

        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId { get; set; } = (int)ObjectTypes.WebSiteMenuItem;
    }
}
