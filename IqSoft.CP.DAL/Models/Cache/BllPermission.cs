using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllPermission
    {
        public string Id { get; set; }

        public int PermissionGroupId { get; set; }

        public string Name { get; set; }

        public int ObjectTypeId { get; set; }
    }
}
