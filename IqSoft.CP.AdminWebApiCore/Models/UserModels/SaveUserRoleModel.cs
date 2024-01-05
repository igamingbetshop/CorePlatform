using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class SaveUserRoleModel
    {
        public int UserId { get; set; }

        public List<RoleModel> RoleModels { get; set; }
    }
}