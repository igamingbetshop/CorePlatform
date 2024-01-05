using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;

namespace IqSoft.CP.BLL.Services
{
    public class BetShopBll : PermissionBll, IBetShopBll
    {
        #region Constructors

        public BetShopBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public BetShopBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public PagedModel<fnBetShops> GetBetShopsPagedModel(FilterfnBetShop filter, bool checkPermissions)
        {
            if(checkPermissions)
                CreateFilterForGetfnBetShops(filter);

            Func<IQueryable<fnBetShops>, IOrderedQueryable<fnBetShops>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnBetShops>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnBetShops>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = betShop => betShop.OrderByDescending(x => x.Id);
            }
            return new PagedModel<fnBetShops>
            {
                Entities = filter.FilterObjects(Db.fn_BetShops(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_BetShops())
            };
        }

        public List<BetShop> GetBetShops(FilterBetShop filter, bool checkPermissions)
        {
            CreateFilterForGetBetShops(filter, checkPermissions);

            return filter.FilterObjects(Db.BetShops).ToList();
        }

        public List<BetShop> GetBetShopsByClientId(int clientId)
        {
            var betShopList = (from bs in Db.BetShops
                               join pr in Db.PaymentRequests on bs.Id equals pr.BetShopId
                               where pr.ClientId == clientId && bs.State == Constants.CashDeskStates.Active
                               group bs by new
                               {
                                   bs.Id,
                                   bs.GroupId,
                                   bs.CurrencyId,
                                   bs.Address,
                                   bs.PartnerId,
                                   bs.State,
                                   bs.DailyTicketNumber,
                                   bs.DefaultLimit,
                                   bs.CurrentLimit,
                                   bs.SessionId,
                                   bs.CreationTime,
                                   bs.LastUpdateTime,
                                   bs.Type,
                                   bs.RegionId,
                                   bs.Name
                               } into betShop
                               select new
                               {
                                   Id = betShop.Key.Id,
                                   GroupId = betShop.Key.GroupId,
                                   CurrencyId = betShop.Key.CurrencyId,
                                   Address = betShop.Key.Address,
                                   PartnerId = betShop.Key.PartnerId,
                                   State = betShop.Key.State,
                                   DailyTicketNumber = betShop.Key.DailyTicketNumber,
                                   DefaultLimit = betShop.Key.DefaultLimit,
                                   CurrentLimit = betShop.Key.CurrentLimit,
                                   SessionId = betShop.Key.SessionId,
                                   CreationTime = betShop.Key.CreationTime,
                                   LastUpdateTime = betShop.Key.LastUpdateTime,
                                   Type = betShop.Key.Type,
                                   RegionId = betShop.Key.RegionId,
                                   Name = betShop.Key.Name
                               }).AsEnumerable().Select(x => new BetShop
                               {
                                   Id = x.Id,
                                   GroupId = x.GroupId,
                                   CurrencyId = x.CurrencyId,
                                   Address = x.Address,
                                   PartnerId = x.PartnerId,
                                   State = x.State,
                                   DailyTicketNumber = x.DailyTicketNumber,
                                   DefaultLimit = x.DefaultLimit,
                                   CurrentLimit = x.CurrentLimit,
                                   SessionId = x.SessionId,
                                   CreationTime = x.CreationTime,
                                   LastUpdateTime = x.LastUpdateTime,
                                   Type = x.Type,
                                   RegionId = x.RegionId,
                                   Name = x.Name
                               }).ToList();
            return betShopList ?? new List<BetShop>();
        }

        public PagedModel<fnCashDesks> GetCashDesksPagedModel(FilterfnCashDesk filter, bool checkPermission = true)
        {
			if (checkPermission)
				CreateFilterfnForGetCashDesks(filter);

            Func<IQueryable<fnCashDesks>, IOrderedQueryable<fnCashDesks>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnCashDesks>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnCashDesks>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = cashDesk => cashDesk.OrderByDescending(x => x.Id);
            }

            return new PagedModel<fnCashDesks>
            {
                Entities = filter.FilterObjects(Db.fn_CashDesks(), orderBy),
                Count = filter.SelectedObjectsCount(Db.fn_CashDesks())
            };
        }

        public List<CashDesk> GetCashDesks(FilterCashDesk filter, bool checkPermission)
        {
            if(checkPermission)
                CreateFilterForGetCashDesks(filter);
    
            return filter.FilterObjects(Db.CashDesks).ToList();
        }

        public List<BetShopGroup> GetBetShopGroups(FilterBetShopGroup filter, bool checkPermission)
        {
            if(checkPermission)
                CreateFilterGetBetShopGroups(filter);
            return filter.FilterObjects(Db.BetShopGroups).ToList();
        }

