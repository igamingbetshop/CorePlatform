using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.User;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.DAL.Models.Clients;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.BLL.Models;
using log4net;
using IqSoft.CP.Common.Models.AgentModels;
using Microsoft.EntityFrameworkCore;

namespace IqSoft.CP.BLL.Services
{
    public class UserBll : PermissionBll, IUserBll
    {
        #region Constructors

        public UserBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public UserBll(BaseBll baseBl)
            : base(baseBl)
        {

        }
        
        #endregion

        public SessionIdentity LoginUser(LoginUserInput loginInput, out string imageData)
        {
            CheckCaptcha(loginInput.PartnerId, loginInput.ReCaptcha);
            var currentTime = GetServerDate();
            var user = Db.Users.FirstOrDefault(x => ((x.UserName == loginInput.UserName && ((x.LoginByNickName.HasValue && !x.LoginByNickName.Value) || !x.LoginByNickName.HasValue)) ||
            (x.LoginByNickName.HasValue && x.LoginByNickName.Value && x.NickName == loginInput.UserName)) && x.PartnerId == loginInput.PartnerId &&
            (x.Type == loginInput.UserType || (loginInput.UserType == (int)UserTypes.Agent && (x.Type == (int)UserTypes.MasterAgent ||
            x.Type == (int)UserTypes.AgentEmployee || x.Type == (int)UserTypes.AdminUser))));

            if (user == null)
                throw CreateException(loginInput.LanguageId, Constants.Errors.WrongLoginParameters);
            var userState = user.State;
            var parentState = CacheManager.GetUserSetting(user.Id)?.ParentState;
            if (parentState.HasValue && CustomHelper.Greater((UserStates)parentState.Value, (UserStates)user.State))
                userState = parentState.Value;
            if (userState == (int)UserStates.ForceBlock || userState == (int)UserStates.ForceBlockBySecurityCode )
                throw CreateException(loginInput.LanguageId, Constants.Errors.UserForceBlocked);       

            var passwordHash = CommonFunctions.ComputeUserPasswordHash(loginInput.Password, user.Salt);
            if (user.PasswordHash != passwordHash)
            {
                var partnerSetting = CacheManager.GetConfigParameters(loginInput.PartnerId, Constants.PartnerKeys.AllowedFaildLoginCount).FirstOrDefault(x => x.Key == "User");
                if (!partnerSetting.Equals(default(KeyValuePair<string, string>)) && int.TryParse(partnerSetting.Value, out int allowedNumber))
                {
                    var count = CacheManager.UpdateUserFailedLoginCount(user.Id);
                    if (count > allowedNumber)
                    {
                        BlockUserForce(user.Id);
                        throw CreateException(loginInput.LanguageId, Constants.Errors.UserForceBlocked);
                    }
                }
                throw CreateException(loginInput.LanguageId, Constants.Errors.WrongLoginParameters);
            }
            if (userState == (int)UserStates.Disabled || userState == (int)UserStates.Closed || userState == (int)UserStates.InactivityClosed)
                throw CreateException(loginInput.LanguageId, Constants.Errors.UserBlocked);

            Db.UserSessions.Where(x => x.UserId == user.Id && x.State == (int)SessionStates.Active).
                UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentTime, LogoutType = (int)LogoutTypes.MultipleDevice });
            var userSession = new UserSession
            {
                UserId = user.Id,
                LanguageId = loginInput.LanguageId ?? Constants.Languages.English,
                Ip = loginInput.Ip,
                CashDeskId = loginInput.CashDeskId,
                State = user.IsTwoFactorEnabled ? (int)SessionStates.Pending : (int)SessionStates.Active
            };
            var newSession = AddUserSession(userSession);
            user.UserSessions = new List<UserSession> { newSession };
            Db.SaveChanges();
            if (user.ImageData != null)
                imageData = Convert.ToBase64String(user.ImageData, 0, user.ImageData.Length);
            else imageData = string.Empty;
            bool resetPassword = false;
            var userPasswordExpirySetting = CacheManager.GetConfigKey(user.PartnerId, Constants.PartnerKeys.UserPasswordExpiryPeriod);
            if (!string.IsNullOrEmpty(userPasswordExpirySetting) && int.TryParse(userPasswordExpirySetting, out int userPasswordExpiryPeriod) &&
               (user.PasswordChangedDate == null || (currentTime - user.PasswordChangedDate).Value.TotalDays > userPasswordExpiryPeriod))
                resetPassword = true;
            var userSetting = CacheManager.GetUserSetting(user.Id);
            var newIdentity = new SessionIdentity
            {
                PartnerId = user.PartnerId,
                LoginIp = loginInput.Ip,
                LanguageId = loginInput.LanguageId ?? Constants.Languages.English,
                SessionId = newSession.Id,
                Id = user.Id,
                Token = newSession.Token,
                CurrencyId = user.CurrencyId,
                IsAdminUser = loginInput.UserType == (int)UserTypes.AdminUser,
                OddsType = userSetting?.OddsType,
                RequiredParameters = new
                {
                    ResetPassword = user.CreationTime == user.PasswordChangedDate || resetPassword,
                    ResetSecurityCode = string.IsNullOrEmpty(user.SecurityCode) ||user.CreationTime == user.PasswordChangedDate,
                    ResetNickName = (string.IsNullOrEmpty(user.NickName) || !user.LoginByNickName.HasValue)
                }
            };
            CacheManager.RemoveUserFailedLoginCountFromCache(user.Id);
            return newIdentity;
        }

        public void LogoutUser(string token)
        {
            var userSession = Db.UserSessions.FirstOrDefault(x => x.Token == token);
            if (userSession == null)
                throw CreateException(LanguageId, Constants.Errors.SessionNotFound);
            userSession.State = (int)SessionStates.Inactive;
            userSession.EndTime = GetServerDate();
            userSession.LogoutType = (int)LogoutTypes.Manual;
            Db.SaveChanges();
        }
        private void CheckCaptcha(int partnerId, string reCaptcha)
        {
            if (CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.AdminCaptchaEnabled) == "1")
            {
                var captchaResponse = CaptchaHelpers.CallCaptchaApi(reCaptcha, new SessionIdentity { LanguageId = Identity.LanguageId, PartnerId = partnerId });
                if (!captchaResponse.Success)
                {
                    Log.Info(JsonConvert.SerializeObject(captchaResponse));
                    throw CreateException(Identity.LanguageId, Constants.Errors.InvalidSecretKey);
                }
            }
        }
        public void BlockUserForce(int userId, int blockState = (int)UserStates.ForceBlock)
        {
            var currentTime = DateTime.UtcNow;
            var action = new ObjectAction
            {
                ObjectId = userId,
                ObjectTypeId = (int)ObjectTypes.User,
                Type = (int)ObjectActionTypes.BlockUserForce,
                State = (int)BaseStates.Active,
                StartTime = currentTime,
                FinishTime = currentTime.AddHours(1)
            };
            var dbAction = Db.ObjectActions.Where(x => x.ObjectId == userId && x.ObjectTypeId == (int)ObjectTypes.User &&
            x.Type == (int)ObjectActionTypes.BlockUserForce && x.State == (int)BaseStates.Active).FirstOrDefault();
            if (dbAction == null)
                Db.ObjectActions.Add(action);
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == userId);
            dbUser.State = blockState;
            dbUser.LastUpdateTime = currentTime;
            Db.SaveChanges();
            CacheManager.RemoveUserFromCache(userId);
        }

        public void VerifyTwoFactorPin(string pin)
        {
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == Identity.Id);
            if (dbUser.IsTwoFactorEnabled)
            {
                var res = CommonFunctions.GeteratePin(dbUser.QRCode);
                if (res != pin)
                    throw CreateException(Identity.LanguageId, Constants.Errors.InvalidTwoFactorKey);
            }
        }

        public UserSession UpdateSessionState(string token, int state)
        {
            if (!Enum.IsDefined(typeof(SessionStates), state))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var dbSession = Db.UserSessions.FirstOrDefault(x => x.Token == token);
            if (dbSession == null)
                throw CreateException(LanguageId, Constants.Errors.SessionNotFound);
            dbSession.State = state;
            Db.SaveChanges();
            return dbSession;
        }

        public void EnableTwoFactor(string code, string pin)
        {
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == Identity.Id);
            if (dbUser.IsTwoFactorEnabled)
                throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
            var res = CommonFunctions.GeteratePin(code);
            if (res != pin)
                throw CreateException(Identity.LanguageId, Constants.Errors.InvalidTwoFactorKey);
            dbUser.IsTwoFactorEnabled = true;
            dbUser.QRCode = code;
        }

        public void DisableTwoFactor(string pin)
        {
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == Identity.Id);
            var res = CommonFunctions.GeteratePin(dbUser.QRCode);
            if (res != pin)
                throw CreateException(Identity.LanguageId, Constants.Errors.InvalidTwoFactorKey);
            dbUser.IsTwoFactorEnabled = false;
            dbUser.QRCode = string.Empty;
        }

        public UserSession GetUserSession(string token, bool checkExpiration = true, bool extendSession = true)
        {
            var userSession = Db.UserSessions.FirstOrDefault(x => x.Token == token);
            if (userSession == null)
                throw CreateException(LanguageId, Constants.Errors.SessionNotFound);
            if (userSession.State != (int)SessionStates.Active && checkExpiration)
                throw CreateException(LanguageId, Constants.Errors.SessionExpired, userSession.CashDeskId, integerInfo: userSession.LogoutType);
            if (extendSession && userSession.State == (int)SessionStates.Active)
                userSession.LastUpdateTime = GetServerDate();

            Db.SaveChanges();
            return userSession;
        }

        public UserSession GetUserSessionById(long id)
        {
            return Db.UserSessions.FirstOrDefault(x => x.Id == id);
        }

        public UserSession CreateProductSession(SessionIdentity session, int productId)
        {
            var newSession = new UserSession
            {
                UserId = session.Id,
                LanguageId = session.LanguageId,
                Ip = session.LoginIp,
                ProductId = productId,
                CashDeskId = session.CashDeskId,
                ParentId = session.SessionId
            };
            newSession = AddUserSession(newSession);
            Db.SaveChanges();
            return newSession;
        }

        public UserSession RefreshUserSession(string token)
        {
            var oldSession = GetUserSession(token);
            oldSession.State = (int)SessionStates.Inactive;
            oldSession.EndTime = GetServerDate();
            oldSession.LogoutType = (int)LogoutTypes.System;
            var newSession = new UserSession
            {
                UserId = oldSession.UserId,
                LanguageId = oldSession.LanguageId,
                Ip = oldSession.Ip,
                ProductId = oldSession.ProductId,
                CashDeskId = oldSession.CashDeskId,
                ParentId = oldSession.ParentId
            };
            var savedSession = AddUserSession(newSession);
            Db.SaveChanges();
            return savedSession;
        }

        public UserSession GetUserProductSession(int sessionId, int productId)
        {
            return Db.UserSessions.FirstOrDefault(x => x.ParentId == sessionId && x.ProductId == productId && x.State == (int)SessionStates.Active);
        }

        public UserSetting SaveUserSettings(UserSetting userSetting, out List<int> changedClientIds)
        {
            var currentDate = GetServerDate();
            var dbUserSettings = Db.UserSettings.FirstOrDefault(x => x.UserId == userSetting.UserId);
            userSetting.LastUpdateTime = currentDate;
            changedClientIds = new List<int>();
            if (dbUserSettings == null)
            {
                userSetting.CreationTime = currentDate;
                Db.UserSettings.Add(userSetting);
                Db.SaveChanges();
            }
            else
            {
                userSetting.Id = dbUserSettings.Id;
                Db.Entry(dbUserSettings).CurrentValues.SetValues(userSetting);
                Db.SaveChanges();
                CacheManager.RemoveUserSetting(userSetting.UserId);
                if (!userSetting.AllowDoubleCommission || (userSetting.AllowAutoPT.HasValue && !userSetting.AllowAutoPT.Value) ||
                    !userSetting.AllowOutright || !string.IsNullOrEmpty(userSetting.CalculationPeriod) )
                {
                    var userIds = Db.Users.Where(x => x.Path.Contains("/" + userSetting.UserId.ToString() + "/") && x.Id != userSetting.UserId).Select(x => x.Id).ToList();
                    var subAgents = Db.UserSettings.Where(x => userIds.Contains(x.UserId)).ToList();
                    var clientSetting = Db.ClientSettings.Where(x => x.Client.UserId.HasValue && (userIds.Contains(x.Client.UserId.Value) || x.Client.UserId == userSetting.UserId)).ToList();
                    var clientIds = new List<int>();
                    if (!userSetting.AllowDoubleCommission)
                    {
                        subAgents.ForEach(x => x.AllowDoubleCommission = false);
                        var val = Convert.ToDecimal(userSetting.AllowDoubleCommission);
                        clientSetting.Where(y => y.Name == nameof(userSetting.AllowDoubleCommission)).ToList()
                                     .ForEach(x =>
                                     {
                                         x.NumericValue = val;
                                         x.LastUpdateTime = currentDate;
                                         clientIds.Add(x.ClientId);
                                     });
                    }
                    if (userIds.Any())
                    {
                        if (userSetting.AllowAutoPT.HasValue && !userSetting.AllowAutoPT.Value)
                            subAgents.ForEach(x => x.AllowAutoPT = false);
                        if(userSetting.CalculationPeriod == "[-1]")
                            subAgents.ForEach(x => x.CalculationPeriod = "[-1]");
                        if (!userSetting.AllowOutright)
                        {
                            var val = userSetting.AllowOutright ? 1 : 0;
                            subAgents.ForEach(x => x.AllowOutright = false);
                            clientSetting.Where(y => y.Name == nameof(userSetting.AllowOutright)).ToList()
                                         .ForEach(x =>
                                         {
                                             x.NumericValue = val;
                                             x.LastUpdateTime = currentDate;
                                             if (!clientIds.Contains(x.ClientId))
                                                 clientIds.Add(x.ClientId);
                                         });
                        }

                        Db.SaveChanges();
                        foreach (var id in userIds)
                            CacheManager.RemoveUserSetting(id);
                        foreach (var id in clientIds)
                        {
                            CacheManager.RemoveClientSetting(id, "AllowDoubleCommission");
                            CacheManager.RemoveClientSetting(id, "AllowOutright");
                        }
                        changedClientIds = clientIds;
                    }
                }
            }
            return userSetting;
        }

        public User AddUser(User user, bool checkPermission = true, AgentEmployeePermissionModel permission = null)
        {
            if (checkPermission)
            {
                var checkResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateUser,
                    ObjectTypeId = (int)ObjectTypes.User,
                    ObjectId = user.Id
                });
                if (!checkResult.HaveAccessForAllObjects && checkResult.AccessibleObjects.All(x => x != user.Id))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var currentTime = GetServerDate();
            var userSalt = new Random().Next();
            var parentUser = CacheManager.GetUserById(Identity.Id);
            user.ParentId = Identity.Id;
            if (user.Type >= (int)UserTypes.MasterAgent)
            {
                var parentLevel = user.Type == (int)UserTypes.MasterAgent ? 0 : parentUser.Level;
                if (user.Type != (int)UserTypes.AgentEmployee && (parentLevel >= user.Level || parentLevel >= (int)AgentLevels.Agent))
                    throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                {
                    CheckGeneratableUsername(parentUser, user.Level.Value, user.UserName);
                    var startLetter = (user.UserName.Length == 4 || user.UserName.Length == 6) ? user.UserName[0].ToString() : string.Empty;
                    var prefix = GenerateUserNamePrefix(parentUser, user.Level.Value, user.Type);
                    if (user.UserName.Length == 6)
                    {
                        if (prefix.Length == 1 || prefix.Length % 2 == 1 )
                            user.UserName = user.UserName.Insert(4, prefix.Remove(0, 1));
                        else user.UserName = user.UserName.Insert(4, prefix);
                    }
                    else
                        user.UserName = startLetter + prefix +
                            (user.UserName.Length != 4 ? user.UserName : user.UserName.Substring(1, user.UserName.Length - 1));
                }
            }
            VerifyUserFields(user);
            user.IsTwoFactorEnabled = false;
            user.CreationTime = currentTime;
            user.LoginByNickName = false;
            user.Salt = userSalt;
            user.LastUpdateTime = currentTime;
            user.SessionId = SessionId;
            user.LanguageId = Identity.LanguageId;
     
            user.PasswordHash = CommonFunctions.ComputeUserPasswordHash(user.Password, userSalt);
            user.PasswordChangedDate = currentTime;
            Db.Users.Add(user);
            Db.SaveChanges();
            if (user.Type >= (int)UserTypes.MasterAgent)
                user.Path = string.IsNullOrEmpty(parentUser.Path) ? ("/" + user.Id + "/") : (parentUser.Path + user.Id + "/");
            else
                user.Path = "/" + user.Id + "/";
            AddUserRole(user.PartnerId, user.Id, user.Type, permission);
            if (user.PartnerId != Constants.MainPartnerId)
            {
                Db.AccessObjects.Add(new AccessObject
                {
                    ObjectTypeId = (int)ObjectTypes.Partner,
                    ObjectId = user.PartnerId,
                    UserId = user.Id,
                    PermissionId = Constants.Permissions.ViewPartner
                });
            }
            Db.SaveChanges();
            var account = new Account
            {
                ObjectId = user.Id,
                ObjectTypeId = (int)ObjectTypes.User,
                TypeId = (int)AccountTypes.UserBalance,
                Balance = 0,
                CurrencyId = user.CurrencyId,
                SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                CreationTime = currentTime,
                LastUpdateTime = currentTime
            };
            Db.Accounts.Add(account);
            Db.SaveChanges();
            return user;
        }

        private void AddUserRole(int partnerId, int userId, int userType, AgentEmployeePermissionModel permission = null)
        {
            Role role;
            switch (userType)
            {
                case (int)UserTypes.AgentEmployee:
                    role = GetRoleByName(string.Format("{0}_{1}", UserTypes.AgentEmployee.ToString(),
                                                                ((AgentEmployeePermissions)permission.MemberInformationPermission).ToString()));
                    if (role != null)
                    {
                        Db.UserRoles.Add(new UserRole
                        {
                            UserId = userId,
                            RoleId = role.Id
                        });
                    }
                    if (permission.ViewLog)
                    {
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewReportByUserLog
                        });
                    }
                    if (permission.ViewTransfer)
                    {
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewReportByTransaction
                        });
                    }
                    if (permission.ViewBetsLists)
                    {
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewInternetBets//??
                        });
                    }
                    if (permission.ViewReport)
                    {
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewUserReport
                        });
                    }
                    if (permission.ViewBetsAndForecast)
                    {
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewInternetBets
                        });
                    }
                    break;
                case (int)UserTypes.MasterAgent:
                case (int)UserTypes.Agent:
                    role = GetRoleByName(UserTypes.Agent.ToString());
                    if (role != null)
                    {
                        Db.UserRoles.Add(new UserRole
                        {
                            UserId = userId,
                            RoleId = role.Id
                        });
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Role,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewRole
                        });
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.User,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.CreateUserRole
                        });
                    }
                    break;
                case (int)UserTypes.Cashier:
                    role = GetRoleByName(UserTypes.Cashier.ToString());
                    if (role != null)
                    {
                        Db.UserRoles.Add(new UserRole
                        {
                            UserId = userId,
                            RoleId = role.Id
                        });
                        Db.AccessObjects.Add(new AccessObject
                        {
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            ObjectId = partnerId,
                            UserId = userId,
                            PermissionId = Constants.Permissions.ViewPartner
                        });
                    }
                    break;
                default: break;
            }
        }

        private void CheckGeneratableUsername(BllUser parentUser, int level, string userName)
        {
            var parentLevel = parentUser.Level;
            var parentParentUser = CacheManager.GetUserById(parentUser.ParentId.Value);
            var startLetter = userName[0];
            if (((level == (int)AgentLevels.Company && userName.Length != 3) ||
                 (parentLevel == (int)AgentLevels.Company &&
                ((level == (int)AgentLevels.SMA && (userName.Length != 4 || !(new char[] { '1', '2', '3' }).Contains(startLetter))) ||
                 (level == (int)AgentLevels.MA && (userName.Length != 6 || !(new char[] { '4', '5', '6' }).Contains(startLetter))) ||
                 (level == (int)AgentLevels.Agent && (userName.Length != 6 || !(new char[] { '7', '8', '9' }).Contains(startLetter))
                )))) ||
               ((level == (int)AgentLevels.Partner && userName.Length != 2) ||
                (parentLevel == (int)AgentLevels.Partner && ((level == (int)AgentLevels.SSMA && userName.Length != 2) ||
                 (level == (int)AgentLevels.SMA && (userName.Length != 4 || !(new char[] { 'A', 'B', 'D' }).Contains(startLetter))) ||
                 (level == (int)AgentLevels.MA && (userName.Length != 6 || !(new char[] { 'E', 'F', 'G' }).Contains(startLetter))) ||
                 (level == (int)AgentLevels.Agent && (userName.Length != 6 || !(new char[] { 'H', 'I', 'J' }).Contains(startLetter))
               )))) ||
               ((level == (int)AgentLevels.SSMA && userName.Length != 2) ||
                (parentLevel == (int)AgentLevels.SSMA && (
                (level == (int)AgentLevels.SMA && (/*userName.Length != 4 ||*/
                  ((parentParentUser.Level == (int)AgentLevels.Partner && !(new char[] { 'L', 'K' }).Contains(startLetter))) ||
                (level == (int)AgentLevels.MA && (userName.Length != 6 ||
                  ((parentParentUser.Level == (int)AgentLevels.Partner && !(new char[] { 'N', 'O' }).Contains(startLetter)))
                  )) ||
                    (level == (int)AgentLevels.Agent && (userName.Length != 6 ||
                  ((parentParentUser.Level == (int)AgentLevels.Partner && !(new char[] { 'Q', 'R' }).Contains(startLetter)))
                  ))
              )
              )))))
                throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            var un = userName[0] == '0' ? userName : userName.Substring(1, userName.Length - 1);
            if (un == "00" || un == "000" || un == "00000")
                throw CreateException(LanguageId, Constants.Errors.UserNameMustContainCharacter);
        }

        public static string GenerateUserNamePrefix(BllUser parentUser, int level, int type)
        {
            if (type == (int)UserTypes.AgentEmployee)
                return string.Format("{0}Sub", parentUser.UserName);
            var parentLevel = parentUser.Level ?? 0;
            var parentParentLevel = parentUser.ParentId.HasValue ? CacheManager.GetUserById(parentUser.ParentId.Value).Level : 0;
            var parentUserName = parentUser.Type == (int)UserTypes.AdminUser ? string.Empty : parentUser.UserName;
            var format = "{0}";
            var count = 0;
            switch (level)
            {
                case (int)AgentLevels.Company:
                    format = "C{0}";
                    break;
                case (int)AgentLevels.SMA:
                    {
                        parentUserName = string.Empty;
                        switch (parentLevel)
                        {
                            case (int)AgentLevels.Company:
                                format = "{0}";
                                break;
                            case (int)AgentLevels.Partner:
                                format = "{0}";
                                parentUserName = string.Empty;
                                break;
                            default:
                                if (parentParentLevel == (int)AgentLevels.Company)
                                    format = "M{0}";
                                else
                                    format = "{0}";
                                break;
                        }
                        break;
                    }
                case (int)AgentLevels.MA:
                    {
                        switch (parentLevel)
                        {
                            case (int)AgentLevels.Company:
                                format = "{0}";
                                parentUserName = string.Empty;
                                break;
                            case (int)AgentLevels.Partner:
                                format = "{0}";
                                parentUserName = string.Empty;
                                break;
                            case (int)AgentLevels.SSMA:
                                parentUserName = string.Empty;
                                if (parentParentLevel == (int)AgentLevels.Company)
                                    format = "P{0}";
                                else format = "{0}";
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                case (int)AgentLevels.Agent:
                    {
                        switch (parentLevel)
                        {
                            case (int)AgentLevels.Company:
                                format = "{0}";
                                parentUserName = "00";
                                break;
                            case (int)AgentLevels.Partner:
                                format = "{0}";
                                parentUserName = "00";
                                break;
                            case (int)AgentLevels.SSMA:
                                parentUserName = string.Empty;
                                if (parentParentLevel == (int)AgentLevels.Company)
                                    format = "S{0}";
                                else
                                    format = "{0}";
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                case (int)AgentLevels.Member:
                    switch (parentLevel)
                    {
                        case (int)AgentLevels.Company:
                        case (int)AgentLevels.SMA:
                            count = 3;
                            break;
                        case (int)AgentLevels.Partner:
                        case (int)AgentLevels.MA:
                            count = 2;
                            break;
                        case (int)AgentLevels.SSMA:
                            count = 1;
                            break;
                    }
                    break;
                default:
                    break;
            };
            var interimPart = string.Empty;
            if (count == 0)
                count = level - parentLevel;
            if ((parentLevel == (int)AgentLevels.Company || parentLevel == (int)AgentLevels.Partner) && (level == (int)AgentLevels.MA || level == (int)AgentLevels.Agent))
                count = 0;
            else if (level >= (int)AgentLevels.SMA && level != (int)AgentLevels.Member)
            {
                count = level - (int)AgentLevels.SMA;
                if (parentLevel == (int)AgentLevels.Partner || parentLevel == (int)AgentLevels.MA)
                    --count;
            }
            for (int i = 1; i < count; ++i)
                interimPart += "00";
            return string.Format(format + "{1}", parentUserName, interimPart);
        }

        public List<char> FindAvailableUserName(int type, int level, char startsWith)
        {
            var parentUser = CacheManager.GetUserById(Identity.Id);
            var parentLevel = type == (int)UserTypes.MasterAgent ? 0 : parentUser.Level;
            var subPerfix = type == (int)UserTypes.AgentEmployee ? "Sub" : string.Empty;
            var len = 2;
            if (level == (int)AgentLevels.Company || level == (int)AgentLevels.SMA || level == (int)AgentLevels.Member ||
                (parentLevel == (int)AgentLevels.SMA && (level == (int)AgentLevels.MA || level == (int)AgentLevels.Agent)))
                len = 3;
            else if ((parentUser.Level == (int)AgentLevels.Company || parentUser.Level == (int)AgentLevels.Partner || parentUser.Level == (int)AgentLevels.SSMA) &&
                     (level == (int)AgentLevels.MA || level == (int)AgentLevels.Agent))
                len = 5;
            var subUserName = (startsWith != '\0' ? startsWith.ToString() : string.Empty) + GenerateUserNamePrefix(parentUser, level, type);
            var availableValue = Enumerable.Repeat('0', len).ToArray();
            availableValue[len - 1] = '1';
            List<string> existingUsernames;
            if (level != (int)AgentLevels.Member)
                existingUsernames = Db.Users.Where(x => x.PartnerId == parentUser.PartnerId && x.UserName.StartsWith(subUserName) && x.Level == level).
                    Select(y => y.UserName.Substring(subUserName.Length)).ToList();
            else
                existingUsernames = Db.Clients.Where(x => x.PartnerId == parentUser.PartnerId && x.UserId == parentUser.Id && x.UserName.StartsWith(subUserName)).
                    Select(y => y.UserName.Substring(subUserName.Length)).ToList();

            while (true)
            {
                if (!existingUsernames.Contains(new string(availableValue)))
                    return availableValue.ToList();

                var c = GetNextSymbol(availableValue[len - 1]);
                if (c <= 'Z')
                    availableValue[len - 1] = c;
                else
                {
                    availableValue[len - 1] = '0';
                    var c1 = GetNextSymbol(availableValue[len - 2]);
                    if (c1 <= 'Z')
                        availableValue[len - 2] = c1;
                    else if (len == 3)
                    {
                        availableValue[len - 2] = '0';
                        var c2 = GetNextSymbol(availableValue[len - 3]);
                        if (c2 <= 'Z')
                            availableValue[len - 3] = c2;
                        else
                            throw CreateException(LanguageId, Constants.Errors.UserNameExists);
                    }
                    else if (len == 5)
                    {
                        availableValue[len - 3] = '0';
                        var c3 = GetNextSymbol(availableValue[len - 4]);
                        if (c3 <= 'Z')
                            availableValue[len - 4] = c3;
                        else
                        {
                            availableValue[len - 4] = '0';
                            var c4 = GetNextSymbol(availableValue[len - 4]);
                            if (c4 <= 'Z')
                                availableValue[len - 5] = c4;
                            else
                                throw CreateException(LanguageId, Constants.Errors.UserNameExists);
                        }
                    }
                    else
                        throw CreateException(LanguageId, Constants.Errors.UserNameExists);
                }
            }
        }

        private char GetNextSymbol(char s)
        {
            var c = (char)(s + 1);
            if (c > '9' && c < 'A')
                return 'A';
            else return c;
        }

        public List<AgentSubAccount> GetSubAccounts(int userId)
        {
            var result = new List<AgentSubAccount>();
            var subAccounts = (from us in Db.Users
                               join ao in Db.AccessObjects on us.Id equals ao.UserId into iao
                               join ur in Db.UserRoles.Where(x => x.Role.Name.Contains(UserTypes.AgentEmployee.ToString())) on us.Id equals ur.UserId into iur
                               from ao in iao.DefaultIfEmpty()
                               from ur in iur.DefaultIfEmpty()
                               where us.ParentId == userId && us.Id != userId && us.Type == (int)UserTypes.AgentEmployee
                               select new
                               {
                                   us.Id,
                                   us.UserName,
                                   us.NickName,
                                   us.FirstName,
                                   us.LastName,
                                   us.CreationTime,
                                   memberInfoPermission = ur.Role.Name.Replace(UserTypes.AgentEmployee.ToString() + "_", string.Empty),
                                   ao.PermissionId
                               }).ToList().GroupBy(x => x.Id).ToList();
            foreach (var sa in subAccounts)
            {
                var item = sa.FirstOrDefault();
                var permissionList = sa.Select(x => x.PermissionId).ToList();
                var membInfoPermission = item.memberInfoPermission == null ? (int)AgentEmployeePermissions.None :
                    (int)Enum.Parse(typeof(AgentEmployeePermissions), item.memberInfoPermission);
                result.Add(new AgentSubAccount
                {
                    Id = item.Id,
                    Username = item.UserName,
                    Nickname = item.NickName,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    CreationTime = item.CreationTime.GetGMTDateFromUTC(Identity.TimeZone),
                    MemberInformationPermission = membInfoPermission,
                    ViewBetsAndForecast = permissionList.Contains(Constants.Permissions.ViewInternetBets),
                    ViewBetsLists = permissionList.Contains(Constants.Permissions.ViewInternetBets),
                    ViewReport = permissionList.Contains(Constants.Permissions.ViewUserReport),
                    ViewTransfer = permissionList.Contains(Constants.Permissions.ViewReportByTransaction),
                    ViewLog = permissionList.Contains(Constants.Permissions.ViewReportByUserLog)
                });
            }
            return result;
        }


        public AgentEmployeePermissionModel GetSubAccountPermissions(int userId)
        {
            var membInfoPermission = (int)AgentEmployeePermissions.None;
            var role = Db.UserRoles.Include(x => x.Role).FirstOrDefault(x => x.Role.Name.Contains(UserTypes.AgentEmployee.ToString()) && x.UserId == userId);
            if (role != null)
                membInfoPermission = (int)Enum.Parse(typeof(AgentEmployeePermissions), role.Role.Name.Replace(UserTypes.AgentEmployee.ToString() + "_", string.Empty));
            var accessList = Db.AccessObjects.Where(x => x.UserId == userId && (
            x.PermissionId == Constants.Permissions.ViewInternetBets ||
            x.PermissionId == Constants.Permissions.ViewUserReport ||
            x.PermissionId == Constants.Permissions.ViewReportByTransaction ||
            x.PermissionId == Constants.Permissions.ViewReportByUserLog
            )).Select(x => x.PermissionId);
            return new AgentEmployeePermissionModel
            {
                MemberInformationPermission = membInfoPermission,
                ViewBetsAndForecast = accessList.Contains(Constants.Permissions.ViewInternetBets),
                ViewBetsLists = accessList.Contains(Constants.Permissions.ViewInternetBets),
                ViewReport = accessList.Contains(Constants.Permissions.ViewUserReport),
                ViewTransfer = accessList.Contains(Constants.Permissions.ViewReportByTransaction),
                ViewLog = accessList.Contains(Constants.Permissions.ViewReportByUserLog)
            };
        }
        public void UpdateUserImage(int userId, byte[] imageData)
        {
            Db.Users.Where(x => x.Id == userId).UpdateFromQuery(x => new User { ImageData = imageData });
        }

        public User EditUser(User user, bool checkPermission, AgentEmployeePermissionModel permission = null)
        {
            if (checkPermission)
            {
                var checkResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateUser,
                    ObjectTypeId = (int)ObjectTypes.User,
                    ObjectId = user.Id
                });
                if (!checkResult.HaveAccessForAllObjects && checkResult.AccessibleObjects.All(x => x != user.Id))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var currentTime = GetServerDate();
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == user.Id);
            if (dbUser == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            if (dbUser.Type == (int)UserTypes.MasterAgent || dbUser.Type == (int)UserTypes.AgentEmployee)
            {
                if (dbUser.CurrencyId != user.CurrencyId)
                    throw CreateException(LanguageId, Constants.Errors.WrongCurrencyId);
                if (dbUser.PartnerId != user.PartnerId)
                    throw CreateException(LanguageId, Constants.Errors.WrongPartnerId);
                if (dbUser.UserName != user.UserName)
                    throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            }
            var oldValue = JsonConvert.SerializeObject(dbUser.ToUserInfo());
            user.PasswordHash = dbUser.PasswordHash;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                var passwordReqex = GetUserPasswordRegex(dbUser.PartnerId, dbUser.Type);
                if (!Regex.IsMatch(user.Password, passwordReqex))
                    throw CreateException(LanguageId, Constants.Errors.InvalidPassword);
                var passwordHash = CommonFunctions.ComputeUserPasswordHash(user.Password, dbUser.Salt);
                if (passwordHash != dbUser.PasswordHash)
                {
                    if (checkPermission)
                    {
                        var checkChangePasss = GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.ChangeUserPass
                        });
                        if (!checkChangePasss.HaveAccessForAllObjects)
                            throw CreateException(LanguageId, Constants.Errors.CanNotChangeUserPassword);
                    }
                    dbUser.PasswordHash = passwordHash;
                    dbUser.PasswordChangedDate = currentTime;
                }
            }
            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;
            dbUser.Email = user.Email;
            dbUser.MobileNumber = user.MobileNumber;
            dbUser.Phone = user.Phone;
            dbUser.Gender = user.Gender;
            dbUser.State = user.State;
            dbUser.Language = user.Language;
            dbUser.LastUpdateTime = currentTime;
            dbUser.OddsType = user.OddsType;
            if (dbUser.Type == (int)UserTypes.AdminUser)
                dbUser.CurrencyId = user.CurrencyId;
            if (Identity.Id == dbUser.Id && user.OddsType.HasValue && Enum.IsDefined(typeof(OddsTypes), user.OddsType))
            {
                var dbUserSettings = Db.UserSettings.FirstOrDefault(x => x.UserId == dbUser.Id);
                if (dbUserSettings == null)
                {
                    Db.UserSettings.Add(new UserSetting
                    {
                        UserId = dbUser.Id,
                        CalculationPeriod = JsonConvert.SerializeObject(new List<int> { 1 }),
                        LevelLimits = string.Empty,
                        CountLimits = string.Empty,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime,
                        OddsType = user.OddsType
                    });
                }
                else
                    dbUserSettings.OddsType = user.OddsType;
                Db.SaveChanges();
                dbUser.OddsType = user.OddsType;
                CacheManager.RemoveUserSetting(dbUser.Id);
            }
            SaveChangesWithHistory((int)ObjectTypes.User, user.Id, oldValue);
            if (permission != null)
            {
                if (!permission.ViewBetsAndForecast)
                    Db.AccessObjects.Where(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewInternetBets).DeleteFromQuery();
                else if (!Db.AccessObjects.Any(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewInternetBets))
                    Db.AccessObjects.Add(new AccessObject
                    {
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        ObjectId = user.PartnerId,
                        UserId = user.Id,
                        PermissionId = Constants.Permissions.ViewInternetBets
                    });
                if (!permission.ViewReport)
                    Db.AccessObjects.Where(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewUserReport).DeleteFromQuery();
                else if (!Db.AccessObjects.Any(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewUserReport))
                    Db.AccessObjects.Add(new AccessObject
                    {
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        ObjectId = user.PartnerId,
                        UserId = user.Id,
                        PermissionId = Constants.Permissions.ViewUserReport
                    });
                if (!permission.ViewBetsLists)
                    Db.AccessObjects.Where(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewInternetBets).DeleteFromQuery();
                else if (!Db.AccessObjects.Any(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewInternetBets))
                    Db.AccessObjects.Add(new AccessObject
                    {
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        ObjectId = user.PartnerId,
                        UserId = user.Id,
                        PermissionId = Constants.Permissions.ViewInternetBets
                    });
                if (!permission.ViewTransfer)
                    Db.AccessObjects.Where(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewReportByTransaction).DeleteFromQuery();
                else if (!Db.AccessObjects.Any(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewReportByTransaction))
                    Db.AccessObjects.Add(new AccessObject
                    {
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        ObjectId = user.PartnerId,
                        UserId = user.Id,
                        PermissionId = Constants.Permissions.ViewReportByTransaction
                    });
                if (!permission.ViewLog)
                    Db.AccessObjects.Where(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewReportByUserLog).DeleteFromQuery();
                else if (!Db.AccessObjects.Any(x => x.UserId == user.Id && x.PermissionId == Constants.Permissions.ViewReportByUserLog))
                    Db.AccessObjects.Add(new AccessObject
                    {
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        ObjectId = user.PartnerId,
                        UserId = user.Id,
                        PermissionId = Constants.Permissions.ViewReportByUserLog
                    });
            }
            Db.SaveChanges();
            return dbUser;
        }

        public void ChangeUserPassword(int userId, string oldPassword, string newPassword, bool isReset = false)
        {
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == userId);
            if (dbUser == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            var passwordReqex = GetUserPasswordRegex(dbUser.PartnerId, dbUser.Type);
            if (!Regex.IsMatch(newPassword, passwordReqex))
                throw CreateException(LanguageId, Constants.Errors.InvalidPassword);
            var newPasswordHash = CommonFunctions.ComputeUserPasswordHash(newPassword, dbUser.Salt);
            var oldPasswordHash = CommonFunctions.ComputeUserPasswordHash(oldPassword, dbUser.Salt);
            if (!isReset && oldPasswordHash != dbUser.PasswordHash)
                throw CreateException(LanguageId, Constants.Errors.WrongPassword);
            if (!isReset && newPasswordHash == oldPasswordHash)
                throw CreateException(LanguageId, Constants.Errors.PasswordMatches);
            var currentTime = GetServerDate();
            dbUser.PasswordHash = newPasswordHash;
            dbUser.LastUpdateTime = currentTime;
            if (!isReset)
                dbUser.PasswordChangedDate = currentTime;
            Db.SaveChanges();
        }

        public void ChangeUserNickName(int userId, string nickName, bool loginByNickName)
        {
            var pattern = "(?=^.{5,20}$)[a-zA-Z0-9]+$";
            var userNamePattern = "(^P[A-Z0-9]{3}$)|(^C[A-Z0-9]{3,10}$)|(^S[A-Z0-9]{3,10}$)|(^([A-OQR1-9]{1})([A-Z0-9]{10})$)";
            if (!Regex.IsMatch(nickName, pattern))
                throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == userId);
            if (dbUser == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            var partnerSetting = CacheManager.GetPartnerSettingByKey(dbUser.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
            if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0 &&
              (dbUser.UserName != nickName && (Regex.IsMatch(nickName, userNamePattern) ||
               nickName.Length == 6 || nickName.Length == 8 || nickName.Length == 11)))
                throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            if (Db.Users.Any(x => ((x.UserName == nickName && x.Id != userId) || x.NickName == nickName) && x.PartnerId == dbUser.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.NickNameExists);
            var currentTime = GetServerDate();
            dbUser.NickName = nickName;
            dbUser.LastUpdateTime = currentTime;
            dbUser.LoginByNickName = loginByNickName;
            Db.SaveChanges();
            CacheManager.RemoveUserFromCache(userId);
        }

        public void ChangeUserSecurityCode(int userId, string oldCode, string newCode, bool checkPermission, bool checkRegex, bool isReset)
        {
            if (checkPermission)
            {
                CheckPermission(Constants.Permissions.CreateApiKey);
                var checkUser = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateUser,
                    ObjectTypeId = (int)ObjectTypes.User
                });
                if (!checkUser.HaveAccessForAllObjects && checkUser.AccessibleObjects.All(x => x != userId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var dbUser = Db.Users.FirstOrDefault(x => x.Id == userId);
            if (dbUser == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            if (checkRegex)
            {
                var pattern = "(^[0-9]{6}$)";
                if (!Regex.IsMatch(newCode, pattern) ||
                    !newCode.Select((i, j) => Math.Abs(Convert.ToInt32(i) - Convert.ToInt32(j))).Distinct().Skip(1).Any() ||
                    newCode.Distinct().Count() == 1)
                    throw CreateException(LanguageId, Constants.Errors.InvalidSecurityCode);
            }
            if (!isReset && oldCode != dbUser.SecurityCode && !string.IsNullOrEmpty(dbUser.NickName))
                throw CreateException(LanguageId, Constants.Errors.WrongSecurityCode);
            dbUser.SecurityCode = newCode;
            dbUser.LastUpdateTime = GetServerDate();
            Db.SaveChanges();
            CacheManager.RemoveUserFromCache(userId);
        }

        public User GetUserById(int id)
        {
            return Db.Users.FirstOrDefault(x => x.Id == id);
        }

        public User GetUserByUserName(string userName)
        {
            return Db.Users.FirstOrDefault(x => x.UserName == userName);
        }

        public string GetUserPasswordRegex(int partnerId, int userType)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewUser);
            CheckPermission(Constants.Permissions.CreateUser);
            if ((!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner==null)
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);
            if (!Enum.IsDefined(typeof(UserTypes), userType))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            string passwordRegex;
            if (userType == (int)UserTypes.MasterAgent || userType == (int)UserTypes.Agent || userType == (int)UserTypes.AgentEmployee)
                passwordRegex = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.AgentPasswordRegex);
            else
                passwordRegex = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.UserPasswordRegex);
            return !string.IsNullOrEmpty(passwordRegex) ? passwordRegex : Constants.PasswordRegex;
        }
        public PagedModel<fnUser> GetUsersPagedModel(FilterfnUser filter, bool checkPermission)
        {
            if (checkPermission)
            {
                CreateFilterWithPermissions(filter);
            }
            Func<IQueryable<fnUser>, IOrderedQueryable<fnUser>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnUser>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnUser>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = user => user.OrderByDescending(x => x.Id);
            }

            return new PagedModel<fnUser>
            {
                Entities = filter.FilterObjects(Db.fn_User(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_User())
            };
        }

        public bool IsUserNameExists(int partnerId, string userName, int? level, char startWith, bool checkPermission)
        {
            if (checkPermission)
                CheckPermission(Constants.Permissions.CreateUser);
            var partnerSetting = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.IsUserNameGeneratable);
            if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
            {
                var parentUser = CacheManager.GetUserById(Identity.Id);
                try
                {
                    CheckGeneratableUsername(parentUser, level.Value, (startWith != '\0' ? startWith.ToString() : string.Empty) + userName);
                    userName = (startWith != '\0' ? startWith.ToString() : string.Empty) + GenerateUserNamePrefix(parentUser, level ?? 0, (int)UserTypes.MasterAgent) + userName;
                }
                catch
                {
                    return true;
                }
            }
            return Db.Users.Any(x => x.UserName == userName);
        }

        private void VerifyUserFields(User user)
        {
            var otherUser = Db.Users.FirstOrDefault(x => x.UserName == user.UserName || x.NickName == user.UserName);
            if (otherUser != null)
                throw CreateException(LanguageId, Constants.Errors.UserNameExists);
            var passwordRegex = string.Empty;
            if (user.Type == (int)UserTypes.Agent)
            {
                if (user.ParentId == null)
                    throw CreateException(LanguageId, Constants.Errors.WrongUserId);
                var parent = Db.Users.FirstOrDefault(x => x.Id == user.ParentId);
                if (parent == null)
                    throw CreateException(LanguageId, Constants.Errors.WrongUserId);
                user.Path = parent.Path;
                passwordRegex = CacheManager.GetConfigKey(user.PartnerId, Constants.PartnerKeys.AgentPasswordRegex);

            }
            else
                passwordRegex = CacheManager.GetConfigKey(user.PartnerId, Constants.PartnerKeys.UserPasswordRegex);

            if (!Regex.IsMatch(user.Password, !string.IsNullOrEmpty(passwordRegex) ? passwordRegex : Constants.PasswordRegex))
                throw CreateException(LanguageId, Constants.Errors.InvalidPassword);
        }

        private UserSession AddUserSession(UserSession session)
        {
            var currentTime = GetServerDate();
            session.StartTime = currentTime;
            session.LastUpdateTime = currentTime;
            session.State = session.State > 0 ? session.State : (int)SessionStates.Active;
            session.Token = Guid.NewGuid().ToString();
            Db.UserSessions.Add(session);
            return session;
        }

        private void CreateFilterWithPermissions(FilterfnUser filter)
        {
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

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnUser>>
            {
                new CheckPermissionOutput<fnUser>
                {
                    AccessibleObjects = userAccess.AccessibleObjects,
                    HaveAccessForAllObjects = userAccess.HaveAccessForAllObjects,
                    Filter = x => userAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                },
                new CheckPermissionOutput<fnUser>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
        }

        public string GetProjectToken(int projectTypeId, SessionIdentity identity)
        {
            var newSession = new UserSession
            {
                UserId = identity.Id,
                LanguageId = identity.LanguageId,
                Ip = identity.LoginIp,
                ProjectTypeId = projectTypeId
            };
            newSession = AddUserSession(newSession);
            Db.SaveChanges();
            return newSession.Token;
        }

        public string GetUserState()
        {
            var user = Db.Users.FirstOrDefault(x => x.Id == Identity.Id);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            return user.AdminState;
        }

        public void UpdateUserState(string state)
        {
            var user = Db.Users.FirstOrDefault(x => x.Id == Identity.Id);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            user.AdminState = state;
            Db.SaveChanges();
        }

        public List<fnAgent> GetSubAgents(int agentId, int? level, int? type, bool onlyDirectDownline, string agentIdentifier, int? id = null, bool? isFromSuspended = null)
        {
            var query = Db.fn_Agent(agentId).Where(x => x.Id != agentId);
            if (level.HasValue)
                query = query.Where(x => x.Level == level);
            if (type.HasValue)
                query = query.Where(x => x.Type == type);
            if (onlyDirectDownline)
                query = query.Where(x => x.ParentId == agentId);
            if (id.HasValue)
                query = query.Where(x => x.Id == id);
            if (!string.IsNullOrEmpty(agentIdentifier))
                query = query.Where(x => x.UserName.Contains(agentIdentifier) ||
                                        x.NickName.Contains(agentIdentifier) ||
                                        x.FirstName.Contains(agentIdentifier) ||
                                        x.LastName.Contains(agentIdentifier));
            var res = query.ToList();
            if (isFromSuspended.HasValue)
            {
                res.ForEach(x =>
                {
                    if (x.ParentState.HasValue)
                    {
                        var parents = x.Path.Split('/');
                        foreach (var sPId in parents)
                        {
                            if (int.TryParse(sPId, out int pId) && pId != agentId)
                            {
                                var p = CacheManager.GetUserById(Convert.ToInt32(pId));
                                if (CustomHelper.Greater((UserStates)p.State, (UserStates)x.ParentState))
                                    x.State = p.State;
                            }
                        }
                    }
                }
                );
                return res.Where(y => (isFromSuspended.Value && y.ParentState != null && y.ParentState.Value == (int)UserStates.Suspended && y.State != (int)UserStates.Suspended &&
                                               CustomHelper.Greater((UserStates)y.ParentState.Value, (UserStates)y.State)) ||
                                     (!isFromSuspended.Value && (y.ParentState == null || (y.ParentState.Value != (int)UserStates.Suspended || y.State == (int)UserStates.Suspended)))).ToList();
            }
            return res;
        }

        public List<Client> GetAgentMemebers(int userId, bool withDownlines)
        {
            if(withDownlines)
                return Db.Clients.Where(x => x.User.Path.Contains("/" + userId + "/")).ToList();
            return Db.Clients.Where(x => x.UserId == userId).ToList();
        }

        public List<AgentDownlineInfo> GetAgentDownline(int userId, bool takeAll, bool directOnly)
        {
            var agent = CacheManager.GetUserById(userId);
            if (agent == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            var qAgents = Db.Users.AsQueryable();
            if(directOnly)
                qAgents = qAgents.Where(x => x.ParentId == userId);
            else
                qAgents = qAgents.Where(x => x.Id != userId && x.Path.Contains("/" + userId + "/"));

            if (!takeAll)
                qAgents = qAgents.Where(x => x.State != (int)UserStates.Disabled);
            var levels = qAgents.GroupBy(x => x.Level).Select(x => 
                new AgentDownlineInfo { Level = x.Key.Value, Count = x.Count(), MaxCredit = 0 });
            foreach (var level in levels)
            {
                var query = Db.UserSettings.Where(x => x.User.ParentId == userId && x.User.Level == level.Level);
                if (query.Any())
                    level.MaxCredit = query.Max(x => x.AgentMaxCredit).Value;
            }

            var membersCount = Db.Clients.Count(x => x.UserId == userId);
            var mQuery = Db.ClientSettings.Where(x => x.Client.UserId == userId && x.Name == ClientSettings.MaxCredit);
            var membersMaxCredit = (mQuery.Any() ? mQuery.Max(x => x.NumericValue).Value : 0m);

            return BaseBll.GetEnumerations(Constants.EnumerationTypes.AgentLevels, LanguageId)
                   .Where(x => (x.Value > agent.Level && agent.Level > 0) || (x.Value == 1 && agent.Level == 0)).Select(x =>
                   {
                       var l = levels.FirstOrDefault(y => y.Level == x.Value);
                       return new AgentDownlineInfo
                       {
                           Level = x.Value,
                           Count = (x.Value == (int)AgentLevels.Member ? membersCount : (l != null ? l.Count : 0)),
                           MaxCredit = (x.Value == (int)AgentLevels.Member ? membersMaxCredit : (l != null ? l.MaxCredit : 0))
                       };
                   }).ToList();
        }

        public List<AgentDownlinesStatuses> GetAgentDownlineStatuses(int userId, bool checkSuspention)
        {
            var agent = CacheManager.GetUserById(userId);
            var levEnums = BaseBll.GetEnumerations(Constants.EnumerationTypes.AgentLevels, LanguageId);
            var agents = Db.UserSettings.Where(x => x.User.Id != userId && x.User.Path.Contains("/" + userId + "/")).GroupBy(x => x.User.Level)
                .Select(x => new
                {
                    Level = x.Key.Value,
                    Users = x.Select(y => new AgentStatusItem
                    {
                        State = y.User.State,
                        ParentState = y.ParentState,
                        Path = y.User.Path
                    }).ToList()
                }).ToList();
            var result = new List<AgentDownlinesStatuses>();
            agents.ForEach(x =>
            {
                x.Users.ForEach(z =>
                {
                    if (z.ParentState.HasValue)
                    {
                        var parents = z.Path.Split('/');
                        foreach (var sPId in parents)
                        {
                            if (int.TryParse(sPId, out int pId) && pId != userId)
                            {
                                var p = CacheManager.GetUserById(pId);
                                if (CustomHelper.Greater((UserStates)p.State, (UserStates)z.ParentState))
                                    z.State = p.State;
                            }
                        }
                    }
                });
                result.Add(new AgentDownlinesStatuses
                {
                    Id = x.Level,
                    Name = levEnums.Where(y => y.Value == x.Level).Select(y => y.Text).FirstOrDefault(),
                    Count = x.Users.Count(),
                    TotalActive = x.Users.Count(y => (y.State == (int)UserStates.Active && y.ParentState == null) ||
                                               (y.ParentState.HasValue && y.ParentState.Value == (int)UserStates.Active && !CustomHelper.Greater((UserStates)y.State, (UserStates)y.ParentState.Value))),
                    TotalSuspended = x.Users.Count(y => (checkSuspention && y.ParentState != null && y.ParentState.Value == (int)UserStates.Suspended && y.State != (int)UserStates.Suspended &&
                                                   CustomHelper.Greater((UserStates)y.ParentState.Value, (UserStates)y.State)) ||
                                                 (!checkSuspention && (y.ParentState == null || y.ParentState.Value != (int)UserStates.Suspended))),
                    TotalLocked = x.Users.Count(y => (y.State == (int)UserStates.ForceBlock || y.State == (int)UserStates.ForceBlockBySecurityCode) &&
                                               (y.ParentState == null || !CustomHelper.Greater((UserStates)y.ParentState.Value, (UserStates)y.State))),

                    TotalClosed = x.Users.Count(y => ((y.State == (int)UserStates.Closed || y.State == (int)UserStates.InactivityClosed) &&
                                               (y.ParentState == null || !CustomHelper.Greater((UserStates)y.ParentState.Value, (UserStates)y.State))) ||
                                               (y.ParentState != null && (y.ParentState.Value == (int)UserStates.Closed || y.ParentState.Value == (int)UserStates.InactivityClosed))),
                    TotalDisabled = x.Users.Count(y => (y.State == (int)UserStates.Disabled || (y.ParentState != null && y.ParentState.Value == (int)UserStates.Disabled))),
                    Unchanged = x.Users.Count() - x.Users.Count(y => y.ParentState != null && y.ParentState.Value == (int)UserStates.Suspended && y.State != (int)UserStates.Suspended &&
                                                  CustomHelper.Greater((UserStates)y.ParentState.Value, (UserStates)y.State))
                });
            });

            var levelText = levEnums.First(x => x.Value == (int)AgentLevels.Member).Text;
            var clients = Db.Clients.Where(x => x.User.Path.Contains("/" + userId + "/"))
                .Select(x => new AgentStatusItem
                {
                    State = x.State,
                    ClientParentState = x.ClientSettings.FirstOrDefault(y => y.ClientId == x.Id && y.Name == "ParentState"),
                    Path = x.User.Path
                }).ToList();



            if (clients.Count == 0)
                result.Add(new AgentDownlinesStatuses { Id = (int)AgentLevels.Member, Name = levelText });
            else
            {
                clients.ForEach(x =>
                {
                    if (x.ClientParentState != null && x.ClientParentState.NumericValue.HasValue)
                    {
                        var parents = x.Path.Split('/');

                        foreach (var sPId in parents)
                        {
                            if (int.TryParse(sPId, out int pId) && pId != userId)
                            {
                                var p = CacheManager.GetUserById(Convert.ToInt32(pId));
                                var st = CustomHelper.MapUserStateToClient[p.State];
                                if (CustomHelper.Greater((ClientStates)st, (ClientStates)x.ClientParentState.NumericValue))
                                    x.State = st;
                            }
                        }
                    }
                });

                result.Add(new AgentDownlinesStatuses
                {
                    Id = (int)AgentLevels.Member,
                    Name = levEnums.Where(y => y.Value == (int)AgentLevels.Member).Select(y => y.Text).FirstOrDefault(),
                    Count = clients.Count(),
                    TotalActive = clients.Count(y => (y.State == (int)ClientStates.Active && (y.ClientParentState == null || y.ClientParentState.NumericValue == null)) ||
                                               (y.ClientParentState != null && y.ClientParentState.NumericValue != null && y.ClientParentState.NumericValue == (int)ClientStates.Active && !CustomHelper.Greater((ClientStates)y.State, (ClientStates)y.ClientParentState.NumericValue))),
                    TotalSuspended = clients.Count(y => (checkSuspention && y.ClientParentState != null && y.ClientParentState.NumericValue != null && y.ClientParentState.NumericValue == (int)ClientStates.Suspended && y.State != (int)ClientStates.Suspended &&
                                                   CustomHelper.Greater((ClientStates)y.ClientParentState.NumericValue, (ClientStates)y.State)) ||
                                                 (!checkSuspention && (y.State == (int)ClientStates.Suspended && (y.ClientParentState == null || y.ClientParentState.NumericValue == (int)ClientStates.Active) ||
                                                                       y.ClientParentState.NumericValue == (int)ClientStates.Suspended))),
                    TotalLocked = clients.Count(y => y.State == (int)ClientStates.ForceBlock &&
                                               (y.ClientParentState == null || y.ClientParentState.NumericValue == null || !CustomHelper.Greater((ClientStates)y.ClientParentState.NumericValue, (ClientStates)y.State))),
                    TotalClosed = clients.Count(y => (y.State == (int)ClientStates.FullBlocked &&
                                               (y.ClientParentState == null || y.ClientParentState.NumericValue == null || !CustomHelper.Greater((ClientStates)y.ClientParentState.NumericValue, (ClientStates)y.State))) ||
                                               (y.ClientParentState != null && y.ClientParentState.NumericValue != null && y.ClientParentState.NumericValue == (int)ClientStates.FullBlocked)),
                    TotalDisabled = clients.Count(y => (y.State == (int)ClientStates.Disabled || (y.ClientParentState != null && y.ClientParentState.NumericValue == (int)ClientStates.Disabled))),
                    Unchanged = clients.Count() - clients.Count(y => y.ClientParentState != null && y.ClientParentState.NumericValue != null && y.ClientParentState.NumericValue == (int)ClientStates.Suspended && y.State != (int)ClientStates.Suspended &&
                                                  CustomHelper.Greater((ClientStates)y.ClientParentState.NumericValue, (ClientStates)y.State))
                });
            }
            var nonExistLevels = levEnums.Where(x => x.Value > agent.Level &&
                                                  !result.Any(y => y.Id == x.Value)).Select(x => new { Level = x.Value, x.Text }).ToList();
            nonExistLevels.ForEach(x => result.Add(new AgentDownlinesStatuses { Id = x.Level, Name = x.Text }));
            return result.OrderBy(x => x.Id).ToList();
        }

        public List<User> FindUsers(string userIdentity, int? level, int? parentId, bool WithDownlines)
        {
            parentId = parentId ?? Identity.Id;
            userIdentity = userIdentity.ToLower();
            var levelKey = userIdentity;
            var keyWords = userIdentity.Split(' ');
            if (keyWords.Count() != 1)
            {
                userIdentity = keyWords[1];
                levelKey = keyWords[0];
            }
            var levels = GetEnumerations(nameof(AgentLevels), LanguageId).Where(x => x.Text.ToLower() == levelKey).Select(x => x.Value).ToList();
            var user = CacheManager.GetUserById(Identity.Id);
            var IsAdminUser = user.Type == (int)UserTypes.AdminUser;
            return Db.Users.Where(x => x.Id != Identity.Id && x.Type != (int)UserTypes.AgentEmployee &&
            ((!IsAdminUser && ((WithDownlines && x.Path.Contains("/" + (parentId) + "/") && x.Id != parentId) || (!WithDownlines && x.ParentId == parentId))) ||
                                        (IsAdminUser && x.ParentId == user.Id && x.Level == (int)AgentLevels.Company)) &&
                                        (level.HasValue && x.Level == level || !level.HasValue) &&
                                        (
                                          (keyWords.Count() > 1 && ((x.UserName.ToLower().Contains(userIdentity) ||
                                           x.NickName.ToLower().Contains(userIdentity) ||
                                           x.FirstName.ToLower().Contains(userIdentity) ||
                                           x.LastName.ToLower().Contains(userIdentity)) &&
                                           levels.Contains(x.Level.Value))
                                          ) ||
                                          (keyWords.Count() == 1 && ((x.UserName.ToLower().Contains(userIdentity) ||
                                           x.NickName.ToLower().Contains(userIdentity) ||
                                           x.FirstName.ToLower().Contains(userIdentity) ||
                                           x.LastName.ToLower().Contains(userIdentity)) ||
                                           levels.Contains(x.Level.Value))
                                          )
                                        )).ToList();
        }

        public List<AccountsBalanceHistoryElement> GetUserAccountsBalanceHistoryPaging(FilterAccountsBalanceHistory filter)
        {
            var user = CacheManager.GetUserById(filter.UserId);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = (int)ObjectTypes.User
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != filter.UserId)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != user.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var accountTypeKinds = GetEnumerations(Constants.EnumerationTypes.AccountTypeKinds, LanguageId).Select(x => new
            {
                Id = x.Value,
                Name = x.Text
            }).ToList();
            var operationTypes = GetOperationTypes();
            var balances = GetAccountsBalances((int)ObjectTypes.User, filter.UserId, filter.FromDate);
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var operations = Db.Transactions.Include(x => x.Account).ThenInclude(x => x.Type)
                    .Where(x => x.Account.ObjectTypeId == (int)ObjectTypes.User &&
                                x.Account.ObjectId == filter.UserId && x.Date >= fDate &&
                                x.Date < tDate).ToList()
                    .OrderBy(x => x.CreationTime)
                    .ThenBy(x => x.Account.TypeId)
                    .ToList();

            var result = new List<AccountsBalanceHistoryElement>();
            foreach (var operation in operations)
            {
                if (operation.Amount > 0)
                {
                    var balance = balances.First(x => x.AccountId == operation.AccountId);
                    result.Add(new AccountsBalanceHistoryElement
                    {
                        TransactionId = operation.Id,
                        DocumentId = operation.DocumentId,
                        AccountId = operation.AccountId,
                        AccountType = accountTypeKinds.First(x => x.Id == operation.Account.Type.Kind).Name,
                        BalanceBefore = balance.Balance,
                        OperationType = operationTypes.First(x => x.Id == operation.OperationTypeId).Name,
                        OperationAmount = operation.Amount,
                        BalanceAfter = balance.Balance + (operation.Type == (int)TransactionTypes.Credit ? -operation.Amount : operation.Amount),
                        OperationTime = operation.CreationTime,
                        PaymentSystemName = string.Empty
                    });
                    balance.Balance += (operation.Type == (int)TransactionTypes.Credit ? -operation.Amount : operation.Amount);
                }
            }
            return result;
        }

        #region UserDocument
        public  void AddAgentProfit(AgentProfit agentProfit)
        {
            Db.AgentProfits.Add(agentProfit);
            Db.SaveChanges();
        }

        public Document CreateDebitOnUser(UserTransferInput transferInput, DocumentBll documentBl)
        {
            var user = CacheManager.GetUserById(transferInput.UserId.Value);
            var creator = CacheManager.GetUserById(transferInput.FromUserId ?? Identity.Id);
            if (user == null || creator == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            var creatorId = Identity.Id;
            if (creator.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.CreateDebitCorrectionOnUser);
                creatorId = user.ParentId.Value;
            }
            if (creatorId == user.Id)
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);
            if (creator.Type == (int)UserTypes.AdminUser)
            {
                CheckPermission(Constants.Permissions.CreateDebitCorrectionOnUser);
                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewUser,
                    ObjectTypeId = (int)ObjectTypes.User
                });
                if (!checkP.HaveAccessForAllObjects && checkP.AccessibleObjects.All(x => x != user.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                if (!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != transferInput.UserId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                if (string.IsNullOrEmpty(transferInput.CurrencyId))
                    transferInput.CurrencyId = user.CurrencyId;
            }
            if ((user.Type == (int)UserTypes.MasterAgent && user.CurrencyId != transferInput.CurrencyId) ||
                (creator.Type == (int)UserTypes.AdminUser && user.Type == (int)UserTypes.Agent))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var userPermissions = CacheManager.GetUserPermissions(creatorId);
            var permission = userPermissions.FirstOrDefault(x => x.PermissionId == Constants.Permissions.EditPartnerAccounts || x.IsAdmin);

            var operation = new Operation
            {
                Type = (int)OperationTypes.DebitCorrectionOnUser,
                Creator = creatorId,
                UserId = transferInput.UserId,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.UserBalance,
                ObjectId = user.Id,
                ObjectTypeId = (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.DebitCorrectionOnUser
            };
            operation.OperationItems.Add(item);

            item = new OperationItem
            {
                AccountTypeId = permission != null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance,
                ObjectId = creator.Type == (int)UserTypes.AgentEmployee ? creator.ParentId.Value : (permission != null ? user.PartnerId : creator.Id),
                ObjectTypeId = permission != null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.DebitCorrectionOnUser
            };
            operation.OperationItems.Add(item);
            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            return document;
        }

        public Document CreateCreditOnUser(UserTransferInput transferInput, DocumentBll documentBl)
        {
            var user = CacheManager.GetUserById(transferInput.UserId.Value);
            var creator = CacheManager.GetUserById(Identity.Id);
            if (user == null || creator == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            var creatorId = Identity.Id;
            if (creator.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.CreateCreditCorrectionOnUser);
                creatorId = user.ParentId.Value;
            }
            if (creatorId == user.Id)
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);
            if (creator.Type == (int)UserTypes.AdminUser)
            {
                CheckPermission(Constants.Permissions.CreateCreditCorrectionOnUser);
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewUser,
                    ObjectTypeId = (int)ObjectTypes.User
                });

                if (!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != transferInput.UserId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                if (string.IsNullOrEmpty(transferInput.CurrencyId))
                    transferInput.CurrencyId = user.CurrencyId;
            }
            if ((user.Type == (int)UserTypes.MasterAgent && user.CurrencyId != transferInput.CurrencyId) || 
                (creator.Type == (int)UserTypes.AdminUser && user.Type == (int)UserTypes.Agent))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var userPermissions = CacheManager.GetUserPermissions(creatorId);
            var permission = userPermissions.FirstOrDefault(x => x.PermissionId == Constants.Permissions.EditPartnerAccounts || x.IsAdmin);

            var operation = new Operation
            {
                Type = (int)OperationTypes.CreditCorrectionOnUser,
                Creator = creatorId,
                Info = transferInput.Info,
                UserId = transferInput.UserId,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = permission != null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance,
                ObjectId = creator.Type == (int)UserTypes.AgentEmployee ? creator.ParentId.Value : (permission != null ? user.PartnerId : creator.Id),
                ObjectTypeId = permission != null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.CreditCorrectionOnUser
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.UserBalance,
                ObjectId = user.Id,
                ObjectTypeId = (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.CreditCorrectionOnUser
            };
            operation.OperationItems.Add(item);
            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            return document;
        }

        public Document TransferToUser(UserTransferInput transferInput, DocumentBll documentBl)
        {
            var user = GetUserById(transferInput.UserId.Value);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);

            var operation = new Operation
            {
                Type = (int)OperationTypes.CommissionForUser,
                Creator = transferInput.FromUserId,
                UserId = transferInput.UserId,
                Amount = transferInput.Amount,
                CurrencyId = user.CurrencyId,
                ExternalTransactionId = transferInput.ExternalTransactionId,
                ProductId = transferInput.ProductId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.UserBalance,
                ObjectId = user.Id,
                ObjectTypeId = (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = user.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.CommissionForUser
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.PartnerBalance,
                ObjectId = user.PartnerId,
                ObjectTypeId = (int)ObjectTypes.Partner,
                Amount = transferInput.Amount,
                CurrencyId = user.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.CommissionForUser
            };
            operation.OperationItems.Add(item);
            var document = documentBl.CreateDocumentFromJob(operation);
            Db.SaveChanges();
            return document;
        }

        public List<UserBalance> GetAgentAccountsInfo(int parentId)
        {
            var result = new List<UserBalance>();
            var agents = Db.Users.Include(x => x.UserSessions).Include(x => x.UserSettings).Where(x => x.ParentId == parentId && x.Type != (int)UserTypes.AgentEmployee).ToList();
            var parentUser = CacheManager.GetUserById(parentId);
            var parentAvailableBalance = new UserAccount();
            if (parentUser.Type != (int)UserTypes.AdminUser)
                parentAvailableBalance = GetUserBalance(parentId);
            agents.ForEach(x =>
            {
                var userBalance = GetUserBalance(x.Id);
                var subUserState = x.State;
                var parentState = CacheManager.GetUserSetting(x.Id)?.ParentState;
                if (parentState.HasValue && CustomHelper.Greater((UserStates)parentState.Value, (UserStates)subUserState))
                    subUserState = parentState.Value;

                result.Add(new UserBalance
                {
                    UserId = x.Id,
                    ObjectId = x.Id,
                    ObjectTypeId = (int)ObjectTypes.User,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Username = x.UserName,
                    Nickname = x.NickName,
                    Status = subUserState,
                    CurrencyId = x.CurrencyId,
                    AvailableCredit = userBalance.Balance,
                    ParentAvailableBalance = parentAvailableBalance.Balance,
                    AgentMaxCredit = x.UserSettings.FirstOrDefault()?.AgentMaxCredit,
                    Credit = userBalance.Credit ?? 0,
                    Cash = 0,
                    AvailableCash = 0,
                    YesterdayCash = 0,
                    Outstanding = 0,
                    LastLoginDate = x.UserSessions.Any() ? x.UserSessions.First().StartTime : null,
                    LastLoginIp = x.UserSessions.Any() ? x.UserSessions.First().Ip : null
                });
            });
            return result;
        }

        public UserAccount GetUserBalance(int userId)
        {
            var user = CacheManager.GetUserById(userId);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);

            var userAccount = Db.Accounts.FirstOrDefault(x => x.ObjectId == userId &&
            x.ObjectTypeId == (int)ObjectTypes.User);

            var userSetting = CacheManager.GetUserSetting(userId);
            return new UserAccount
            {
                Credit = userSetting?.AgentMaxCredit,
                CurrencyId = user.CurrencyId,
                Balance = userAccount != null ? Math.Floor(userAccount.Balance * 100) / 100 : 0
            };
        }

        public PagedModel<fnUserCorrection> GetUserCorrections(FilterUserCorrection filter)
        {
            var user = GetUserById(Identity.Id);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            if (user.Type == (int)UserTypes.AdminUser)
            {
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnUserCorrection>>();

                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewUser
                });

                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnUserCorrection>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                });

            }
            var result = new PagedModel<fnUserCorrection>
            {
                Entities = filter.FilterObjects(Db.fn_UserCorrection(), documents => documents.OrderByDescending(y => y.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_UserCorrection())
            };
            return result;
        }

        #endregion UserDocument

        #region Export to excel

        public List<User> ExportUsersModel(FilterUser filter)
        {
            var checkPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = (int)ObjectTypes.User
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportUsersModel
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<User>>
            {
                new CheckPermissionOutput<User>
                {
                    AccessibleObjects = checkPermission.AccessibleObjects,
                    HaveAccessForAllObjects = checkPermission.HaveAccessForAllObjects,
                    Filter = x => checkPermission.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<User>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<User>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },

            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            return filter.FilterObjects(Db.Users, x => x.OrderBy(y => y.Id)).ToList();
        }

        #endregion

        #region Agent

        public void CheckSecurityCode(string code)
        {
            var user = CacheManager.GetUserById(Identity.Id);
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(user.SecurityCode) || user.SecurityCode != code)
            {
                var partnerSetting = CacheManager.GetConfigParameters(user.PartnerId, Constants.PartnerKeys.AllowedFaildLoginCount).FirstOrDefault(x => x.Key == "SecurityCode");
                if (!partnerSetting.Equals(default(KeyValuePair<string, string>)) && int.TryParse(partnerSetting.Value, out int allowedNumber))
                {
                    var count = CacheManager.UpdateUserFailedSecurityCodeCount(user.Id);
                    if (count > allowedNumber)
                    {
                        BlockUserForce(user.Id, (int)UserStates.ForceBlockBySecurityCode);
                        LogoutUser(Identity.Token);
                        throw CreateException(LanguageId, Constants.Errors.SecurityCodeFailed);
                    }
                }
                throw CreateException(LanguageId, Constants.Errors.WrongSecurityCode);
            }
            CacheManager.RemoveUserSecurityCodeCountFromCache(user.Id);
        }

        public List<AgentCommission> GetAgentCommissionPlan(int partnerId, int? agentId, int? clientId, int? productId, bool checkPermission = true)
        {
            if (checkPermission)
            {
                CheckPermission(Constants.Permissions.ViewUser);
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                if (!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != partnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var query = Db.AgentCommissions.AsQueryable();
            if (agentId.HasValue)
                query = query.Where(x => x.AgentId == agentId);
            else if (clientId.HasValue)
                query = query.Where(x => x.ClientId == clientId);
            if (productId.HasValue)
                query.Where(x => x.ProductId == productId);
            return query.ToList();
        }

        public AgentCommission UpdateAgentCommission(AgentCommission agentCommissionInput)
        {
            var user = CacheManager.GetUserById(Identity.Id);
            if (agentCommissionInput.ClientId.HasValue && agentCommissionInput.AgentId.HasValue)
                throw BaseBll.CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
            var userId = user.Id;
            BllUser subAgent = null;
            BllClient member = null;
            if (user.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.EditCommissionPlan);
                userId = user.PartnerId;
            }
            if (agentCommissionInput.AgentId.HasValue)
            {
                subAgent = CacheManager.GetUserById(agentCommissionInput.AgentId.Value);
                if (subAgent == null)
                    throw CreateException(Identity.LanguageId, Constants.Errors.UserNotFound);
                if (subAgent.Id == userId)
                    throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
            }
            else if (agentCommissionInput.ClientId.HasValue)
            {
                member = CacheManager.GetClientById(agentCommissionInput.ClientId.Value);
                if (member == null)
                    throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotFound);
                if (member.UserId != userId)
                    throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
            }
            else
                throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);



            var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
            AsianCommissionPlan parentCommision = null;

            if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
            {
                var newCommitionPlan = JsonConvert.DeserializeObject<AsianCommissionPlan>(agentCommissionInput.TurnoverPercent);

                var parent = CacheManager.GetUserById(subAgent.ParentId.Value);
                if (subAgent.Type == (int)UserTypes.AdminUser || parent.Type == (int)UserTypes.AdminUser)
                {
                    var partnerCurrencySetting = CacheManager.GetPartnerCurrencies(subAgent.PartnerId).FirstOrDefault(x => x.CurrencyId == subAgent.CurrencyId);
                    parentCommision = new AsianCommissionPlan(partnerCurrencySetting?.ClientMinBet);
                }
                else
                    parentCommision = JsonConvert.DeserializeObject<AsianCommissionPlan>(Db.AgentCommissions.FirstOrDefault(x => x.AgentId == parent.Id).TurnoverPercent);

                newCommitionPlan.BetSettings.ForEach(x =>
                {
                    var pc = parentCommision.BetSettings.First(y => y.Name == x.Name);
                    if (x.MinBet < pc.MinBet || x.MaxBet > pc.MaxBet || x.MaxPerMatch > pc.MaxPerMatch || x.MinBet > x.MaxBet || x.MaxPerMatch < x.MaxBet)
                        throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                });
                var children = new List<AgentCommission>();
                children.AddRange(Db.AgentCommissions.Where(x => x.Agent.Path.Contains("/" + subAgent.Id + "/") && x.AgentId != subAgent.Id).ToList());
                children.AddRange(Db.AgentCommissions.Where(x => x.Client.User.Path.Contains("/" + subAgent.Id + "/")).ToList());
                foreach (var bs in newCommitionPlan.BetSettings)
                {
                    bs.MaxBetLimit = bs.MaxBet ?? 0;
                    bs.MinBetLimit = bs.MinBet ?? 0;
                    bs.MaxPerMatchLimit = bs.MaxPerMatch ?? 0;
                }
                agentCommissionInput.TurnoverPercent = JsonConvert.SerializeObject(newCommitionPlan);

                foreach (var ca in children)
                {
                    var cp = JsonConvert.DeserializeObject<AsianCommissionPlan>(ca.TurnoverPercent);
                    foreach (var bs in newCommitionPlan.BetSettings)
                    {
                        var oldBs = cp.BetSettings.FirstOrDefault(x => x.Name == bs.Name);
                        if (oldBs == null)
                        {
                            oldBs = new BetSetting();
                            cp.BetSettings.Add(oldBs);
                        }
                        oldBs.MaxBetLimit = Math.Min(oldBs.MaxBetLimit, bs.MaxBet ?? 0);
                        oldBs.MaxBet = Math.Min(oldBs.MaxBet ?? 0, bs.MaxBet ?? 0);

                        oldBs.MinBetLimit = Math.Max(oldBs.MinBetLimit, bs.MinBet ?? 0);
                        oldBs.MinBet = Math.Max(oldBs.MinBet ?? 0, bs.MinBet ?? 0);

                        oldBs.MaxPerMatchLimit = Math.Min(oldBs.MaxPerMatchLimit, bs.MaxPerMatch ?? 0);
                        oldBs.MaxPerMatch = Math.Min(oldBs.MaxPerMatch ?? 0, bs.MaxPerMatch ?? 0);

                        if (bs.PreventBetting)
                            oldBs.ParentPreventBetting = bs.PreventBetting;
                    }
                    ca.TurnoverPercent = JsonConvert.SerializeObject(cp);
                }
            }
            else if (subAgent != null && subAgent.ParentId.HasValue && subAgent.Type != (int)UserTypes.MasterAgent)
            {
                var parentCommission = Db.fn_ProductCommission(agentCommissionInput.ProductId, subAgent.ParentId).FirstOrDefault();
                if (parentCommission == null)
                    throw CreateException(Identity.LanguageId, Constants.Errors.WrongParentDocument);
                if (parentCommission.Percent < agentCommissionInput.Percent)
                    throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                bool isParentNumber = decimal.TryParse(parentCommission.TurnoverPercent, out decimal parentTurnoverPercent);
                bool isInputNumber = decimal.TryParse(agentCommissionInput.TurnoverPercent, out decimal turnoverPercent);
                if ((isParentNumber && isInputNumber && parentTurnoverPercent < turnoverPercent) ||
                     (isParentNumber && !isInputNumber && !string.IsNullOrEmpty(agentCommissionInput.TurnoverPercent) &&
                        JsonConvert.DeserializeObject<List<TurnoverPercent>>(agentCommissionInput.TurnoverPercent).Any(x => x.Percent > parentTurnoverPercent)) ||
                     (!isParentNumber && isInputNumber && JsonConvert.DeserializeObject<List<TurnoverPercent>>(parentCommission.TurnoverPercent).Any(x => x.Percent < turnoverPercent))
                   )
                    throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                else if (!isParentNumber && !isInputNumber)
                {
                    var parentTurnoverPercentList = JsonConvert.DeserializeObject<List<TurnoverPercent>>(parentCommission.TurnoverPercent);
                    var turnoverPercentList = JsonConvert.DeserializeObject<List<TurnoverPercent>>(agentCommissionInput.TurnoverPercent);
                    foreach (var turnover in turnoverPercentList)
                    {
                        if (parentTurnoverPercentList.Any(x => x.FromCount <= turnover.FromCount && x.ToCount >= turnover.ToCount && x.Percent < turnover.Percent))
                            throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                    }
                }
            }
            else if (subAgent == null)
            {
                var parentCommission = Db.fn_ProductCommission(agentCommissionInput.ProductId, member.UserId).FirstOrDefault();
                if (parentCommission == null)
                    throw CreateException(Identity.LanguageId, Constants.Errors.WrongParentDocument);
                if (parentCommission.Percent < agentCommissionInput.Percent)
                    throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                bool isParentNumber = decimal.TryParse(parentCommission.TurnoverPercent, out decimal parentTurnoverPercent);
                bool isInputNumber = decimal.TryParse(agentCommissionInput.TurnoverPercent, out decimal turnoverPercent);
                if ((isParentNumber && isInputNumber && parentTurnoverPercent < turnoverPercent) ||
                     (isParentNumber && !isInputNumber && !string.IsNullOrEmpty(agentCommissionInput.TurnoverPercent) &&
                        JsonConvert.DeserializeObject<List<TurnoverPercent>>(agentCommissionInput.TurnoverPercent).Any(x => x.Percent > parentTurnoverPercent)) ||
                     (!isParentNumber && isInputNumber && JsonConvert.DeserializeObject<List<TurnoverPercent>>(parentCommission.TurnoverPercent).Any(x => x.Percent < turnoverPercent))
                   )
                    throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                else if (!isParentNumber && !isInputNumber)
                {
                    var parentTurnoverPercentList = JsonConvert.DeserializeObject<List<TurnoverPercent>>(parentCommission.TurnoverPercent);
                    var turnoverPercentList = JsonConvert.DeserializeObject<List<TurnoverPercent>>(agentCommissionInput.TurnoverPercent);
                    foreach (var turnover in turnoverPercentList)
                    {
                        if (parentTurnoverPercentList.Any(x => x.FromCount <= turnover.FromCount && x.ToCount >= turnover.ToCount && x.Percent < turnover.Percent))
                            throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                    }
                }
            }

            var dbAgentCommission = Db.AgentCommissions.FirstOrDefault(x => x.AgentId == agentCommissionInput.AgentId &&
                                                                            x.ProductId == agentCommissionInput.ProductId &&
                                                                            x.ClientId == agentCommissionInput.ClientId);
            if (dbAgentCommission == null)
            {
                if (agentCommissionInput.Percent != null || !string.IsNullOrEmpty(agentCommissionInput.TurnoverPercent))
                {
                    Db.AgentCommissions.Add(agentCommissionInput);
                    dbAgentCommission = agentCommissionInput;
                }
            }
            else
            {
                if (agentCommissionInput.Percent == null && string.IsNullOrEmpty(agentCommissionInput.TurnoverPercent))
                {
                    Db.AgentCommissions.Remove(dbAgentCommission);
                    dbAgentCommission = null;
                }
                else
                {
                    dbAgentCommission.Percent = agentCommissionInput.Percent;
                    if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                    {
                        var ocp = JsonConvert.DeserializeObject<AsianCommissionPlan>(dbAgentCommission.TurnoverPercent);
                        var ncp = JsonConvert.DeserializeObject<AsianCommissionPlan>(agentCommissionInput.TurnoverPercent);
                        for (int i = 0; i < ocp.Groups.Count; i++)
                        {
                            if (ocp.Groups[i].Value > ncp.Groups[i].Value)
                                UpdateDownlineCommissionGroups(dbAgentCommission, ncp.Groups[i].Value, i);
                        }
                    }
                    dbAgentCommission.TurnoverPercent = agentCommissionInput.TurnoverPercent;
                }
            }
            Db.SaveChanges();
            if (agentCommissionInput.ClientId != null)
            {
                var partnerProducts = CacheManager.GetPartnerProductSettingsForDesktop(user.PartnerId, Constants.DefaultLanguageId);
                foreach (var pp in partnerProducts)
                    CacheManager.RemoveFromCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientProductCommissionTree, agentCommissionInput.ClientId, pp.ProductId));
                partnerProducts = CacheManager.GetPartnerProductSettingsForMobile(user.PartnerId, Constants.DefaultLanguageId);
                foreach (var pp in partnerProducts)
                    CacheManager.RemoveFromCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientProductCommissionTree, agentCommissionInput.ClientId, pp.ProductId));
            }
            return dbAgentCommission;
        }

        private void UpdateDownlineCommissionGroups(AgentCommission dbAgentCommission, decimal newValue, int index)
        {
            var ocp = JsonConvert.DeserializeObject<AsianCommissionPlan>(dbAgentCommission.TurnoverPercent);
            var childAgents = Db.AgentCommissions.Where(x => x.Agent.ParentId == dbAgentCommission.AgentId).ToList();
            foreach (var c in childAgents)
            {
                var cocp = JsonConvert.DeserializeObject<AsianCommissionPlan>(c.TurnoverPercent);
                if (ocp.Groups[index].Value - cocp.Groups[index].Value >= 0 && newValue - cocp.Groups[index].Value < 0)
                    UpdateDownlineCommissionGroups(c, newValue, index);
                else if(ocp.Groups[index].Value - cocp.Groups[index].Value < 0)
                    UpdateDownlineCommissionGroups(c, newValue - ocp.Groups[index].Value + cocp.Groups[index].Value, index);
            }
            ocp.Groups[index].Value = newValue;
            dbAgentCommission.TurnoverPercent = JsonConvert.SerializeObject(ocp);
            var childMembers = Db.AgentCommissions.Include(x => x.Client).Where(x => x.Client.UserId == dbAgentCommission.AgentId).ToList();
            foreach (var c in childMembers)
            {
                var cocp = JsonConvert.DeserializeObject<AsianCommissionPlan>(c.TurnoverPercent);
                if (index > 3 && ocp.Groups[index].Value - cocp.Groups[index - 3].Value >= 0 && newValue - cocp.Groups[index - 3].Value < 0)
                {
                    cocp.Groups[index - 3].Value = newValue;
                    c.TurnoverPercent = JsonConvert.SerializeObject(cocp);
                }
                else if (index <= 3 && index + 1 == (c.Client.CategoryId % 10) && 
                    ocp.Groups[index].Value - cocp.Groups[0].Value >= 0 && newValue - cocp.Groups[0].Value < 0)
                {
                    cocp.Groups[0].Value = newValue;
                    c.TurnoverPercent = JsonConvert.SerializeObject(cocp);
                }

                else if (index > 3 && ocp.Groups[index].Value - cocp.Groups[index - 3].Value < 0)
                {
                    cocp.Groups[index - 3].Value = newValue - ocp.Groups[index].Value + cocp.Groups[index - 3].Value;
                    c.TurnoverPercent = JsonConvert.SerializeObject(cocp);
                }
                else if (index <= 3 && index + 1 == (c.Client.CategoryId % 10) && ocp.Groups[index].Value - cocp.Groups[0].Value < 0)
                {
                    cocp.Groups[0].Value = newValue - ocp.Groups[index].Value + cocp.Groups[0].Value;
                    c.TurnoverPercent = JsonConvert.SerializeObject(cocp);
                }
            }
            
            Db.SaveChanges();
        }

        public AgentCommission UpdateMemberCommission(AgentCommission agentCommissionInput)
        {
            var user = CacheManager.GetUserById(Identity.Id);
            if (user == null)
                throw BaseBll.CreateException(Identity.LanguageId, Constants.Errors.UserNotFound);
            var member = CacheManager.GetClientById(agentCommissionInput.ClientId.Value);
            if (member == null || !member.UserId.HasValue)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotFound);
            if (user.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.EditCommissionPlan);
                if (member.UserId != user.ParentId)
                    throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
            }
            else if (member.UserId != Identity.Id)
                throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);

            var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
            if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
            {
                var newCommitionPlan = JsonConvert.DeserializeObject<AsianCommissionPlan>(agentCommissionInput.TurnoverPercent);
                AsianCommissionPlan parentCommision = null;
                if (user.Type == (int)UserTypes.AdminUser)
                {
                    var partnerCurrencySetting = CacheManager.GetPartnerCurrencies(user.PartnerId).FirstOrDefault(x => x.CurrencyId == user.CurrencyId);
                    parentCommision = new AsianCommissionPlan(partnerCurrencySetting?.ClientMinBet);
                }
                else
                    parentCommision = JsonConvert.DeserializeObject<AsianCommissionPlan>(Db.AgentCommissions.FirstOrDefault(x => x.AgentId == user.Id).TurnoverPercent);
                newCommitionPlan.BetSettings.ForEach(x =>
                {
                    var pc = parentCommision.BetSettings.First(y => y.Name == x.Name);
                    if (x.MinBet < pc.MinBet || x.MaxBet > pc.MaxBet || x.MaxPerMatch > pc.MaxPerMatch || x.MinBet > x.MaxBet || x.MaxPerMatch < x.MaxBet)
                        throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                });
                foreach (var bs in newCommitionPlan.BetSettings)
                {
                    bs.MaxBetLimit = bs.MaxBet ?? 0;
                    bs.MinBetLimit = bs.MinBet ?? 0;
                    bs.MaxPerMatchLimit = bs.MaxPerMatch ?? 0;
                }
                agentCommissionInput.TurnoverPercent = JsonConvert.SerializeObject(newCommitionPlan);
            }
            
            var dbAgentCommission = Db.AgentCommissions.FirstOrDefault(x => x.ClientId == member.Id && x.ProductId == agentCommissionInput.ProductId);
            if (dbAgentCommission == null)
            {
                if (agentCommissionInput.Percent != null || !string.IsNullOrEmpty(agentCommissionInput.TurnoverPercent))
                {
                    Db.AgentCommissions.Add(agentCommissionInput);
                    dbAgentCommission = agentCommissionInput;
                }
            }
            else
            {
                if (agentCommissionInput.Percent == null && string.IsNullOrEmpty(agentCommissionInput.TurnoverPercent))
                {
                    Db.AgentCommissions.Remove(dbAgentCommission);
                    dbAgentCommission = null;
                }
                else
                {
                    dbAgentCommission.Percent = agentCommissionInput.Percent;
                    dbAgentCommission.TurnoverPercent = agentCommissionInput.TurnoverPercent;
                }
            }
            Db.SaveChanges();
            return dbAgentCommission;
        }

        public fnAgent UpdateAgent(int subAgentId, int state, string pass, out List<int> clientIds)
        {
            var user = CacheManager.GetUserById(Identity.Id);
            if (user.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.CreateUser);
                user = CacheManager.GetUserById(user.ParentId.Value);
            }
            var subAgent = GetUserById(subAgentId);
            if (subAgent == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.UserNotFound);
            var subAgentState = subAgent.State;
            var parentState = CacheManager.GetUserSetting(subAgentId)?.ParentState;
            if (parentState.HasValue && CustomHelper.Greater((UserStates)parentState.Value, (UserStates)subAgentState))
                subAgentState = parentState.Value;
            var parentAgent = CacheManager.GetUserById(subAgent.ParentId.Value);

            var parentAgentState = parentAgent.State;
            var parentSetting = CacheManager.GetUserSetting(parentAgent.Id);
            if (parentSetting != null && parentSetting.ParentState.HasValue && CustomHelper.Greater((UserStates)parentSetting.ParentState.Value, (UserStates)parentAgentState))
                parentAgentState = parentSetting.ParentState.Value;

            if (!Enum.IsDefined(typeof(UserStates), state) || 
                !subAgent.Path.Contains("/" + user.Id.ToString() + "/") ||
                subAgentState == (int)UserStates.Disabled || 
               (state == (int)UserStates.Disabled && subAgentState != (int)UserStates.Closed && subAgentState != (int)UserStates.InactivityClosed) || 
               (parentAgentState!=state && (parentAgentState != (int)UserStates.ForceBlock && parentAgentState != (int)UserStates.ForceBlockBySecurityCode) &&
                CustomHelper.Greater((UserStates)parentAgentState, (UserStates)state))
              )
                throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
            clientIds = new List<int>();
            using (var documentBll = new DocumentBll(this))
            {
                using (var clientBll = new ClientBll(this))
                {
                    if (subAgentState != state)
                    {
                        if ((subAgentState != (int)UserStates.Closed && subAgentState != (int)UserStates.InactivityClosed) || state == (int)UserStates.Disabled)
                            ChangeUserState(subAgentId, state, documentBll, clientBll, clientIds);
                        subAgent.State = state;
                        Db.UserSettings.Where(x => x.UserId == subAgent.Id).UpdateFromQuery(x => new UserSetting { ParentState = state });
                        Db.SaveChanges();
                        CacheManager.RemoveUserSetting(subAgent.Id);
                        if (subAgentState == (int)UserStates.ForceBlock)
                            CacheManager.RemoveUserFailedLoginCountFromCache(subAgent.Id);
                        else if(subAgentState == (int)UserStates.ForceBlockBySecurityCode)
                            CacheManager.RemoveUserSecurityCodeCountFromCache(subAgent.Id);

                    }
                    if (!string.IsNullOrEmpty(pass))
                    {
                        subAgent.PasswordHash = CommonFunctions.ComputeUserPasswordHash(pass, subAgent.Salt);
                        subAgent.PasswordChangedDate = GetServerDate();
                        Db.SaveChanges();
                    }
                    CacheManager.RemoveUserFromCache(subAgentId);
                    return Db.fn_Agent(subAgentId).FirstOrDefault(x => x.Id == subAgentId);
                }
            }
        }

        public fnAgent GetfnAgent(int agentId)
        {
            return Db.fn_Agent(agentId).FirstOrDefault(x => x.Id == agentId);
        }

        private void ChangeUserState(int userId, int state, DocumentBll documentBll, ClientBll clientBll, List<int> clientIds)
        {
            var user = GetUserById(userId);
            var currentState = user.State;
            var clientState = CustomHelper.MapUserStateToClient[state];
            var subUsers = Db.Users.Where(x => x.ParentId == userId && x.State != (int)UserStates.Disabled).ToList();
            var currentDate = DateTime.UtcNow;
            foreach (var subUser in subUsers)
            {
                var subUserState = subUser.State;
                var parentState = CacheManager.GetUserSetting(subUser.Id)?.ParentState;
                if (parentState.HasValue && CustomHelper.Greater((UserStates)parentState.Value, (UserStates)subUserState))
                    subUserState = parentState.Value;

                if ((state == (int)UserStates.Active && parentState.HasValue && parentState.Value == (int)UserStates.Suspended && subUser.State != (int)UserStates.Suspended) ||
                    (state == (int)UserStates.Suspended && (subUserState == (int)UserStates.Active ||
                    subUserState == (int)UserStates.Suspended ||
                    subUserState == (int)UserStates.ForceBlock || subUserState == (int)UserStates.ForceBlockBySecurityCode)) ||
                   ((state == (int)UserStates.Closed || state == (int)UserStates.InactivityClosed) && subUserState != (int)UserStates.Disabled) ||
                     state == (int)UserStates.Disabled)
                {
                    Db.UserSettings.Where(x => x.UserId == subUser.Id).UpdateFromQuery(x => new UserSetting { ParentState = state });
                    ChangeUserState(subUser.Id, state, documentBll, clientBll, clientIds);
                    if (state == (int)UserStates.Disabled)
                        Db.UserSessions.Where(x => x.UserId == subUser.Id && x.State == (int)SessionStates.Active).
                        UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentDate, LogoutType = (int)LogoutTypes.System });
                }
            }

            var clients = Db.Clients.Where(x => x.UserId == userId && x.State != (int)ClientStates.Disabled).ToList();
            clients.ForEach(x =>
            {
                var currState = x.State;
                var st = CacheManager.GetClientSettingByName(x.Id, ClientSettings.ParentState);
                if (st.NumericValue.HasValue && CustomHelper.Greater((ClientStates)st.NumericValue, (ClientStates)currState))
                    currState = Convert.ToInt32(st.NumericValue.Value);

                if ((clientState == (int)ClientStates.Active && currState == (int)ClientStates.Suspended && x.State != (int)ClientStates.Suspended) ||
                (clientState == (int)ClientStates.Suspended && (currState == (int)ClientStates.Active || currState == (int)ClientStates.ForceBlock)) ||
                  ((clientState == (int)ClientStates.FullBlocked) && currState != (int)ClientStates.Disabled) ||
                    clientState == (int)ClientStates.Disabled)
                {
                    var clientSetting = new ClientCustomSettings
                    {
                        ClientId = x.Id,
                        ParentState = clientState
                    };
                    clientBll.SaveClientSetting(clientSetting);
                }
                if (clientState == (int)ClientStates.Disabled)
                {
                    var balance = CacheManager.GetClientCurrentBalance(x.Id).Balances
                    .Where(y => y.TypeId != (int)AccountTypes.ClientBonusBalance &&
                                y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                y.TypeId != (int)AccountTypes.ClientCoinBalance)
                    .Sum(y => y.Balance);
                    var clientCorrectionInput = new ClientCorrectionInput
                    {
                        Amount = balance,
                        CurrencyId = x.CurrencyId,
                        ClientId = x.Id
                    };
                    clientBll.CreateCreditCorrectionOnClient(clientCorrectionInput, documentBll, false);
                    var clientSetting = new ClientCustomSettings
                    {
                        ClientId = x.Id,
                        MaxCredit = 0
                    };
                    clientBll.SaveClientSetting(clientSetting);
                }
                else if (clientState == (int)ClientStates.FullBlocked)
                    clientBll.LogoutClientById(x.Id, (int)LogoutTypes.Admin);
            });
            if (state == (int)UserStates.Closed)
            {
                Db.UserSessions.Where(x => x.UserId == userId && x.State == (int)SessionStates.Active).
                     UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentDate, LogoutType = (int)LogoutTypes.System });
            }
            else if (state == (int)UserStates.Disabled)
            {
                var userBalance = GetUserBalance(user.Id);
                var userTransferInput = new UserTransferInput
                {
                    FromUserId = userId,
                    UserId = user.ParentId,
                    Amount = userBalance.Balance,
                    CurrencyId = user.CurrencyId
                };
                CreateDebitOnUser(userTransferInput, documentBll);
                var levelLimits = JsonConvert.SerializeObject(Enum.GetValues(typeof(AgentLevels)).Cast<int>()
                                 .Where(x => x >= user.Level).Select(x => new { Level = x, Limit = 0 }).ToList());
                Db.UserSettings.Where(x => x.UserId == userId).UpdateFromQuery(x => new UserSetting { LevelLimits = levelLimits, AgentMaxCredit = 0 });
            }
            Db.SaveChanges();
            if (currentState == (int)UserStates.ForceBlock)
                CacheManager.RemoveUserFailedLoginCountFromCache(userId);
            else if (currentState == (int)UserStates.ForceBlockBySecurityCode)
                CacheManager.RemoveUserSecurityCodeCountFromCache(userId);

            CacheManager.RemoveUserSetting(userId);
        }

        public List<CommissionItem> GetAgentTurnoverProfit(long fromDate, long toDate)
        {
            var result = new List<CommissionItem>();
            var clients = Db.fn_ProfitByClientProduct(fromDate, toDate).ToList();

            foreach (var client in clients)
            {
                var clientCommissionTree = CacheManager.GetClientProductCommissionTree(client.ClientId, client.ProductId);
                var senderId = client.AgentId.Value;
                decimal previousItemInitialPercent = 0;
                for (int i = clientCommissionTree.Count - 1; i >= 0; i--)
                {
                    if (clientCommissionTree[i].AgentId!= null && senderId != clientCommissionTree[i].AgentId)
                        senderId = CacheManager.GetUserById(clientCommissionTree[i].AgentId.Value).ParentId.Value;
                    decimal percent = 0;
                    if (!decimal.TryParse(clientCommissionTree[i].TurnoverPercent, out percent))
                    {
                        var percents = JsonConvert.DeserializeObject<List<TurnoverPercent>>(clientCommissionTree[i].TurnoverPercent);
                        var p = percents.FirstOrDefault(x => client.SelectionsCount >= x.FromCount && client.SelectionsCount <= x.ToCount);
                        percent = p == null ? 0 : p.Percent;
                    }
                    var finalPercent = percent - previousItemInitialPercent;
                    previousItemInitialPercent = percent;

                    if (finalPercent > 0)
                    {
                        var item = result.FirstOrDefault(x => x.RecieverAgentId == clientCommissionTree[i].AgentId &&
                        x.RecieverClientId == clientCommissionTree[i].ClientId &&
                        x.SenderAgentId == senderId && x.ProductId == client.ProductId);
                        if (item != null)
                        {
                            item.TotalBetAmount += client.TotalBetAmount ?? 0;
                            item.TotalWinAmount += client.TotalWinAmount ?? 0;
                            item.TotalProfit += ((client.TotalBetAmount ?? 0) * finalPercent / 100);
                        }
                        else
                        {
                            var productGroupId = Db.fn_ProductGroup(client.ProductId).FirstOrDefault()?.Id;
                            result.Add(new CommissionItem
                            {
                                RecieverAgentId = clientCommissionTree[i].AgentId,
                                RecieverClientId = clientCommissionTree[i].ClientId,
                                SenderAgentId = senderId,
                                TotalBetAmount = client.TotalBetAmount ?? 0,
                                TotalWinAmount = client.TotalWinAmount ?? 0,
                                TotalProfit = (client.TotalBetAmount ?? 0) * finalPercent / 100,
                                ProductId = client.ProductId,
                                ProductGroupId = productGroupId
                            });
                        }
                    }
                }
            }
            return result;
        }

        public List<fnAgentProfit> GetAgentProfit(long fromDate, long toDate)
        {
            return Db.fn_AgentProfit(fromDate, toDate).ToList();
        }

        public List<AgentCommission> GetAgentCommissions(List<int> agentIds)
        {
            return Db.AgentCommissions.Include(x => x.Agent).Where(x => agentIds.Contains(x.AgentId.Value)).ToList();
        }
        public List<AgentCommission> GetClientCommissions(List<int> clientIds)
        {
            return Db.AgentCommissions.Include(x => x.Client).Where(x => clientIds.Contains(x.ClientId.Value)).ToList();
        }

        public int GetUnreadTicketsCount(int partnerId, int? userId)
        {
            var query = Db.fn_Ticket().Where(x => x.PartnerId == partnerId && x.UserUnreadMessagesCount > 0);
            if (userId != null)
                query = query.Where(x => x.ClientPath.Contains("/" + userId.Value + "/"));

            return query.Count();
        }
        #endregion
    }
}