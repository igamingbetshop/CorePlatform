using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllFnUserPermission
    {
        public int UserId { get; set; }

        public string PermissionId { get; set; }

        public bool IsForAll { get; set; }

        public bool IsAdmin { get; set; }
    }
}