        public BetShopGroup SaveBetShopGroup(BetShopGroup betShopGroup)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateBetShopGroup,
                ObjectTypeId = ObjectTypes.BetShopGroup,
                ObjectId = betShopGroup.Id
            });
            var currentTime = GetServerDate();
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                BetShopGroup parentBetShopGroup = null;
                var dbBetShopGroup = Db.BetShopGroups.FirstOrDefault(x => x.Id == betShopGroup.Id);
                if (dbBetShopGroup == null)
                {
                    dbBetShopGroup = new BetShopGroup { CreationTime = currentTime, IsLeaf = true, Path = string.Empty };
                    Db.BetShopGroups.Add(dbBetShopGroup);

                    if (betShopGroup.ParentId.HasValue)
                    {
                        parentBetShopGroup = Db.BetShopGroups.FirstOrDefault(x => x.Id == betShopGroup.ParentId);
                        if (parentBetShopGroup == null)
                            throw CreateException(LanguageId, Constants.Errors.ParentBetShopGroupNotFound);
                        if (parentBetShopGroup.State != Constants.BetShopGroupStates.Active)
                            throw CreateException(LanguageId, Constants.Errors.ParentBetShopGroupBlocked);
                        if (parentBetShopGroup.PartnerId != betShopGroup.PartnerId)
                            throw CreateException(LanguageId, Constants.Errors.WrongPartnerId);

                        parentBetShopGroup.IsLeaf = false;
                        ChangeBetShopBetShopGroups(betShopGroup.ParentId.Value, dbBetShopGroup.Id);
                    }
                }
                if (betShopGroup.State != dbBetShopGroup.State && (betShopGroup.State == Constants.BetShopGroupStates.ManuallyBlocked ||
                    betShopGroup.State == Constants.BetShopGroupStates.Active))
                {
                    dbBetShopGroup.State = betShopGroup.State;
                    AuthomaticallyBlockChildBetShopGroups(dbBetShopGroup);
                }

                if (betShopGroup.ParentId != dbBetShopGroup.ParentId)
                {
                    if (dbBetShopGroup.ParentId.HasValue && !Db.BetShopGroups.Any(x => x.ParentId == dbBetShopGroup.ParentId && x.Id != dbBetShopGroup.Id))
                    {
                        var oldParentBetShopGroup = Db.BetShopGroups.FirstOrDefault(x => x.Id == dbBetShopGroup.ParentId);
                        if (oldParentBetShopGroup == null)
                            throw CreateException(LanguageId, Constants.Errors.ParentBetShopGroupNotFound);
                        oldParentBetShopGroup.IsLeaf = true;
                    }
                    if (betShopGroup.ParentId.HasValue)
                    {
                        parentBetShopGroup = Db.BetShopGroups.FirstOrDefault(x => x.Id == betShopGroup.ParentId);
                        if (parentBetShopGroup == null)
                            throw CreateException(LanguageId, Constants.Errors.ParentBetShopGroupNotFound);
                        parentBetShopGroup.IsLeaf = false;
                    }

                    dbBetShopGroup.Path = parentBetShopGroup != null
                        ? string.Format(@"{0}{1}\", parentBetShopGroup.Path, dbBetShopGroup.Id)
                        : dbBetShopGroup.Id.ToString();
                    ChangeChildBetShopGroupPaths(dbBetShopGroup);
                }

                betShopGroup.CreationTime = dbBetShopGroup.CreationTime;
                betShopGroup.Path = dbBetShopGroup.Path;
                Db.Entry(dbBetShopGroup).CurrentValues.SetValues(betShopGroup);
                dbBetShopGroup.LastUpdateTime = currentTime;
                dbBetShopGroup.SessionId = SessionId;
                SaveChanges();

                parentBetShopGroup = parentBetShopGroup ??
                                     Db.BetShopGroups.FirstOrDefault(x => x.Id == dbBetShopGroup.ParentId);
                var path = dbBetShopGroup.ParentId.HasValue
                    ? string.Format(@"{0}{1}\", parentBetShopGroup.Path, dbBetShopGroup.Id)
                    : string.Format(@"{0}\", dbBetShopGroup.Id);
                if (path != dbBetShopGroup.Path)
                {
                    dbBetShopGroup.Path = path;
                    SaveChanges();
                }
                scope.Complete();
                CacheManager.DeleteBetShopGroup(betShopGroup.Id);
                return dbBetShopGroup;
            }
        }

        private void ChangeChildBetShopGroupPaths(BetShopGroup betShopGroup)
        {
            var childGroups = Db.BetShopGroups.Where(x => x.ParentId == betShopGroup.Id).ToList();
            foreach (var childGroup in childGroups)
            {
                childGroup.Path = string.Format(@"{0}\{1}", betShopGroup.Path, childGroup.Id);
                ChangeChildBetShopGroupPaths(childGroup);
            }
        }

        private void ChangeBetShopBetShopGroups(int oldBetShopGroupId, int newBetShopGroupId)
        {
            var betShops = Db.BetShops.Where(x => x.GroupId == oldBetShopGroupId).ToList();
            foreach (var betShop in betShops)
            {
                betShop.GroupId = newBetShopGroupId;
            }
        }

        public void AuthomaticallyBlockChildBetShopGroups(BetShopGroup betShopGroup)
        {
            if (betShopGroup.IsLeaf)
            {
                var betShops = Db.BetShops.Where(x => x.GroupId == betShopGroup.Id && x.State == Constants.CashDeskStates.Active).ToList();
                foreach (var betShop in betShops)
                {
                    betShop.State = Constants.CashDeskStates.Blocked;
                }
            }
            var betShopGroups = Db.BetShopGroups.Where(x => x.ParentId == betShopGroup.Id && x.State == Constants.BetShopGroupStates.Active).ToList();
            foreach (var childGroup in betShopGroups)
            {
                childGroup.State = Constants.BetShopGroupStates.AutomaticallyBlocked;
                AuthomaticallyBlockChildBetShopGroups(childGroup);
            }
        }

        public void AuthomaticallyUnBlockChildBetShopGroups(BetShopGroup betShopGroup)
        {
            if (betShopGroup.IsLeaf)
            {
                var betShops = Db.BetShops.Where(x => x.GroupId == betShopGroup.Id && x.State == Constants.CashDeskStates.Blocked).ToList();
                foreach (var betShop in betShops)
                {
                    betShop.State = Constants.CashDeskStates.Active;
                }
            }
            var betShopGroups = Db.BetShopGroups.Where(x => x.ParentId == betShopGroup.Id && x.State == Constants.BetShopGroupStates.AutomaticallyBlocked).ToList();
            foreach (var childGroup in betShopGroups)
            {
                childGroup.State = Constants.BetShopGroupStates.Active;
                AuthomaticallyUnBlockChildBetShopGroups(childGroup);
            }
        }

        public BetShop SaveBetShop(BetShop betShop)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateBetShop,
                ObjectTypeId = ObjectTypes.BetShop,
                ObjectId = betShop.Id
            });
            var currentTime = GetServerDate();
			var betShopGroupPartnerId = Db.BetShopGroups.Where(x => x.Id == betShop.GroupId).Select(x => x.PartnerId).FirstOrDefault();
			if (betShopGroupPartnerId == 0)
				throw CreateException(LanguageId, Constants.Errors.BetShopGroupNotFound);

			var dbBetShop = Db.BetShops.FirstOrDefault(x => x.Id == betShop.Id);
			if (dbBetShop == null)
			{
				dbBetShop = new BetShop
				{
					PartnerId = betShopGroupPartnerId,
					GroupId = betShop.GroupId,
					Type = betShop.Type,
					Name = betShop.Name,
					CurrencyId = betShop.CurrencyId,
					Address = betShop.Address,
					RegionId = betShop.RegionId,
					State = betShop.State,
					DailyTicketNumber = betShop.DailyTicketNumber,
					DefaultLimit = betShop.DefaultLimit,
					CurrentLimit = betShop.CurrentLimit,
					SessionId = SessionId,
					CreationTime = currentTime,
					LastUpdateTime = currentTime,
					BonusPercent = betShop.BonusPercent,
					PrintLogo = betShop.PrintLogo,
                    UserId = betShop.UserId,
                    PaymentSystems = betShop.PaymentSystems
                };
				Db.BetShops.Add(dbBetShop);
				Db.SaveChanges();
			}
			else
			{
                var oldValue = JsonConvert.SerializeObject(dbBetShop.ToBetShopInfo());
				dbBetShop.PartnerId = betShopGroupPartnerId;
				dbBetShop.GroupId = betShop.GroupId;
				dbBetShop.Name = betShop.Name;
				dbBetShop.Address = betShop.Address;
				dbBetShop.RegionId = betShop.RegionId;
				dbBetShop.State = betShop.State;
				dbBetShop.BonusPercent = betShop.BonusPercent;
				dbBetShop.DefaultLimit = betShop.DefaultLimit;
				dbBetShop.PrintLogo = betShop.PrintLogo;
				dbBetShop.LastUpdateTime = currentTime;
                dbBetShop.Type = betShop.Type;
                dbBetShop.UserId = betShop.UserId;
                dbBetShop.PaymentSystems = betShop.PaymentSystems;

                SaveChangesWithHistory((int)ObjectTypes.BetShop, dbBetShop.Id, oldValue);
			}
            CacheManager.UpdateBetShopById(dbBetShop.Id);
            return dbBetShop;
        }

        public CashDesk SaveCashDesk(CashDesk cashDesk)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateCashDesk,
                ObjectTypeId = ObjectTypes.CashDesk,
                ObjectId = cashDesk.Id
            });
            var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
            if(betShop == null || betShop.Id == 0)
                throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);

            var currentTime = GetServerDate();
            var dbCashDesk = Db.CashDesks.FirstOrDefault(x => x.Id == cashDesk.Id);

			if (dbCashDesk == null)
			{
                if(!string.IsNullOrEmpty(cashDesk.MacAddress) && Db.CashDesks.Any(x => x.BetShop.PartnerId == betShop.PartnerId && x.MacAddress == cashDesk.MacAddress))
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                dbCashDesk = new CashDesk
				{
					BetShopId = cashDesk.BetShopId,
					Name = cashDesk.Name,
					State = cashDesk.State,
					MacAddress = cashDesk.MacAddress,
					EncryptPassword = cashDesk.EncryptPassword,
					EncryptSalt = cashDesk.EncryptSalt,
					EncryptIv = cashDesk.EncryptIv,
					SessionId = Identity.SessionId,
					CreationTime = currentTime,
					LastUpdateTime = currentTime,
					CurrentCashierId = 1,
                    Type = cashDesk.Type,
                    Restrictions = cashDesk.Restrictions
				};
				Db.CashDesks.Add(dbCashDesk);
			}
			else
			{
                if (!string.IsNullOrEmpty(cashDesk.MacAddress) && Db.CashDesks.Any(x => x.Id != dbCashDesk.Id && x.BetShop.PartnerId == betShop.PartnerId && x.MacAddress == cashDesk.MacAddress))
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                dbCashDesk.Name = cashDesk.Name;
				dbCashDesk.State = cashDesk.State;
				dbCashDesk.MacAddress = cashDesk.MacAddress;
				dbCashDesk.EncryptPassword = cashDesk.EncryptPassword;
				dbCashDesk.EncryptSalt = cashDesk.EncryptSalt;
				dbCashDesk.EncryptIv = cashDesk.EncryptIv;
                dbCashDesk.Type = cashDesk.Type;
                dbCashDesk.Restrictions = cashDesk.Restrictions;
			}
            Db.SaveChanges();
            CacheManager.DeleteCashDesk(dbCashDesk.Id);

            return dbCashDesk;
        }

        public void DeleteBetShopGroup(int id)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.DeleteBetShopGroup,
                ObjectTypeId = ObjectTypes.BetShopGroup,
                ObjectId = id
            });

            var betShopGroup = Db.BetShopGroups.FirstOrDefault(x => x.Id == id);
            if (betShopGroup == null)
                throw CreateException(LanguageId, Constants.Errors.BetShopGroupNotFound);

            if (!betShopGroup.IsLeaf ||
                Db.BetShopGroups.Any(x => x.ParentId == betShopGroup.Id && x.Id != betShopGroup.Id) ||
                Db.BetShops.Any(x => x.GroupId == betShopGroup.Id))
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);

            if (betShopGroup.ParentId.HasValue &&
                !Db.BetShopGroups.Any(x => x.ParentId == betShopGroup.ParentId.Value && x.Id != betShopGroup.Id))
            {
                var parentGroup = Db.BetShopGroups.FirstOrDefault(x => x.Id == betShopGroup.ParentId.Value);
                parentGroup.IsLeaf = true;
                SaveBetShopGroup(parentGroup);
            }
            Db.BetShopGroups.Remove(betShopGroup);

            SaveChanges();
        }

        public BetShopGroup GetBetShopGroupById(int id)
        {
            return Db.BetShopGroups.FirstOrDefault(x => x.Id == id);
        }

        public BetShop GetBetShopById(int id, bool checkPermission, int? userId = null)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewBetShop,
                    ObjectTypeId = ObjectTypes.BetShop,
                    UserId = userId
                });
                if (!checkPermissionResult.HaveAccessForAllObjects && !checkPermissionResult.AccessibleObjects.Contains(id))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return Db.BetShops.FirstOrDefault(x => x.Id == id);
        }

        private void CreateFilterForGetBetShops(FilterBetShop filter, bool checkPermission)
        {
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<BetShop>>();

            if (checkPermission)
            {
                var betshoppermishion = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewBetShop,
                    ObjectTypeId = ObjectTypes.BetShop
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                filter.CheckPermissionResuts.Add(new CheckPermissionOutput<BetShop>
                {
                    AccessibleObjects = betshoppermishion.AccessibleObjects,
                    HaveAccessForAllObjects = betshoppermishion.HaveAccessForAllObjects,
                    Filter = x => betshoppermishion.AccessibleObjects.Contains(x.Id)
                });

                filter.CheckPermissionResuts.Add(new CheckPermissionOutput<BetShop>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                });
            }
        }

        private void CreateFilterForGetfnBetShops(FilterfnBetShop filter)
        {
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShops>>();

            var betshoppermishion = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnBetShops>
            {
                AccessibleObjects = betshoppermishion.AccessibleObjects,
                HaveAccessForAllObjects = betshoppermishion.HaveAccessForAllObjects,
                Filter = x => betshoppermishion.AccessibleObjects.Contains(x.Id)
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnBetShops>
            {
                AccessibleObjects = partnerAccess.AccessibleObjects,
                HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
            });
        }

        private void CreateFilterForGetCashDesks(FilterCashDesk filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCashDesk,
                ObjectTypeId = ObjectTypes.CashDesk
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<CashDesk>>
            {
                new CheckPermissionOutput<CashDesk>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.Contains(x.Id)
                }
            };
        }

        private void CreateFilterfnForGetCashDesks(FilterfnCashDesk filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCashDesk,
                ObjectTypeId = ObjectTypes.CashDesk
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnCashDesks>>
            {
                new CheckPermissionOutput<fnCashDesks>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.Contains(x.Id)
                }
            };
        }

        private void CreateFilterGetBetShopGroups(FilterBetShopGroup filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShopGroup
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<BetShopGroup>>
            {
                new CheckPermissionOutput<BetShopGroup>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.Contains(x.Id)
                }
            };

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<BetShopGroup>
            {
                AccessibleObjects = partnerAccess.AccessibleObjects,
                HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
            });
        }

        public CashDeskShift CloseShift(IDocumentBll documentBl, IReportBll reportBl, int cashDeskId, int cashierId, long sessionId)
        {
            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                var cashDesk = Db.CashDesks.FirstOrDefault(x => x.Id == cashDeskId);
                if (cashDesk == null)
                    throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
                Db.sp_GetBetShopLock(cashDesk.BetShopId);
                var betShop = GetBetShopById(cashDesk.BetShopId, false);
                if (betShop == null)
                    throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);

                var currentDate = GetServerDate();
                var shift = Db.CashDeskShifts.Include(x => x.User).Include(x => x.CashDesk.BetShop).Where(x =>
                    x.CashDeskId == cashDeskId && x.CashierId == cashierId)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();
                if (shift != null)
                {

                    shift.BetShopId = betShop.Id;
                    shift.BetShopAddress = shift.CashDesk.BetShop.Address;

                    if (shift.State == (int)CashDeskShiftStates.Active)
                    {
                        var currentTime = GetServerDate();
                        var shiftInfo = Db.fn_ShiftReport(shift.StartTime, currentTime, shift.CashDeskId, shift.CashierId).FirstOrDefault();
                        var objectBalance = documentBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk, cashDeskId, betShop.CurrencyId);
                        var balance = objectBalance.AvailableBalance;

                        var bonusAmount = (shiftInfo == null || betShop.BonusPercent == null) ? 0 :
                            Math.Max(0, (shiftInfo.BetAmount * betShop.BonusPercent.Value + shiftInfo.DepositToInternetClient) / 100);
                        CreateDocumentForCloseShift(documentBl, balance, bonusAmount, betShop, cashDesk.Id, cashierId);

                        shift.State = (int)CashDeskShiftStates.Closed;
                        shift.EndAmount = balance;
                        shift.BonusAmount = bonusAmount;
                        shift.EndTime = currentDate;
                        if (cashDesk.CurrentShiftNumber == null)
                            cashDesk.CurrentShiftNumber = 1;
                        else
                            cashDesk.CurrentShiftNumber += 1;
                        shift.Number = cashDesk.CurrentShiftNumber;
                        if (shiftInfo != null)
                        {
                            shift.BetAmount = shiftInfo.BetAmount;
                            shift.PayedWinAmount = shiftInfo.PayedWin;
                            shift.DepositAmount = shiftInfo.DepositToInternetClient;
                            shift.WithdrawAmount = shiftInfo.WithdrawFromInternetClient;
                            shift.DebitCorrectionAmount = shiftInfo.DebitCorrectionOnCashDesk;
                            shift.CreditCorrectionAmount = shiftInfo.CreditCorrectionOnCashDesk;
                        }
                        if (sessionId > 0)
                        {
                            Db.UserSessions.Where(x => x.ParentId == sessionId).
                                  UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentDate });
                        }
                        Db.SaveChanges();
                    }
                    transactionScope.Complete();
                }
                
                return shift;
            }
        }

        private void CreateDocumentForCloseShift(IDocumentBll documentBl, decimal balance, decimal bonusAmount,
            BetShop betShop, int cashDeskId, int cashierId)
        {
            var operation = new Operation
            {
                Amount = Math.Abs(balance),
                CurrencyId = betShop.CurrencyId,
                Type = balance - bonusAmount > 0 ? (int)OperationTypes.CloseShiftCreditFromCashDesk
                                        : (int)OperationTypes.CloseShiftDebitToCashDesk,
                CashDeskId = cashDeskId,
                UserId = cashierId,
                OperationItems = new List<OperationItem>()
            };
            if (balance < 0)
            {
                var item = new OperationItem
                {
                    ObjectId = cashDeskId,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = Math.Abs(balance),
                    CurrencyId = betShop.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = operation.Type,
                    AccountTypeId = (int)Common.Enums.AccountTypes.CashDeskBalance
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    ObjectId = betShop.Id,
                    ObjectTypeId = (int)ObjectTypes.BetShop,
                    Amount = bonusAmount + Math.Abs(balance),
                    CurrencyId = betShop.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = operation.Type,
                    AccountTypeId = (int)Common.Enums.AccountTypes.BetShopBalance
                };
                operation.OperationItems.Add(item);
                if (bonusAmount > 0)
                {
                    item = new OperationItem
                    {
                        ObjectId = cashierId,
                        ObjectTypeId = (int)ObjectTypes.User,
                        Amount = bonusAmount,
                        CurrencyId = betShop.CurrencyId,
                        Type = (int)TransactionTypes.Debit,
                        OperationTypeId = (int)OperationTypes.TransferFromCashDeskToUser,
                        AccountTypeId = (int)Common.Enums.AccountTypes.UserBalance
                    };
                    operation.OperationItems.Add(item);
                }
            }
            else
            {
                var item = new OperationItem
                {
                    ObjectId = cashDeskId,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = balance,
                    CurrencyId = betShop.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = operation.Type,
                    AccountTypeId = (int)AccountTypes.CashDeskBalance
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    ObjectId = betShop.Id,
                    ObjectTypeId = (int)ObjectTypes.BetShop,
                    Amount = Math.Abs(balance - bonusAmount),
                    CurrencyId = betShop.CurrencyId,
                    Type = balance - bonusAmount > 0 ? (int)TransactionTypes.Debit : (int)TransactionTypes.Credit,
                    OperationTypeId = operation.Type,
                    AccountTypeId = (int)AccountTypes.BetShopBalance
                };
                operation.OperationItems.Add(item);
                if (bonusAmount > 0)
                {
                    item = new OperationItem
                    {
                        ObjectId = cashierId,
                        ObjectTypeId = (int)ObjectTypes.User,
                        Amount = bonusAmount,
                        CurrencyId = betShop.CurrencyId,
                        Type = (int)TransactionTypes.Debit,
                        OperationTypeId = (int)OperationTypes.TransferFromCashDeskToUser,
                        AccountTypeId = (int)Common.Enums.AccountTypes.UserBalance
                    };
                    operation.OperationItems.Add(item);
                }
            }
            documentBl.CreateDocument(operation);
        }

        public void CashierIncasation(int cashDeskId, decimal amount)
        {
            var cashDesk = Db.CashDesks.FirstOrDefault(x => x.Id == cashDeskId);
            if (cashDesk == null)
                throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);

            var betShop = GetBetShopById(cashDesk.BetShopId, true);
            if (betShop == null)
                throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != betShop.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var betShopPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });

            if (!betShopPermission.HaveAccessForAllObjects && betShopPermission.AccessibleObjects.All(x => x != cashDesk.BetShopId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var checkAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateCorrectionForCashDesk
            });
            if (!checkAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                Db.sp_GetBetShopLock(cashDesk.BetShopId);


                var shift = Db.CashDeskShifts.Where(x => x.Number == cashDesk.CurrentShiftNumber).FirstOrDefault();
                if (shift == null || shift.State != (int)CashDeskShiftStates.Active)
                    throw CreateException(LanguageId, Constants.Errors.ShiftNotFound);

                var operation = new Operation
                {
                    Amount = Math.Abs(amount),
                    CurrencyId = betShop.CurrencyId,
                    Type =
                        amount > 0 ? (int)OperationTypes.IncasationCreditFromCashDesk
                                    : (int)OperationTypes.IncasationDebitToCashDesk,
                    CashDeskId = cashDesk.Id,
                    OperationItems = new List<OperationItem>()
                };

                var item = new OperationItem
                {
                    ObjectId = cashDesk.Id,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = Math.Abs(amount),
                    CurrencyId = betShop.CurrencyId,
                    Type =
                        amount < 0 ? (int)TransactionTypes.Debit
                                    : (int)TransactionTypes.Credit,
                    OperationTypeId = operation.Type,
                    AccountTypeId = (int)AccountTypes.CashDeskBalance
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    ObjectId = betShop.Id,
                    ObjectTypeId = (int)ObjectTypes.BetShop,
                    Amount = Math.Abs(amount),
                    CurrencyId = betShop.CurrencyId,
                    Type =
                        amount < 0 ? (int)TransactionTypes.Credit
                                    : (int)TransactionTypes.Debit,
                    OperationTypeId = operation.Type,
                    AccountTypeId = (int)AccountTypes.BetShopBalance
                };
                operation.OperationItems.Add(item);

                using (var documentBl = new DocumentBll(Identity, Log))
                {
                    documentBl.CreateDocument(operation);
                    Db.SaveChanges();
                }
                transactionScope.Complete();
            }
        }

        public ObjectBalance UpdateShifts(IUserBll userBl, int cashierId, int cashDeskId)
        {
            using (var documentBl = new DocumentBll(this))
            {
                using (var reportBl = new ReportBll(this))
                {
                    var dbCashDesk = Db.CashDesks.Include(x => x.BetShop).First(x => x.Id == cashDeskId);
                    var oldShifts =
                        Db.CashDeskShifts.Where(
                            x =>
                                x.CashDeskId != cashDeskId && x.CashierId == cashierId &&
                                x.State == (int)CashDeskShiftStates.Active).ToList();
                    foreach (var s in oldShifts)
                    {
                        CloseShift(documentBl, reportBl, s.CashDeskId, cashierId, 0);
                    }

                    if (dbCashDesk.CurrentCashierId != cashierId)
                    {
                        var oldSession =
                            Db.UserSessions.Where(x => x.UserId == dbCashDesk.CurrentCashierId && x.CashDeskId != null && x.ProductId == null)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();
                        if (oldSession != null)
                        {
                            if (!string.IsNullOrEmpty(oldSession.Token))
                            {
                                var session = userBl.GetUserSession(oldSession.Token, false);
                                session.State = (int)SessionStates.Inactive;
                            }
                            CloseShift(documentBl, reportBl, dbCashDesk.Id, dbCashDesk.CurrentCashierId, oldSession.Id);
                        }
                    }

                    if (dbCashDesk.CurrentCashierId != cashierId || !HasActiveShift(dbCashDesk.Id))
                        OpenNewShift(dbCashDesk, cashierId);

                    var balance = userBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
                        cashDeskId, dbCashDesk.BetShop.CurrencyId);

                    return balance;
                }
            }
        }

        private void OpenNewShift(CashDesk dbCashDesk, int cashierId)
        {
            var shift = new CashDeskShift
            {
                CashierId = cashierId,
                StartTime = GetServerDate(),
                CashDeskId = dbCashDesk.Id,
                StartAmount = 0,
                State = (int)CashDeskShiftStates.Active,
                BetAmount = 0,
                PayedWinAmount = 0,
                DepositAmount = 0,
                WithdrawAmount = 0,
                DebitCorrectionAmount = 0,
                CreditCorrectionAmount = 0,
                BonusAmount = 0
            };
            Db.CashDeskShifts.Add(shift);
            dbCashDesk.CurrentCashierId = cashierId;
            Db.SaveChanges();
        }

        private bool HasActiveShift(int cashDeskId)
        {
            var shift =
               Db.CashDeskShifts.Where(
                   x =>
                       x.CashDeskId == cashDeskId && x.State == (int)CashDeskShiftStates.Active)
                   .OrderByDescending(x => x.Id)
                   .FirstOrDefault();
            return shift != null;
        }

        public void ChangeBetShopLimit(BetShop betShop, int userId)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ChangeBetShopLimit,
                ObjectTypeId = ObjectTypes.BetShop,
                ObjectId = betShop.Id
            });

            var betShopLimit = Db.BetShops.FirstOrDefault(x => x.Id == betShop.Id);
            
            if (betShopLimit != null)
            {
                var objectHistory = new ObjectDataChangeHistory
                {
                    UserId = userId,
                    ObjectId = betShop.Id,
                    ObjectTypeId = (int)ObjectTypes.BetShop,
                    FieldName = "CurrentLimit",
                    NumericValue = betShop.CurrentLimit,
                    CreationTime = GetServerDate()
                };
                Db.ObjectDataChangeHistories.Add(objectHistory);
                betShopLimit.CurrentLimit = betShop.CurrentLimit;
                Db.SaveChanges();
            }
        }

        public BetShopTicket GetBetShopBetByDocumentId(long documentId, bool isForPrint)
        {
            var result = Db.BetShopTickets.FirstOrDefault(x => x.DocumentId == documentId);
            if (isForPrint && result != null)
            {
                result.LastPrintTime = GetServerDate();
                result.NumberOfPrints += 1;
                Db.SaveChanges();
            }

            var document = Db.Documents.First(x => x.Id == documentId);
            result.ExternalTransactionId = document.ExternalTransactionId;
            return result;
        }

        #region Shift Report

        public AdminShiftReportOutput GetfnAdminShiftReportPaging(FilterAdminShift filter)
        {
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnAdminShiftReport>>();

            var betshoppermishion = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnAdminShiftReport>
            {
                AccessibleObjects = betshoppermishion.AccessibleObjects,
                HaveAccessForAllObjects = betshoppermishion.HaveAccessForAllObjects,
                Filter = x => betshoppermishion.AccessibleObjects.Contains(x.BetShopId)
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnAdminShiftReport>
            {
                AccessibleObjects = partnerAccess.AccessibleObjects,
                HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAdminShift
            });

            Func<IQueryable<fnAdminShiftReport>, IOrderedQueryable<fnAdminShiftReport>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnAdminShiftReport>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnAdminShiftReport>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = shiftReport => shiftReport.OrderByDescending(x => x.StartDate);
            }

		    var totals = (from s in filter.FilterObjects(Db.fn_AdminShiftReport())
		        group s by 1
		        into y
		        select new
		        {
                    TotalCount = y.Count(),
                    TotalAmount = y.Sum(x => x.EndAmount),
					TotalBonusAmount = y.Sum(x => x.BonusAmount),
                    TotalBetAmount = y.Sum(x=>x.BetAmount),
                    TotalPayedWinAmount = y.Sum(x => x.PayedWinAmount),
                    TotalDepositAmount = y.Sum(x => x.DepositAmount),
                    TotalWithdrawAmount = y.Sum(x => x.WithdrawAmount),
                    TotalDebitCorrectionAmount = y.Sum(x => x.DebitCorrectionAmount),
                    TotalCreditCorrectionAmount = y.Sum(x => x.CreditCorrectionAmount)
                }).FirstOrDefault();

            return new AdminShiftReportOutput
            {
                Entities = filter.FilterObjects(Db.fn_AdminShiftReport(), orderBy).ToList(),
                Count = totals == null ? 0 : totals.TotalCount,
                TotalAmount = totals == null ? 0 : totals.TotalAmount,
                TotalBonusAmount = totals == null ? 0 : totals.TotalBonusAmount,
                TotalBetAmount = totals == null ? 0 : totals.TotalBetAmount,
                TotalPayedWinAmount = totals == null ? 0 : totals.TotalPayedWinAmount,
                TotalDepositAmount = totals == null ? 0 : totals.TotalDepositAmount,
                TotalWithdrawAmount = totals == null ? 0 : totals.TotalWithdrawAmount,
                TotalDebitCorrectionAmount = totals == null ? 0 : totals.TotalDebitCorrectionAmount,
                TotalCreditCorrectionAmount = totals == null ? 0 : totals.TotalCreditCorrectionAmount
            };
        }

        public List<fnAdminShiftReport> GetfnAdminShiftReports(FilterAdminShift filter)
        {
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnAdminShiftReport>>();

            var betshoppermishion = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnAdminShiftReport>
            {
                AccessibleObjects = betshoppermishion.AccessibleObjects,
                HaveAccessForAllObjects = betshoppermishion.HaveAccessForAllObjects,
                Filter = x => betshoppermishion.AccessibleObjects.Contains(x.BetShopId)
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnAdminShiftReport>
            {
                AccessibleObjects = partnerAccess.AccessibleObjects,
                HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAdminShift
            });

			GetPermissionsToObject(new CheckPermissionInput
			{
				Permission = Constants.Permissions.ExportAdminShift
			});
			filter.TakeCount = 0;
			filter.SkipCount = 0;

			return filter.FilterObjects(Db.fn_AdminShiftReport(), shiftReport => shiftReport.OrderByDescending(x => x.StartDate)).ToList();
        }
        #endregion

        #region BetShopDocument

        // Debit Correction On CashDesk
        public Document CreateDebitCorrectionOnCashDesk(CashDeskCorrectionInput correction)
        {
            using (var documentBl = new DocumentBll(this))
            {
                CheckPermission(Constants.Permissions.CreateDebitCorrectionOnCashDesk);

                var cashDesk = CacheManager.GetCashDeskById(correction.CashDeskId);
                if (cashDesk == null)
                    throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
                var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
                if (betShop == null)
                    throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                betShop.CurrentLimit -= ConvertCurrency(correction.CurrencyId, betShop.CurrencyId, correction.Amount);
                if (betShop.CurrentLimit < 0)
                    throw CreateException(LanguageId, Constants.Errors.BetShopLimitExceeded);

                var operation = new Operation
                {
                    Type = (int)OperationTypes.DebitCorrectionOnCashDesk,
                    Creator = Identity.Id,
                    Info = correction.Info,
                    CashDeskId = correction.CashDeskId,
                    UserId = correction.CashierId,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    ExternalOperationId = correction.ExternalOperationId,
                    ExternalTransactionId = correction.ExternalTransactionId,
                    OperationItems = new List<OperationItem>()
                };
                var item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.BetShopBalance,
                    ObjectId = cashDesk.Id,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.DebitCorrectionOnCashDesk
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.PartnerBalance,
                    ObjectId = betShop.PartnerId,
                    ObjectTypeId = (int)ObjectTypes.Partner,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.DebitCorrectionOnCashDesk
                };
                operation.OperationItems.Add(item);
                var document = documentBl.CreateDocument(operation);
                Db.SaveChanges();
                return document;
            }
        }

        // Credit Correction On CashDesk
        public Document CreateCreditCorrectionOnCashDesk(CashDeskCorrectionInput correction)
        {
            using (var documentBl = new DocumentBll(this))
            {
                CheckPermission(Constants.Permissions.CreateCreditCorrectionOnCashDesk);

                var cashDesk = CacheManager.GetCashDeskById(correction.CashDeskId);
                if (cashDesk == null)
                    throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
                var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
                if (betShop == null)
                    throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                betShop.CurrentLimit += ConvertCurrency(correction.CurrencyId, betShop.CurrencyId, correction.Amount);
                if (betShop.CurrentLimit < 0)
                    throw CreateException(LanguageId, Constants.Errors.BetShopLimitExceeded);

                var operation = new Operation
                {
                    Type = (int)OperationTypes.CreditCorrectionOnCashDesk,
                    Creator = Identity.Id,
                    Info = correction.Info,
                    CashDeskId = correction.CashDeskId,
                    UserId = correction.CashierId,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    ExternalOperationId = correction.ExternalOperationId,
                    ExternalTransactionId = correction.ExternalTransactionId,
                    OperationItems = new List<OperationItem>()
                };
                var item = new OperationItem
                {
                    AccountTypeId = (int)Common.Enums.AccountTypes.BetShopBalance,
                    ObjectId = cashDesk.Id,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.CreditCorrectionOnCashDesk
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    AccountTypeId = (int)Common.Enums.AccountTypes.PartnerBalance,
                    ObjectId = betShop.PartnerId,
                    ObjectTypeId = (int)ObjectTypes.Partner,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.CreditCorrectionOnCashDesk
                };
                operation.OperationItems.Add(item);
                var document =documentBl.CreateDocument(operation);
                Db.SaveChanges();
                return document;
            }
        }

		public BetShopFinOperationsOutput CreateBetsFromBetShop(ListOfOperationsFromApi transactions, DocumentBll documentBl)
		{
			CheckPermission(Constants.Permissions.MakeBetFromBetShop);
			var response = new BetShopFinOperationsOutput { Documents = new List<BetShopFinOperationDocument>() };
			var operationTypeId = transactions.OperationTypeId ?? (int)OperationTypes.Bet;
			var product = CacheManager.GetProductByExternalId(transactions.GameProviderId, transactions.ExternalProductId);
			if (product == null)
				throw CreateException(LanguageId, Constants.Errors.ProductNotFound);

			if (documentBl.GetExistingDocumentId(transactions.GameProviderId, transactions.TransactionId, operationTypeId, product.Id) > 0)
				throw CreateException(LanguageId, Constants.Errors.TransactionAlreadyExists);
			using (var scope = CommonFunctions.CreateTransactionScope())
			{
				foreach (var operationItemFromProduct in transactions.OperationItems)
				{
					var cashDesk = CacheManager.GetCashDeskById(operationItemFromProduct.CashDeskId);
					if (cashDesk == null)
						throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
					Db.sp_GetBetShopLock(cashDesk.BetShopId);
					var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
					if (betShop == null)
						throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
					if (betShop.CurrentLimit < operationItemFromProduct.Amount)
						throw CreateException(LanguageId, Constants.Errors.BetShopLimitExceeded);
					if (betShop.State == Constants.CashDeskStates.Blocked)
						throw CreateException(LanguageId, Constants.Errors.BetShopBlocked);
					if (cashDesk.State == Constants.CashDeskStates.Blocked ||
						cashDesk.State == Constants.CashDeskStates.BlockedForWithdraw)
						throw CreateException(LanguageId, Constants.Errors.CashDeskBlocked);

					var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(betShop.PartnerId, product.Id);
					if (partnerProductSetting == null)
						throw CreateException(LanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
					if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
						throw CreateException(LanguageId, Constants.Errors.ProductBlockedForThisPartner);
                    
                    var partner = CacheManager.GetPartnerById(betShop.PartnerId);
                    ClientBll.CheckPartnerProductLimit(product.Id, partner, betShop.CurrencyId,
						operationItemFromProduct.Amount, LanguageId);

					var clientOperation = new ClientOperation
					{
						GameProviderId = transactions.GameProviderId,
						Amount = operationItemFromProduct.Amount,
						CurrencyId = betShop.CurrencyId,
						ClientId = operationItemFromProduct.CashierId,
						CashDeskId = operationItemFromProduct.CashDeskId,
						PartnerProductId = partnerProductSetting.Id,
						ProductId = product.Id,
						RoundId = transactions.RoundId,
						PossibleWin = operationItemFromProduct.PossibleWin,
						TypeId = transactions.TypeId,
						ExternalTransactionId = transactions.TransactionId,
						ExternalOperationId = transactions.ExternalOperationId,
						Info = transactions.Info,
						OperationTypeId = operationTypeId
					};
					var withdrawResponse = PlaceBetFromBetShop(clientOperation, documentBl).MapToBetShopFinOperationDocument();
					withdrawResponse.Type = operationItemFromProduct.Type;
					response.Documents.Add(withdrawResponse);
					var currentTime = DateTime.UtcNow;
					var bet = new BetShopTicket
					{
						DocumentId = withdrawResponse.Id,
						GameId = product.Id,
						BarCode = CommonFunctions.CalculateBarcode(withdrawResponse.Id),
						NumberOfPrints = 1,
						CreationTime = currentTime,
						LastPrintTime = currentTime
					};

					Db.BetShopTickets.Add(bet);
					Db.SaveChanges();

                    withdrawResponse.Barcode = bet.BarCode;
                }
				scope.Complete();
			}
			return response;
		}

		private BetShopFinOperationDocument PlaceBetFromBetShop(ClientOperation transaction, DocumentBll documentBl)
		{
			var currentDate = GetServerDate();
			var cashDesk = CacheManager.GetCashDeskById(transaction.CashDeskId);
			if (cashDesk == null)
				throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);

			Db.sp_GetBetShopLock(cashDesk.BetShopId);
			var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
			if (betShop == null)
				throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
			betShop.CurrentLimit -= ConvertCurrency(transaction.CurrencyId, betShop.CurrencyId, transaction.Amount);
			if (betShop.CurrentLimit < 0)
				throw CreateException(LanguageId, Constants.Errors.BetShopLimitExceeded);

			betShop.DailyTicketNumber++;
			var ticketNumber = Convert.ToInt64(string.Format("{0}{1}{2}", currentDate.ToString("yyMMdd"),
				string.Format("{0,5:0}", betShop.Id),
				string.Format("{0,4:0}", betShop.DailyTicketNumber)).Replace(" ", "0"));
			var operation = new Operation
			{
				Amount = transaction.Amount,
				CurrencyId = transaction.CurrencyId,
				UserId = transaction.ClientId,
				Type = transaction.OperationTypeId,
				ExternalTransactionId = transaction.ExternalTransactionId,
				ExternalOperationId = transaction.ExternalOperationId,
				TicketNumber = ticketNumber,
				GameProviderId = transaction.GameProviderId,
				PartnerProductId = transaction.PartnerProductId,
				ProductId = transaction.ProductId,
				RoundId = transaction.RoundId,
				PossibleWin = transaction.PossibleWin,
				DocumentTypeId = transaction.TypeId,
				Info = transaction.Info,
				CashDeskId = transaction.CashDeskId,
				OperationItems = new List<OperationItem>()
			};
            if (cashDesk.Type == (int)CashDeskTypes.Terminal)
            {
                var item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.TerminalBalance,
                    ObjectId = cashDesk.Id,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = transaction.OperationTypeId
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.BetShopBalance,
                    ObjectId = cashDesk.BetShopId,
                    ObjectTypeId = (int)ObjectTypes.BetShop,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = transaction.OperationTypeId
                };
                operation.OperationItems.Add(item);
            }
            else
            {
                var item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.ExternalClientsAccount,
                    ObjectId = Constants.MainExternalClientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = transaction.OperationTypeId
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.CashDeskBalance,
                    ObjectId = cashDesk.Id,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = transaction.OperationTypeId
                };
                operation.OperationItems.Add(item);
            }
			var document = documentBl.CreateDocument(operation);
			Db.SaveChanges();
			return document.MapToBetShopFinOperationDocument();
		}

        public Document CreateDepositToTerminal(PaymentRequest request)
        {
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                using (var documentBl = new DocumentBll(this))
                {
                    var cashDesk = CacheManager.GetCashDeskById(request.CashDeskId.Value);
                    if (cashDesk == null)
                        throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
                    var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
                    if (betShop == null)
                        throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                    betShop.CurrentLimit -= request.Amount;
                    if (betShop.CurrentLimit < 0)
                        throw CreateException(LanguageId, Constants.Errors.BetShopLimitExceeded);
                    Db.PaymentRequests.Add(request);
                    Db.SaveChanges();

                    var operation = new Operation
                    {
                        Type = (int)OperationTypes.Deposit,
                        Creator = Identity.Id,
                        CashDeskId = request.CashDeskId,
                        UserId = Identity.Id,
                        Amount = request.Amount,
                        CurrencyId = betShop.CurrencyId,
                        ExternalTransactionId = request.Id.ToString(),
                        OperationItems = new List<OperationItem>()
                    };
                    var item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.TerminalBalance,
                        ObjectId = cashDesk.Id,
                        ObjectTypeId = (int)ObjectTypes.CashDesk,
                        Amount = request.Amount,
                        CurrencyId = betShop.CurrencyId,
                        Type = (int)TransactionTypes.Debit,
                        OperationTypeId = (int)OperationTypes.Deposit
                    };
                    operation.OperationItems.Add(item);
                    item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.ExternalClientsAccount,
                        ObjectId = Constants.MainExternalClientId,
                        ObjectTypeId = (int)ObjectTypes.Client,
                        Amount = request.Amount,
                        CurrencyId = betShop.CurrencyId,
                        Type = (int)TransactionTypes.Credit,
                        OperationTypeId = (int)OperationTypes.Deposit
                    };
                    operation.OperationItems.Add(item);
                    var document = documentBl.CreateDocument(operation);
                    Db.SaveChanges();
                    scope.Complete();
                    return document;
                }
            }
        }

        public Document CreateWithdrawFromTerminal(PaymentRequest request)
        {
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                using (var documentBl = new DocumentBll(this))
                {
                    var cashDesk = CacheManager.GetCashDeskById(request.CashDeskId.Value);
                    if (cashDesk == null)
                        throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
                    var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
                    if (betShop == null)
                        throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                    
                    var balance = GetObjectBalance((int)ObjectTypes.CashDesk, cashDesk.Id).Balances.
                        Where(x => x.TypeId == (int)AccountTypes.TerminalBalance).Select(x => x.Balance).DefaultIfEmpty(0).Sum();
                    if (balance <= 0)
                        throw CreateException(LanguageId, Constants.Errors.WrongOperationAmount);
                    request.Amount = balance;
                    betShop.CurrentLimit += request.Amount;
                    Db.PaymentRequests.Add(request);
                    Db.SaveChanges();

                    var operation = new Operation
                    {
                        Type = (int)OperationTypes.PaymentRequestBooking,
                        Creator = Identity.Id,
                        CashDeskId = request.CashDeskId,
                        UserId = Identity.Id,
                        Amount = request.Amount,
                        CurrencyId = betShop.CurrencyId,
                        ExternalTransactionId = request.Id.ToString(),
                        OperationItems = new List<OperationItem>()
                    };
                    var item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.TerminalBalance,
                        ObjectId = cashDesk.Id,
                        ObjectTypeId = (int)ObjectTypes.CashDesk,
                        Amount = request.Amount,
                        CurrencyId = betShop.CurrencyId,
                        Type = (int)TransactionTypes.Credit,
                        OperationTypeId = (int)OperationTypes.PaymentRequestBooking
                    };
                    operation.OperationItems.Add(item);
                    item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.PartnerPaymentSettingBalance,
                        ObjectId = request.PartnerPaymentSettingId.Value,
                        ObjectTypeId = (int)ObjectTypes.PaymentSystem,
                        Amount = request.Amount,
                        CurrencyId = betShop.CurrencyId,
                        Type = (int)TransactionTypes.Debit,
                        OperationTypeId = (int)OperationTypes.PaymentRequestBooking
                    };
                    operation.OperationItems.Add(item);
                    var document = documentBl.CreateDocument(operation);
                    Db.SaveChanges();
                    scope.Complete();
                    return document;
                }
            }
        }

        public CashDesk GetCashDeskById (int id)
        {
            return Db.CashDesks.FirstOrDefault(x => x.Id == id);
        }

        public CashDeskShift GetShift(int cashDeskId, int? number)
        {
            if(number == null)
                return Db.CashDeskShifts.Where(x => x.CashDeskId == cashDeskId && x.State == (int)CashDeskShiftStates.Active).OrderByDescending(x => x.Id).FirstOrDefault();
            return Db.CashDeskShifts.Where(x => x.CashDeskId == cashDeskId && x.Number >= number).OrderBy(x => x.Number).FirstOrDefault();
        }

        public BetShopFinOperationsOutput CreateWinsToBetShop(ListOfOperationsFromApi transactions, DocumentBll documentBl)
		{
			var response = new BetShopFinOperationsOutput { Documents = new List<BetShopFinOperationDocument>() };
			var operationTypeId = transactions.OperationTypeId ?? (int)OperationTypes.Win;
			var product = transactions.ProductId == null ? CacheManager.GetProductByExternalId(transactions.GameProviderId, transactions.ExternalProductId) :
				CacheManager.GetProductById(transactions.ProductId.Value);

			if (product == null)
				throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
			if (documentBl.GetExistingDocumentId(transactions.GameProviderId, transactions.TransactionId, operationTypeId, product.Id) > 0)
				throw CreateException(LanguageId, Constants.Errors.TransactionAlreadyExists);

			using (var scope = CommonFunctions.CreateTransactionScope())
			{
				foreach (var operationItemFromProduct in transactions.OperationItems)
				{
					var cashDesk = CacheManager.GetCashDeskById(operationItemFromProduct.CashDeskId);
					if (cashDesk == null)
						throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
					var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
					if (betShop == null)
						throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
					if (cashDesk.State == Constants.CashDeskStates.Blocked ||
						cashDesk.State == Constants.CashDeskStates.BlockedForWithdraw)
						throw CreateException(LanguageId, Constants.Errors.CashDeskBlocked);
					var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(betShop.PartnerId, product.Id);
					if (partnerProductSetting == null)
						throw CreateException(LanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
					if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
						throw CreateException(LanguageId, Constants.Errors.ProductBlockedForThisPartner);

					long? parentDocumentId = null;
					//var provider = CacheManager.GetGameProviderById(transactions.GameProviderId);
					//if (provider == null)
					//    throw CreateException(LanguageId, Constants.Errors.WrongProviderId);
					//if (provider.Type == Constants.GameProviderType.CreditAndDebitConnected ||
					//    provider.Type == Constants.GameProviderType.Mixed)
					//{
					var creditTransaction = transactions.CreditTransactionId == null
						? null
						: documentBl.GetDocumentById(transactions.CreditTransactionId.Value);

					if (creditTransaction != null)
						parentDocumentId = creditTransaction.Id;
					else //if (provider.Type == Constants.GameProviderType.CreditAndDebitConnected)
						throw CreateException(LanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
					//}

					var clientOperation = new ClientOperation
					{
						ParentDocumentId = parentDocumentId,
						GameProviderId = transactions.GameProviderId,
						OperationTypeId = operationTypeId,
						Amount = operationItemFromProduct.Amount,
						CurrencyId = betShop.CurrencyId,
						ClientId = operationItemFromProduct.CashierId,
						CashDeskId = operationItemFromProduct.CashDeskId,
						PartnerProductId = partnerProductSetting.Id,
						ProductId = product.Id,
						RoundId = transactions.RoundId,
						ExternalTransactionId = transactions.TransactionId,
						ExternalOperationId = transactions.ExternalOperationId,
						Info = transactions.Info
					};
					response.Documents.Add(CreateWinToBetShop(clientOperation, documentBl));
				}
				scope.Complete();
			}
			return response;
		}

		private BetShopFinOperationDocument CreateWinToBetShop(ClientOperation transaction, DocumentBll documentBl)
		{
			var cashDesk = CacheManager.GetCashDeskById(transaction.CashDeskId);
			if (cashDesk == null)
				throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
			var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
			if (betShop == null)
				throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
			var operation = new Operation
			{
				Amount = transaction.Amount,
				CurrencyId = transaction.CurrencyId,
				Type = transaction.OperationTypeId,
				ExternalTransactionId = transaction.ExternalTransactionId,
				ExternalOperationId = transaction.ExternalOperationId,
				CashDeskId = transaction.CashDeskId,
				ParentId = transaction.ParentDocumentId,
				ProductId = transaction.ProductId,
				RoundId = transaction.RoundId,
				Info = transaction.Info,
				UserId = transaction.ClientId,
				PartnerProductId = transaction.PartnerProductId,
				GameProviderId = transaction.GameProviderId,
				State = (transaction.Amount == 0 ? (int)BetDocumentStates.Lost : (int)BetDocumentStates.Won),
				OperationItems = new List<OperationItem>()
			};
			var item = new OperationItem
			{
				AccountTypeId = (int)Common.Enums.AccountTypes.PartnerBalance,
				ObjectId = betShop.PartnerId,
				ObjectTypeId = (int)ObjectTypes.Partner,
				Amount = transaction.Amount,
				CurrencyId = transaction.CurrencyId,
				Type = (int)TransactionTypes.Credit,
				OperationTypeId = transaction.OperationTypeId
			};
			operation.OperationItems.Add(item);
			item = new OperationItem
			{
				ObjectId = betShop.PartnerId,
				ObjectTypeId = (int)ObjectTypes.Partner,
				AccountTypeId = (int)Common.Enums.AccountTypes.PartnerBalance,
				Amount = transaction.Amount,
				CurrencyId = transaction.CurrencyId,
				Type = (int)TransactionTypes.Debit,
				OperationTypeId = transaction.OperationTypeId
			};
			operation.OperationItems.Add(item);
			var document = documentBl.CreateDocument(operation);
			Db.SaveChanges();
			return document.MapToBetShopFinOperationDocument();
		}
		// pay win from BetShop (take win amount from BetShop unpaid wins account)
		public Document PayWinFromBetShop(ClientOperation transaction, DocumentBll documentBl)
		{
			CheckPermission(Constants.Permissions.PayWinFromBetShop);
			using (var scope = CommonFunctions.CreateTransactionScope())
			{
				Document betDocument = null;
				var cashDesk = CacheManager.GetCashDeskById(transaction.CashDeskId);
				if (cashDesk == null)
					throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);

				Db.sp_GetBetShopLock(cashDesk.BetShopId);
				if (transaction.ParentDocumentId.HasValue)
					betDocument = Db.Documents.FirstOrDefault(x => x.Id == transaction.ParentDocumentId.Value);
				if (betDocument == null || betDocument.CashDeskId == null)
					throw CreateException(LanguageId, Constants.Errors.DocumentNotFound);

				if (betDocument.CashDeskId != cashDesk.Id)
				{
					var betCashDesk = CacheManager.GetCashDeskById(betDocument.CashDeskId.Value);
					if (betCashDesk.BetShopId != cashDesk.BetShopId)
						throw CreateException(LanguageId, Constants.Errors.DocumentNotFound);
				}
				var documents =
					Db.Documents.Where(x => x.ParentId == betDocument.Id).ToList();
				var winDocument = documents.FirstOrDefault(x => x.OperationTypeId == (int)OperationTypes.Win && x.State != (int)BetDocumentStates.Deleted);
				if (winDocument == null || !winDocument.CashDeskId.HasValue)
					throw CreateException(LanguageId, Constants.Errors.WrongParentDocument);

				if (documents.Any(x => x.State == (int)BetDocumentStates.Paid))
					throw CreateException(LanguageId, Constants.Errors.PaymentRequestAlreadyPayed);

				var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
				if (betShop == null)
					throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);

				var partner = CacheManager.GetPartnerById(betShop.PartnerId);
				if ((DateTime.UtcNow - winDocument.CreationTime).Hours > partner.UnpaidWinValidPeriod * 24)
					throw CreateException(LanguageId, Constants.Errors.TicketNotFound);

				var amount = documents.Sum(x => x.Amount);
				betShop.CurrentLimit += ConvertCurrency(winDocument.CurrencyId, betShop.CurrencyId, amount);

				var operation = new Operation
				{
					Amount = amount,
					CurrencyId = winDocument.CurrencyId,
					Type = (int)OperationTypes.PayWinFromBetshop,
					ExternalTransactionId = transaction.ExternalTransactionId,
					UserId = transaction.ClientId,
					CashDeskId = transaction.CashDeskId,
					ParentId = winDocument.ParentId,
					State = (int)BetDocumentStates.Uncalculated,
					OperationItems = new List<OperationItem>()
				};
				var item = new OperationItem
				{
					AccountTypeId = (int)AccountTypes.ExternalClientsAccount,
					ObjectId = Constants.MainExternalClientId,
					ObjectTypeId = (int)ObjectTypes.Client,
					Amount = amount,
					CurrencyId = winDocument.CurrencyId,
					Type = (int)TransactionTypes.Debit,
					OperationTypeId = (int)OperationTypes.PayWinFromBetshop
                };
				operation.OperationItems.Add(item);
				item = new OperationItem
				{
					AccountTypeId = (int)Common.Enums.AccountTypes.CashDeskBalance,
					ObjectId = transaction.CashDeskId,
					ObjectTypeId = (int)ObjectTypes.CashDesk,
					Amount = amount,
					CurrencyId = winDocument.CurrencyId,
					Type = (int)TransactionTypes.Credit,
					OperationTypeId = (int)OperationTypes.PayWinFromBetshop
                };
				operation.OperationItems.Add(item);
				var document = documentBl.CreateDocument(operation);
				winDocument.State = (int)BetDocumentStates.Paid;
				betDocument.State = (int)BetDocumentStates.Paid;
				foreach (var doc in documents)
				{
					doc.State = (int)BetDocumentStates.Paid;
				}
				Db.SaveChanges();
				scope.Complete();
				return document;
			}
		}

        public Document GetDocumentByExternalId(string externalTransactionId, int productId, int gameProviderId, long? parentId, int operationTypeId)
        {
            return Db.Documents.FirstOrDefault(x => x.ExternalTransactionId == externalTransactionId && x.OperationTypeId == operationTypeId
                && x.GameProviderId == gameProviderId && x.ProductId == productId && x.ParentId == parentId);
        }

        public Document GetDocumentByRoundId(int operationTypeId, string roundId, int providerId, int userId)
        {
            return Db.Documents.FirstOrDefault(x => x.GameProviderId == providerId && x.OperationTypeId == operationTypeId && x.RoundId == roundId && x.UserId == userId);
        }

        #endregion

        public CashDesk GetCashDeskByMacAddress(int partnerId, string externalId)
        {
            return Db.CashDesks.FirstOrDefault(x => x.BetShop.PartnerId == partnerId && x.MacAddress == externalId);
        }

        public static bool GetBetShopUsePin(int shopId)
        {
            var betShop = CacheManager.GetBetShopById(shopId);
            if (betShop.UsePin != null)
                return betShop.UsePin.Value;

            var group = CacheManager.GetBetShopGroupById(betShop.GroupId);

            while (true)
            {
                if (group.UsePin != null)
                    return group.UsePin.Value;
                if (group.ParentId == null)
                    return true;
                group = CacheManager.GetBetShopGroupById(group.ParentId.Value);
            }
        }

        public static bool GetBetShopAllowLive(int shopId)
        {
            var betShop = CacheManager.GetBetShopById(shopId);
            if (betShop.AllowLive != null)
                return betShop.AllowLive.Value;

            var group = CacheManager.GetBetShopGroupById(betShop.GroupId);

            while (true)
            {
                if (group.AllowLive != null)
                    return group.AllowLive.Value;
                if (group.ParentId == null)
                    return true;
                group = CacheManager.GetBetShopGroupById(group.ParentId.Value);
            }
        }

        public static bool GetBetShopAllowCashout(int shopId)
        {
            var betShop = CacheManager.GetBetShopById(shopId);
            if (betShop.AllowCashout != null)
                return betShop.AllowCashout.Value;

            var group = CacheManager.GetBetShopGroupById(betShop.GroupId);

            while (true)
            {
                if (group.AllowCashout != null)
                    return group.AllowCashout.Value;
                if (group.ParentId == null)
                    return true;
                group = CacheManager.GetBetShopGroupById(group.ParentId.Value);
            }
        }

        public static bool GetBetShopAllowAnonymousBet(int shopId)
        {
            var betShop = CacheManager.GetBetShopById(shopId);
            if (betShop.AnonymousBet != null)
                return betShop.AnonymousBet.Value;

            var group = CacheManager.GetBetShopGroupById(betShop.GroupId);

            while (true)
            {
                if (group.AnonymousBet != null)
                    return group.AnonymousBet.Value;
                if (group.ParentId == null)
                    return true;
                group = CacheManager.GetBetShopGroupById(group.ParentId.Value);
            }
        }
    }
}