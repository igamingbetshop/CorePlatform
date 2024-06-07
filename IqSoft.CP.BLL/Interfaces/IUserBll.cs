using System.Collections.Generic;
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IUserBll : IBaseBll
    {
        SessionIdentity LoginUser(LoginInput loginInput, out string imageData);

        void LogoutUser(string token);

        UserSession GetUserSession(string token, bool checkExpiration = true, bool extendSession = true);

        UserSession GetUserSessionById(long id);

        UserSession CreateProductSession(SessionIdentity session, int productId);

        UserSession RefreshUserSession(string token);

        User AddUser(User user, bool chechPermission = true, AgentEmployeePermissionModel permission = null);

		User EditUser(User user, bool checkPermission, AgentEmployeePermissionModel permission = null);

		User GetUserById(int id);

        PagedModel<fnUser> GetUsersPagedModel(FilterfnUser filter, bool checkPermission);

        bool IsUserNameExists(int partnerId, string userName, int? level, char startWith, bool checkPermission);

        List<User> ExportUsersModel(FilterUser filter);

        string GetProjectToken(int projectTypeId, SessionIdentity identity);
    }
}
