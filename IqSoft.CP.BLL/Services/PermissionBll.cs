using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
            var permission = userPermissions.FirstOrDefault(x => x.PermissionId == input.Permission || x.IsAdmin);
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
                        x.PermissionId == input.Permission).Select(x => x.ObjectId).AsQueryable();
                response.AccessibleStringObjects = objects.Where(x => x.ObjectTypeId == (int)input.ObjectTypeId &&
                        x.PermissionId == input.Permission).Select(x => x.ObjectId.ToString()).AsQueryable();
            }
            return response;
        }

        public CheckPermissionOutput CheckPermission(string permissionId)
        {
            var permissions = CacheManager.GetUserPermissions(Identity.Id).Where(x => x.PermissionId == permissionId || x.IsAdmin).ToList();

            var p = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (permissions == null || permissions.Count == 0)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (permissions.Any(x => x.IsForAll) || permissions.Any(x => x.IsAdmin))
                return p;
            throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
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
                                x.ObjectTypeId == (int)input.ObjectTypeId && x.ObjectId == input.ObjectId &&
                                x.PermissionId == input.Permission))
                return;
            throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
        }

        public List<Role> GetRoles(FilterRole filter)
        {
            CreateFilterWithPermissions(filter);
            return filter.FilterObjects(Db.Roles).ToList();
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
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateRole,
                ObjectTypeId = ObjectTypes.Role
            });
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if ((!checkPermissionResult.HaveAccessForAllObjects && !checkPermissionResult.AccessibleObjects.Contains(role.Id)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != role.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbRole = Db.Roles.Include(x => x.RolePermissions).FirstOrDefault(x => x.Id == role.Id);
            if (dbRole == null)
            {
                dbRole = new Role { Id = role.Id };
                Db.Roles.Add(dbRole);
            }
            Db.Entry(dbRole).CurrentValues.SetValues(role);

            var rolePermissions = dbRole.RolePermissions.ToList();
            // delete old role permissions
            foreach (var rolePermission in rolePermissions)
            {
                var newRolePermission =
                    role.RolePermissions.FirstOrDefault(
                        x => x.PermissionId == rolePermission.PermissionId && x.RoleId == rolePermission.RoleId);
                if (newRolePermission == null)
                    Db.RolePermissions.Remove(rolePermission);
                else
                {
                    rolePermission.Id = newRolePermission.Id;
                    Db.Entry(rolePermission).CurrentValues.SetValues(newRolePermission);
                    role.RolePermissions.Remove(newRolePermission);
                }
            }

            // add new role permissions
            foreach (var rolePermission in role.RolePermissions)
            {
                var dbRolePermission = new RolePermission();
                Db.RolePermissions.Add(dbRolePermission);
                Db.Entry(dbRolePermission).CurrentValues.SetValues(rolePermission);
            }
            SaveChanges();
            var users = Db.UserRoles.Where(x => x.RoleId == role.Id).Select(x => x.UserId).ToList();
            foreach (var u in users)
            {
                CacheManager.UpdateUserPermissionsInCache(u);
            }
            return dbRole;
        }
        public Role CloneRole(int roleId, string newRoleName)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateRole,
                ObjectTypeId = ObjectTypes.Role
            });
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var dbRole = Db.Roles.Include(x => x.RolePermissions).FirstOrDefault(x => x.Id == roleId);
            if (dbRole == null)
                throw CreateException(LanguageId, Constants.Errors.RoleNotFound);

            if ((!checkPermissionResult.HaveAccessForAllObjects && !checkPermissionResult.AccessibleObjects.Contains(roleId)) || (dbRole.PartnerId.HasValue &&
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != dbRole.PartnerId))))
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
                ObjectTypeId = ObjectTypes.User
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var query = Db.UserRoles.Include(x => x.User).Where(x => x.RoleId == roleId);
            if (!userAccess.HaveAccessForAllObjects)
                query = query.Where(x => userAccess.AccessibleObjects.Contains(x.UserId));
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.User.PartnerId));

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
            CacheManager.UpdateUserPermissionsInCache(userId);
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
                    user.PartnerId != Constants.MainPartnerId &&
                    accessObject.ObjectId != user.PartnerId)
                    throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                var dbUserAccessobject = new AccessObject();
                Db.AccessObjects.Add(dbUserAccessobject);
                Db.Entry(dbUserAccessobject).CurrentValues.SetValues(accessObject);
            }
            SaveChanges();
            CacheManager.UpdateUserPermissionsInCache(userId);
            CacheManager.UpdateUserAccessObjectsInCache(userId);
        }

        private void CreateFilterWithPermissions(FilterRole filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewRole,
                ObjectTypeId = ObjectTypes.Role
            });
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
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
                    AccessibleObjects = checkPartnerPermission.AccessibleObjects,
                    HaveAccessForAllObjects = checkPartnerPermission.HaveAccessForAllObjects,
                    Filter = x => !x.PartnerId.HasValue || checkPartnerPermission.AccessibleObjects.Contains(x.PartnerId.Value)
                }
            };
        }
    }
}