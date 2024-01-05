using IqSoft.CP.AdminWebApi.Models.RoleModels;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class RoleModel : UserRoleModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public bool IsAdmin { get; set; }

        public int? PartnerId { get; set; }

        public List<ApiPermissionModel> RolePermissions { get; set; }
    }
}