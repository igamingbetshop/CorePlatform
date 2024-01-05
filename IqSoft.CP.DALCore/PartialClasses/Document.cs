using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Document : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Document; }
        }

        [NotMapped]
        public long Barcode
        {
            get { return CommonFunctions.CalculateBarcode(Id); }
        }

        [NotMapped]
        public int? BonusId { get; set; }
    }
}
