using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.User
{
    public class PermissionModel
    {
        public string Id { get; set; }
        public int PermissionGroupId { get; set; }
        public string Name { get; set; }
        public int ObjectTypeId { get; set; }
        public List<AccessObject> AccessObjects { get; set; }
    }
}
