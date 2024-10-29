using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.User;
using log4net;
using Newtonsoft.Json;

namespace IqSoft.CP.BLL.Services
{
    public class PermissionBll : BaseBll, IPermissionBll
    {
        #region Constructors

        public PermissionBll(SessionIdentity identity, ILog log, int? timeout = null) : base(identity, log, timeout)
        {

        }

        public PermissionBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public CheckPermissionOutput GetPermissionsToObject(CheckPermissionInput input)
        {
            var response = new CheckPermissionOutput();
            var userPermissions = CacheManager.GetUserPermissions(input.UserId ?? Identity.Id);
            var permission = userPermissions.FirstOrDefault(x => x.IsAdmin);
            if (permission == null)
                permission = userPermissions.FirstOrDefault(x => x.IsForAll && x.PermissionId == input.Permission);
            if (permission == null)
                permission = userPermissions.FirstOrDefault(x => x.PermissionId == input.Permission);
            if (permission == null)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            
            if (permission.IsForAll || permission.IsAdmin)
            {
                response.HaveAccessForAllObjects = true;
                return response;
            }
            if (input.ObjectTypeId.HasValue)
            {
                var objects = CacheManager.GetUserAccessObjects(input.UserId ?? Identity.Id);
                response.AccessibleObjects = objects.Where(x => x.ObjectTypeId == (int)input.ObjectTypeId &&
                        x.PermissionId == input.Permission && Int64.TryParse(x.ObjectId, out long r)).Select(x => Convert.ToInt64(x.ObjectId)).ToList();
                response.AccessibleIntegerObjects = objects.Where(x => x.ObjectTypeId == (int)input.ObjectTypeId &&
                        x.PermissionId == input.Permission && Int32.TryParse(x.ObjectId, out int r)).Select(x => Convert.ToInt32(x.ObjectId)).ToList();
                response.AccessibleStringObjects = objects.Where(x => x.ObjectTypeId == (int)input.ObjectTypeId &&
                        x.PermissionId == input.Permission).Select(x => x.ObjectId).ToList();
            }
            return response;
        }

        public void CheckPermission(string permissionId, bool checkForAll = true)
        {
            var permissions = CacheManager.GetUserPermissions(Identity.Id).Where(x => x.PermissionId == permissionId || x.IsAdmin).ToList();
            if (permissions == null || permissions.Count == 0)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (checkForAll && !permissions.Any(x => x.IsForAll) && !permissions.Any(x => x.IsAdmin))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
        }

        public bool CheckPermissionAvailability(string permissionId, bool checkForAll = true)
        {
            var permissions = CacheManager.GetUserPermissions(Identity.Id).Where(x => x.PermissionId == permissionId || x.IsAdmin).ToList();
            if (permissions == null || permissions.Count == 0)
                return false;
            if (checkForAll && !permissions.Any(x => x.IsForAll) && !permissions.Any(x => x.IsAdmin))
                return false;

            return true;
        }

        public void CheckPermissionToSaveObject(CheckPermissionInput input)
        {
            var permissions =
                CacheManager.GetUserPermissions(Identity.Id).Where(x => x.PermissionId == input.Permission || x.IsAdmin).ToList();
            if (permissions == null || permissions.Count == 0)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (permissions.Any(x => x.IsForAll) || permissions.Any(x => x.IsAdmin))
                return;
            if (input.ObjectTypeId.HasValue &&
                    CacheManager.GetUserAccessObjects(Identity.Id)
                        .Any(
                            x =>
                                x.ObjectTypeId == (int)input.ObjectTypeId && x.ObjectId == input.ObjectId.ToString() &&
                                x.PermissionId == input.Permission))
                return;
            throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
        }

        public List<Role> GetRoles(int partnerId)
        {
            var roles = Db.Roles.Where(x => x.PartnerId == partnerId || x.PartnerId == null).ToList();
            Log.Info("GetRoles_" + JsonConvert.SerializeObject(roles.Where(x => x.RolePermissions != null).SelectMany(x => x.RolePermissions.Select(y => y.Id)).ToList() ?? new List<int>()));

            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewRole,
                ObjectTypeId = (int)ObjectTypes.Role
            });

