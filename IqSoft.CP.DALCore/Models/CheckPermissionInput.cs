using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.DAL.Models
{
    public class CheckPermissionInput
    {
        public string Permission { get; set; }

        public ObjectTypes? ObjectTypeId { get; set; }

        public long ObjectId { get; set; }

        public int? UserId { get; set; }
    }
}
