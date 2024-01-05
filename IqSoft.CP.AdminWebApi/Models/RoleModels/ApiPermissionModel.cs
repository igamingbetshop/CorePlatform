using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.RoleModels
{
    public class ApiPermissionModel
    {
        public int UserId { get; set; }

        public int Id { get; set; }

        public int RoleId { get; set; }

        public string Permissionid { get; set; }

        public bool IsForAll { get; set; }

        public List<AccessObjectModel> AccessObjects { get; set; }

        public List<long> AccessObjectsIds { get; set; }
    }
}