            if (!checkP.HaveAccessForAllObjects)
                roles = roles.Where(x => checkP.AccessibleObjects.Contains(x.ObjectId)).ToList();

            Log.Info("GetRoles1_" + JsonConvert.SerializeObject(roles.Where(x => x.RolePermissions != null).SelectMany(x => x.RolePermissions.Select(y => y.Id)).ToList() ?? new List<int>()));
            return roles;
        }

        public PagedModel<Role> GetRolesPagedModel(FilterRole filter)
        {
            CreateFilterWithPermissions(filter);

            return new PagedModel<Role>
            {
                Entities = filter.FilterObjects(Db.Roles, r => r.OrderBy(x => x.Id)),
                Count = filter.SelectedObjectsCount(Db.Roles)
            };
        }

        public Role GetRoleById(int id)
        {
            CheckPermission(Constants.Permissions.ViewRole);
            var role = Db.Roles.FirstOrDefault(x => x.Id == id);
            if (role == null)
                return null;
            role.RolePermissions = Db.RolePermissions.Where(x => x.RoleId == id).ToList();
            return role;
        }

        public Role GetRoleByName(string name)
        {
            var role = Db.Roles.FirstOrDefault(x => x.Name == name);
            return role;
        }

        public Role SaveRole(Role role)
        {
            if (role.PartnerId == Constants.MainPartnerId)
                role.PartnerId = null;

            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateRole,
                ObjectTypeId = (int)ObjectTypes.Role
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            if ((!checkPermissionResult.HaveAccessForAllObjects && !checkPermissionResult.AccessibleObjects.Contains(role.Id)) ||
                (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != role.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var dbRole = Db.Roles.Include(x => x.RolePermissions).FirstOrDefault(x => x.Id == role.Id);
            if (dbRole == null)
            {
                dbRole = new Role { 
                    Id = role.Id,
                    Name = role.Name,
                    IsStatic = role.IsStatic,
                    IsAdmin = role.IsAdmin,
                    Comment = role.Comment,
                    PartnerId = role.PartnerId
                };
                Db.Roles.Add(dbRole);
                Db.SaveChanges();
            }
            var userPermissions = CacheManager.GetUserPermissions(Identity.Id);
            var userRoles = GetUserRoles(Identity.Id);
            var isAdmin = userRoles.Any(x => x.IsAdmin);

            var dbRolePermissions = dbRole.RolePermissions.ToList();
            foreach (var dbRolePermission in dbRolePermissions)
            {
                var newRolePermission = role.RolePermissions.FirstOrDefault(
                        x => x.PermissionId == dbRolePermission.PermissionId && x.RoleId == dbRolePermission.RoleId);
                if (newRolePermission == null)
                    Db.RolePermissions.Remove(dbRolePermission);
                else
                {
                    if (!isAdmin)
                    {
                        var up = userPermissions.FirstOrDefault(x => x.PermissionId == dbRolePermission.PermissionId);
                        if (up == null || (!up.IsForAll && newRolePermission.IsForAll))
                            throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                    }
                    dbRolePermission.IsForAll = newRolePermission.IsForAll;
                    role.RolePermissions.Remove(newRolePermission);
                }
            }
            foreach (var rolePermission in role.RolePermissions)
            {
                if (!isAdmin)
                {
                    var up = userPermissions.FirstOrDefault(x => x.PermissionId == rolePermission.PermissionId);
                    if (up == null || (!up.IsForAll && rolePermission.IsForAll))
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                }

                Db.RolePermissions.Add(new RolePermission
                {
                    RoleId = rolePermission.RoleId,
                    PermissionId = rolePermission.PermissionId,
                    IsForAll = rolePermission.IsForAll
                });
            }
            Db.SaveChanges();

            var users = Db.UserRoles.Where(x => x.RoleId == role.Id).Select(x => x.UserId).ToList();
            foreach (var u in users)
            {
                CacheManager.DeleteUserPermissions(u);
            }
            return dbRole;
        }

        public Role CloneRole(int roleId, string newRoleName)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateRole,
                ObjectTypeId = (int)ObjectTypes.Role
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var dbRole = Db.Roles.Include(x => x.RolePermissions).FirstOrDefault(x => x.Id == roleId);
            if (dbRole == null)
                throw CreateException(LanguageId, Constants.Errors.RoleNotFound);

            if ((!checkPermissionResult.HaveAccessForAllObjects && !checkPermissionResult.AccessibleObjects.Contains(roleId)) || (dbRole.PartnerId.HasValue &&
                (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbRole.PartnerId))))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var newRole = new Role
            {
                Name = newRoleName,
                IsStatic = dbRole.IsStatic,
                IsAdmin = dbRole.IsAdmin,
                Comment = dbRole.Comment,
                PartnerId = dbRole.PartnerId
            };
            Db.Roles.Add(newRole);
            Db.SaveChanges();
            var permissions = new List<RolePermission>();
            dbRole.RolePermissions.ToList().ForEach(x => permissions.Add(new RolePermission { RoleId = newRole.Id, PermissionId = x.PermissionId, IsForAll = x.IsForAll }));
            newRole.RolePermissions = permissions;
            Db.SaveChanges();
            return newRole;
        }

        public List<Role> GetUserRoles(int userId)
        {
            CheckPermission(Constants.Permissions.ViewUserRole);
            return Db.UserRoles.Include(x => x.Role).Where(x => x.UserId == userId).Select(x => x.Role).ToList();
        }

        public List<UserRole> GetRoleUsers(int roleId)
        {
            CheckPermission(Constants.Permissions.ViewRole);
            CheckPermission(Constants.Permissions.ViewUserRole);
            var userAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = (int)ObjectTypes.User
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var query = Db.UserRoles.Include(x => x.User).Where(x => x.RoleId == roleId);
            if (!userAccess.HaveAccessForAllObjects)
                query = query.Where(x => userAccess.AccessibleObjects.Contains(x.UserId));
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleIntegerObjects.Contains(x.User.PartnerId));

            return query.ToList();
        }

        public List<RolePermissionModel> GetRolePermissions(int roleId, int userId)
        {
            CheckPermission(Constants.Permissions.ViewUserRole);
            return Db.RolePermissions.Where(x => x.RoleId == roleId).Select(x => new RolePermissionModel
            {
                Id = x.Id,
                RoleId = x.RoleId,
                PermissionId = x.PermissionId,
                IsForAll = x.IsForAll,
                Permission = x.Permission == null ? null : new PermissionModel 
                {
                    Id = x.Permission.Id,
                    PermissionGroupId = x.Permission.PermissionGroupId,
                    Name = x.Permission.Name,
                    ObjectTypeId = x.Permission.ObjectTypeId,
                    AccessObjects = x.Permission.AccessObjects.Where(y => y.UserId == userId).ToList()
                }
            }).ToList();
        }

        public void SaveUserRoles(int userId, List<UserRole> userRoles)
        {
            CheckPermission(Constants.Permissions.CreateUserRole);
            var dbUserRoles = Db.UserRoles.Where(x => x.UserId == userId).ToList();
            var user = CacheManager.GetUserById(userId);
            var admin = CacheManager.GetUserPermissions(Identity.Id).FirstOrDefault(x => x.IsAdmin);
            var isAdmin = (admin != null);
            var dbRoles = Db.Roles.Where(x => x.PartnerId == user.PartnerId || (isAdmin && x.PartnerId == null)).Select(x => x.Id).ToList();
            userRoles = userRoles.Where(x => dbRoles.Contains(x.RoleId)).ToList();
            // delete old role permissions
            foreach (var userRole in dbUserRoles)
            {
                var newUserRole = userRoles.FirstOrDefault(x => x.UserId == userRole.UserId && x.RoleId == userRole.RoleId);
                if (newUserRole == null)
                    Db.UserRoles.Remove(userRole);
                else
                    userRoles.Remove(newUserRole);
            }

            foreach (var userRole in userRoles)
            {
                var dbUserRole = new UserRole();
                Db.UserRoles.Add(dbUserRole);
                Db.Entry(dbUserRole).CurrentValues.SetValues(userRole);
            }
            SaveChanges();
            CacheManager.DeleteUserPermissions(userId);
        }

        public Role GetUserCustomRole(int userId)
        {
            var customUserRole =
                Db.UserRoles.Include(x => x.Role.RolePermissions).FirstOrDefault(x => x.UserId == userId && !x.Role.IsStatic);
            return customUserRole == null
                ? new Role { Name = string.Format(Constants.UserCustomRoleFormat, userId) }
                : customUserRole.Role;
        }

        public void SaveAccessObjects(int userId, string permissionId, List<AccessObject> accessObjects)
        {
            CheckPermission(Constants.Permissions.CreateUserRole);

            var dbUserAccessObjects = Db.AccessObjects.Where(x => x.UserId == userId && x.PermissionId == permissionId).ToList();

            // delete old role accessObjects
            foreach (var accessObject in dbUserAccessObjects)
            {
                var newAccessObject = accessObjects.FirstOrDefault(x => x.UserId == accessObject.UserId &&
                    x.PermissionId == accessObject.PermissionId && x.ObjectId == accessObject.ObjectId && x.ObjectTypeId == accessObject.ObjectTypeId);

                if (newAccessObject == null)
                    Db.AccessObjects.Remove(accessObject);
                else
                    accessObjects.Remove(newAccessObject);
            }
            var user = Db.Users.FirstOrDefault(x => x.Id == userId);
            foreach (var accessObject in accessObjects)
            {
                if (accessObject.ObjectTypeId == (int)ObjectTypes.Partner &&
                    user.PartnerId != Constants.MainPartnerId && accessObject.ObjectId != user.PartnerId.ToString())
                    throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                var dbUserAccessobject = new AccessObject();
                Db.AccessObjects.Add(dbUserAccessobject);
                Db.Entry(dbUserAccessobject).CurrentValues.SetValues(accessObject);
            }
            SaveChanges();
            CacheManager.DeleteUserPermissions(userId);
            CacheManager.UpdateUserAccessObjectsInCache(userId);
        }

        private void CreateFilterWithPermissions(FilterRole filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewRole,
                ObjectTypeId = (int)ObjectTypes.Role
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<Role>>
            {
                new CheckPermissionOutput<Role>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<Role>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => !x.PartnerId.HasValue || partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId.Value)
                }
            };
        }

        public Dictionary<string, string> GetBulkEditFields(int adminMenuId, int gridIndex)
        {
            var resp = new Dictionary<string, string>(); 
            switch (adminMenuId) //TO DO: Change this part and take from the db, like in the case of regform
            {
                case 261:
                    var hasPercentPermission = CheckPermissionAvailability(Constants.Permissions.ViewProductCommissionPercent, true);
                    switch (gridIndex)
                    {
                        case 0:
                            if(hasPercentPermission)
                                resp.Add("Percent", "Number");
                            resp.Add("Rating", "Number");
                            resp.Add("OpenMode", "Number");
                            resp.Add("CategoryIds", "ProductCategory");
                            resp.Add("State", "ProductState");
                            break;
                        case 1:
                            if (hasPercentPermission)
                                resp.Add("Percent", "Number");
                            resp.Add("Rating", "Number");
                            resp.Add("OpenMode", "Number");
                            resp.Add("RTP", "Number");
                            resp.Add("CategoryIds", "ProductCategory");
                            resp.Add("State", "ProductState");
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return resp;
        }
    }
}