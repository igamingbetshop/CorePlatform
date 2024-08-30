using System.Collections.Generic;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.User;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IPermissionBll : IBaseBll
    {
        Role GetRoleById(int id);

		Role GetRoleByName(string name);
			
		List<Role> GetRoles(FilterRole filter);

        PagedModel<Role> GetRolesPagedModel(FilterRole filter);

		void CheckPermission(string checkPermission, bool checkForAll = true);

        Role SaveRole(Role role);

        List<Role> GetUserRoles(int userId);

        List<RolePermissionModel> GetRolePermissions(int roleId, int userId);

        void SaveUserRoles(int userId, List<UserRole> userRoles);

        void SaveAccessObjects(int userId, string permissionId, List<AccessObject> accessObjects);
    }
}
