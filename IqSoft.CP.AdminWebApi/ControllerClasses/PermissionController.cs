using System;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.AdminWebApi.Models.UserModels;
using IqSoft.CP.AdminWebApi.Models.RoleModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class PermissionController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "SaveRole":
                    return SaveRole(JsonConvert.DeserializeObject<RoleModel>(request.RequestData), identity, log);
                case "CloneRole":
                    return CloneRole(JsonConvert.DeserializeObject<CloneModel>(request.RequestData), identity, log);
                case "GetRoles":
                    return GetRoles(JsonConvert.DeserializeObject<ApiFilterRole>(request.RequestData), identity, log);
                case "GetRoleById":
                    return GetRoleById(JsonConvert.DeserializeObject<ApiFilterRole>(request.RequestData), identity, log);
                case "GetPermissions":
                    return GetPermissions();
                case "GetUserRoles":
                    return GetUserRoles(Convert.ToInt32(request.RequestData), identity, log);
                case "GetRolePermissions":
                    return GetRolePermissions(
                        JsonConvert.DeserializeObject<ApiRolePermissionModel>(request.RequestData), identity, log);
                case "SaveUserRoles":
                    return SaveUserRoles(JsonConvert.DeserializeObject<SaveUserRoleModel>(request.RequestData),
                        identity, log);
                case "SaveAccessObjects":
                    return SaveAccessObjects(JsonConvert.DeserializeObject<ApiPermissionModel>(request.RequestData),
                        identity, log);
                case "GetRoleUsers":
                    return GetRoleUsers(JsonConvert.DeserializeObject<ApiFilterRole>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase SaveUserRoles(SaveUserRoleModel userRoleModel, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                var userRoles = userRoleModel.RoleModels.Select(u => new UserRole
                {
                    RoleId = u.Id,
                    UserId = userRoleModel.UserId,
                }).ToList();

                permissionBl.SaveUserRoles(userRoleModel.UserId, userRoles);

                return new ApiResponseBase
                {
                    ResponseObject = "OK"
                };
            }
        }

        private static ApiResponseBase SaveRole(RoleModel request, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                var role = request.MapToRole();
                role.IsStatic = true;
                role.IsAdmin = false;
                role = permissionBl.SaveRole(role);
                return new ApiResponseBase
                {
                    ResponseObject = role.MapToRoleModel()
                };
            }
        }

        private static ApiResponseBase CloneRole(CloneModel input, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = permissionBl.CloneRole(input.RoleId, input.NewRoleName).MapToRoleModel()
                };
            }
        }

        public static ApiResponseBase GetRoles(ApiFilterRole request, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                var filter = request.MapToFilterFilterRole();
                filter.IsStatic = true;
                filter.IsAdmin = false;
                var roles = permissionBl.GetRolesPagedModel(filter);
                var response = new ApiResponseBase
                {
                    ResponseObject = new {roles.Count, Entities = roles.Entities.Select(x => x.MapToRoleModel()).ToList()}
                };
                return response;
            }
        }

        private static ApiResponseBase GetRoleById(ApiFilterRole request, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                if (!request.Id.HasValue)
                    return new ApiResponseBase();
                var role = permissionBl.GetRoleById(request.Id.Value);
                if (role.IsAdmin)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.RoleNotFound);
                var response = new ApiResponseBase { ResponseObject = role.MapToRoleModel() };
                return response;
            }
        }

        private static ApiResponseBase GetRoleUsers(ApiFilterRole request, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                if (!request.Id.HasValue)
                    return new ApiResponseBase();
                var roleUsers = permissionBl.GetRoleUsers(request.Id.Value);
                return new ApiResponseBase
                {
                    ResponseObject = roleUsers.Select(x =>
                    new
                    {
                        x.UserId,
                        x.User.PartnerId,
                        x.User.UserName,
                        x.User.FirstName,
                        x.User.LastName,
                        x.User.State,
                        x.RoleId
                    })
                };
            }
        }

        private static ApiResponseBase GetPermissions()
        {
            var permissions = CacheManager.GetPermissions().Select(x => x.MapToPermissionModel()).ToList();
            var response = new ApiResponseBase {ResponseObject = permissions};
            return response;
        }

        private static ApiResponseBase GetUserRoles(int userId, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                var user = CacheManager.GetUserById(userId);
                var allRoles = permissionBl.GetRoles(user.PartnerId);
                var mappedRoles = allRoles.Select(x => x.MapToRoleModel()).ToList();
                var isAdmin = CacheManager.GetUserPermissions(identity.Id).FirstOrDefault(x => x.IsAdmin);
                if (isAdmin == null)
                    mappedRoles = mappedRoles.Where(x => x.PartnerId == user.PartnerId).ToList();

                var userRoles = permissionBl.GetUserRoles(userId).Select(x => x.MapToRoleModel()).ToList();
                foreach (var role in mappedRoles)
                {
                    role.HasRole = userRoles.Any(x => x.Id == role.Id);
                }

                return new ApiResponseBase
                {
                    ResponseObject = mappedRoles.OrderByDescending(x => x.HasRole)
                };
            }
        }

        public static ApiResponseBase GetRolePermissions(ApiRolePermissionModel input, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                var rolePermissions = permissionBl.GetRolePermissions(input.RoleId, input.UserId).OrderByDescending(p => p.IsForAll);
                return new ApiResponseBase
                {
                    ResponseObject = rolePermissions.Select(x => x.MapToRolePermissionModel()).ToList()
                };
            }
        }

        private static ApiResponseBase SaveAccessObjects(ApiPermissionModel rolePermission, SessionIdentity identity, ILog log)
        {
            using (var permissionBl = new PermissionBll(identity, log))
            {
                var permission = CacheManager.GetPermissions().FirstOrDefault(p => p.Id == rolePermission.Permissionid);
                if (permission != null)
                {
                    var list = rolePermission.AccessObjectsIds.Select(accessObjectsId => new AccessObject
                    {
                        UserId = rolePermission.UserId,
                        ObjectTypeId = permission.ObjectTypeId,
                        ObjectId = accessObjectsId,
                        PermissionId = rolePermission.Permissionid
                    }).ToList();

                    permissionBl.SaveAccessObjects(rolePermission.UserId, rolePermission.Permissionid, list);

                    return new ApiResponseBase
                    {
                        ResponseObject = "OK"
                    };
                }
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.NotAllowed
                };
            }
        }
    }
}