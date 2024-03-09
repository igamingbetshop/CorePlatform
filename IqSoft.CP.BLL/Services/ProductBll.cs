using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using log4net;
using System.Data.Entity;
using IqSoft.CP.BLL.Interfaces;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.Common.Models.Products;
using ProductCategory = IqSoft.CP.DAL.ProductCategory;
using System.Threading.Tasks;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.AdminModels;

namespace IqSoft.CP.BLL.Services
{
    public class ProductBll : PermissionBll, IProductBll
    {
        #region Constructors

        public ProductBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public ProductBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Product SaveProduct(fnProduct product, string comment, FtpModel model, out List<int> partners)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateProduct,
                ObjectTypeId = ObjectTypes.Product,
                ObjectId = product.Id
            });
            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(product.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbProduct = Db.Products.Include(x => x.PartnerProductSettings).FirstOrDefault(x => x.Id == product.Id);
            if (product.IsNewObject && dbProduct != null)
                throw CreateException(LanguageId, Constants.Errors.ProductAlreadyExists);
            if (!product.IsNewObject && dbProduct == null)
                throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
            var currentDate = DateTime.UtcNow;

            if((product.GameProviderId.HasValue && CacheManager.GetGameProviderById(product.GameProviderId.Value) == null) ||
                (product.SubproviderId.HasValue && CacheManager.GetGameProviderById(product.SubproviderId.Value) == null))
                throw CreateException(LanguageId, Constants.Errors.WrongProviderId);

            if (dbProduct == null)
            {
                partners = new List<int>();
                var t = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Product,
                    Text = product.Name,
                    LanguageId = Constants.DefaultLanguageId
                });
                var parent = CacheManager.GetProductById(product.ParentId ?? 0);
                var newId = product.Id;
                if (newId == 0)
                {
                    var maxId = Db.Products.Where(x => x.Id < 1000).OrderByDescending(x => x.Id).FirstOrDefault();
                    newId = maxId == null ? 0 : maxId.Id + 1;
                }

                dbProduct = new Product
                {
                    Id = newId,
                    GameProviderId = product.GameProviderId,
                    PaymentSystemId = null,
                    Level = parent.Level + 1,
                    NickName = product.NickName,
                    ParentId = product.ParentId,
                    ExternalId = product.ExternalId,
                    State = product.State,
                    IsForDesktop = product.IsForDesktop,
                    IsForMobile = product.IsForMobile,
                    WebImageUrl = product.WebImageUrl,
                    MobileImageUrl = product.MobileImageUrl,
                    BackgroundImageUrl = product.BackgroundImageUrl,
                    SubproviderId = product.SubproviderId,
                    HasDemo = product.HasDemo,
                    Jackpot = product.Jackpot,
                    FreeSpinSupport = product.FreeSpinSupport,
                    Translation = t,
                    CategoryId = product.CategoryId,
                    RTP = product.RTP,
                    CreationTime = currentDate,
                    LastUpdateTime = currentDate
                };
                Db.Products.Add(dbProduct);
                Db.SaveChanges();
                if (dbProduct.ParentId == null)
                    dbProduct.Path = "/" + dbProduct.Id + "/";
                else
                {
                    var parentProduct = Db.Products.First(x => x.Id == dbProduct.ParentId);
                    dbProduct.Path += parentProduct.Path + dbProduct.Id + "/";
                }
                Db.SaveChanges();
            }
            else
            {
                if (product.State != (int)ProductStates.Active && product.State != (int)ProductStates.Inactive)
                    product.State = (int)ProductStates.Active;

                var isParentChanged = dbProduct.ParentId != product.ParentId;

                var oldValue = dbProduct.ToProductInfo(LanguageId);
                dbProduct.GameProviderId = product.GameProviderId;
                dbProduct.NickName = product.NickName;
                dbProduct.ParentId = product.ParentId;
                dbProduct.ExternalId = product.ExternalId;
                dbProduct.State = product.State;
                dbProduct.IsForDesktop = product.IsForDesktop;
                dbProduct.IsForMobile = product.IsForMobile;
                dbProduct.WebImageUrl = product.WebImageUrl;
                dbProduct.MobileImageUrl = product.MobileImageUrl;
                dbProduct.BackgroundImageUrl = product.BackgroundImageUrl;
                dbProduct.SubproviderId = product.SubproviderId;
                dbProduct.HasDemo = product.HasDemo;
                dbProduct.Jackpot = product.Jackpot;
                dbProduct.FreeSpinSupport = product.FreeSpinSupport;
                dbProduct.CategoryId = product.CategoryId;
                dbProduct.RTP = product.RTP;
                dbProduct.LastUpdateTime = currentDate;
                if (product.ProductCountrySettings == null || !product.ProductCountrySettings.Any())
                    Db.ProductCountrySettings.Where(x => x.ProductId == product.Id).DeleteFromQuery();
                else
                {
                    var type = product.ProductCountrySettings.First().Type;
                    var countries = product.ProductCountrySettings.Select(x => x.CountryId).ToList();
                    Db.ProductCountrySettings.Where(x => x.ProductId == dbProduct.Id && (x.Type != type || !countries.Contains(x.CountryId))).DeleteFromQuery();
                    var dbCountries = Db.ProductCountrySettings.Where(x => x.ProductId == dbProduct.Id).Select(x => x.CountryId).ToList();
                    countries.RemoveAll(x => dbCountries.Contains(x));
                    foreach (var c in countries)
                        Db.ProductCountrySettings.Add(new ProductCountrySetting { ProductId = dbProduct.Id, CountryId = c, Type = type });
                }
                SaveChangesWithHistory((int)ObjectTypes.Product, dbProduct.Id, JsonConvert.SerializeObject(oldValue), comment);
                if (isParentChanged)
                {
                    int parentLevel = 0;
                    if (dbProduct.ParentId.HasValue)
                    {
                        var parentProduct = CacheManager.GetProductById(dbProduct.ParentId.Value);
                        parentLevel = parentProduct.Level;
                    }
                    dbProduct.Level = ++parentLevel;
                    ChangeProductPaths(dbProduct);
                    Db.SaveChanges();

                    if (dbProduct.ParentId == null)
                        dbProduct.Path = "/" + dbProduct.Id + "/";
                    else
                    {
                        var parentProduct = Db.Products.First(x => x.Id == dbProduct.ParentId);
                        dbProduct.Path += parentProduct.Path + dbProduct.Id + "/";
                    }
                    Db.SaveChanges();
                }
                CacheManager.DeleteProductFromCache(dbProduct.Id);
                partners = Db.PartnerProductSettings.Where(x => x.ProductId == dbProduct.Id && x.State == (int)ProductStates.Active)
                                                    .Select(x => x.PartnerId).Distinct().ToList();

                var provider = CacheManager.GetGameProviderById(dbProduct.SubproviderId ?? dbProduct.GameProviderId ?? 0);

                try
                {
                    if (!string.IsNullOrEmpty(product.WebImage))
                        UploadProductImage(product.Id, product.WebImage, provider?.Name.ToLower() + "/web", model);

                    if (!string.IsNullOrEmpty(product.BackgroundImage))
                        UploadProductImage(product.Id, product.BackgroundImage, provider?.Name.ToLower() + "/background", model);

                    if (!string.IsNullOrEmpty(product.MobileImage))
                        UploadProductImage(product.Id, product.MobileImage, provider?.Name.ToLower() + "/mob", model);
                }
                catch(Exception e)
                {
                    Log.Error($"ftp://{model.Url}/resources/products/{provider?.Name}/{product.Id}.png");
                    Log.Error(e);
                }
            }

            return dbProduct;
        }

        public void SaveProductsCountrySetting(List<int> productIds, List<ProductCountrySetting> productCountrySettings, int? state, out List<int> partners)
        {
            CheckPermission(Constants.Permissions.ViewProduct);
            CheckPermission(Constants.Permissions.CreateProduct);
            if (state.HasValue && !Enum.IsDefined(typeof(ProductStates), state.Value))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            if (productCountrySettings == null || !productCountrySettings.Any())
                Db.ProductCountrySettings.Where(x => productIds.Contains(x.ProductId)).DeleteFromQuery();


            var dbProducts = Db.Products.Include(x => x.ProductCountrySettings).Where(x => productIds.Contains(x.Id)).ToList();
            foreach (var product in dbProducts)
            {
                var oldValue = product.ToProductInfo(LanguageId);
                if (state.HasValue)
                    product.State = state.Value;
                if (productCountrySettings != null && productCountrySettings.Any())
                {
                    var type = productCountrySettings.First().Type;
                    var countries = productCountrySettings.Select(x => x.CountryId).ToList();
                    Db.ProductCountrySettings.Where(x => x.ProductId == product.Id && (x.Type != type || !countries.Contains(x.CountryId))).DeleteFromQuery();
                    var dbCountries = Db.ProductCountrySettings.Where(x => x.ProductId == product.Id).Select(x => x.CountryId).ToList();
                    countries.RemoveAll(x => dbCountries.Contains(x));
                    foreach (var c in countries)
                        Db.ProductCountrySettings.Add(new ProductCountrySetting { ProductId = product.Id, CountryId = c, Type = type });
                }
                SaveChangesWithHistory((int)ObjectTypes.Product, product.Id, JsonConvert.SerializeObject(oldValue), "Bulk edit");
                CacheManager.DeleteProductFromCache(product.Id);
            }

            partners = Db.PartnerProductSettings.Where(x => productIds.Contains(x.ProductId) && x.State == (int)ProductStates.Active)
                                                .Select(x => x.PartnerId).Distinct().ToList();

        }

        private void UploadProductImage(int productId,  string image, string folder, FtpModel model)
        {
            var path = $"ftp://{model.Url}/resources/products/{folder}/{productId}.png";
            byte[] bytes = Convert.FromBase64String(image);
            UploadFtpImage(bytes, model, path);
        }
        private void ChangeProductPaths(Product product)
        {
            var childProducts = Db.Products.Where(x => x.ParentId == product.Id).ToList();
            foreach (var childProduct in childProducts)
            {
                ChangeProductPaths(childProduct);
            }
        }

        public fnProduct GetfnProductById(int id, bool checkPermission, string languageId = null)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewProduct,
                    ObjectTypeId = ObjectTypes.Product,
                    ObjectId = id
                });
                if (!checkPermissionResult.HaveAccessForAllObjects &&
                    !checkPermissionResult.AccessibleObjects.Contains(id))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }

            var langId = languageId ?? LanguageId;
            var product = Db.fn_Product(langId).FirstOrDefault(x => x.Id == id);
            product.ProductCountrySettings = Db.ProductCountrySettings.Where(x => x.ProductId == id).ToList();
            return product;
        }
        
        public PagedModel<fnProduct> GetFnProducts(FilterfnProduct filter, bool checkPermission = true)
        {
            if (checkPermission)
            {
                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewProduct,
                    ObjectTypeId = ObjectTypes.Product
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnProduct>>
                {
                    new CheckPermissionOutput<fnProduct>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x=> checkP.AccessibleObjects.Contains(x.ObjectId)
                    }
                };
            }

            Func<IQueryable<fnProduct>, IOrderedQueryable<fnProduct>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnProduct>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnProduct>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = products => products.OrderByDescending(x => x.Id);
            }
            var languageId = String.IsNullOrEmpty(LanguageId) ? Constants.DefaultLanguageId : LanguageId;
            return new PagedModel<fnProduct>
            {
                Entities = filter.FilterObjects(Db.fn_Product(languageId), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_Product(languageId))
            };
        }

        public PagedModel<fnProduct> GetPartnerProducts(int partnerId, FilterfnProduct filter)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewProduct,
                ObjectTypeId = ObjectTypes.Product
            });
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && !checkPartnerPermission.AccessibleObjects.Contains(partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnProduct>>
            {
                new CheckPermissionOutput<fnProduct>
                {
                    AccessibleObjects = checkPermissionResult.AccessibleObjects,
                    HaveAccessForAllObjects = checkPermissionResult.HaveAccessForAllObjects,
                    Filter = x => checkPermissionResult.AccessibleObjects.Contains(x.ObjectId)
                }
            };
            Func<IQueryable<fnProduct>, IOrderedQueryable<fnProduct>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnProduct>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnProduct>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = products => products.OrderByDescending(x => x.Id);
            }
            var languageId = string.IsNullOrEmpty(LanguageId) ? Constants.DefaultLanguageId : LanguageId;

            var res = from p in Db.fn_Product(languageId)
                      where p.GameProviderId != null
                      join pps in Db.PartnerProductSettings.Where(x => x.PartnerId == partnerId)
                      on p.Id equals pps.ProductId
                      into pp
                      from x in pp.DefaultIfEmpty()
                      where x == null
                      select p;

            return new PagedModel<fnProduct>
            {
                Entities = filter.FilterObjects(res, orderBy).ToList(),
                Count = filter.SelectedObjectsCount(res)
            };
        }

        public PagedModel<fnProduct> ExportPartnerProducts(int partnerId, FilterfnProduct filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportPartnerProductSetting
            });
            return GetPartnerProducts(partnerId, filter);
        }

        public List<Product> GetProducts(FilterProduct filter, bool checkPermission = true)
        {
            if (checkPermission)
            {
                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewProduct,
                    ObjectTypeId = ObjectTypes.Product
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<Product>>
                {
                    new CheckPermissionOutput<Product>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x=> checkP.AccessibleObjects.Contains(x.ObjectId)
                    }
                };
            }
            return filter.FilterObjects(Db.Products).ToList();
        }

        public List<PartnerProductSetting> GetPartnerProductSettings(FilterPartnerProductSetting filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartnerProductSetting,
                ObjectTypeId = ObjectTypes.PartnerProductSetting
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<PartnerProductSetting>>
                {
                    new CheckPermissionOutput<PartnerProductSetting>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x => checkP.AccessibleObjects.Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<PartnerProductSetting>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                    }
                };
            return
                filter.FilterObjects(Db.PartnerProductSettings).ToList();
        }

        public PagedModel<fnPartnerProductSetting> GetfnPartnerProductSettings(FilterfnPartnerProductSetting filter, bool checkPermission)
        {
            if (checkPermission)
            {
                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartnerProductSetting,
                    ObjectTypeId = ObjectTypes.PartnerProductSetting
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPartnerProductSetting>>
                {
                    new CheckPermissionOutput<fnPartnerProductSetting>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x => checkP.AccessibleObjects.Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<fnPartnerProductSetting>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                    }
                };
            }
            Func<IQueryable<fnPartnerProductSetting>, IOrderedQueryable<fnPartnerProductSetting>> orderBy;

            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnPartnerProductSetting>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = products => products.OrderByDescending(x => x.Id);
            return new PagedModel<fnPartnerProductSetting>
            {
                Entities = filter.FilterObjects(Db.fn_PartnerProductSetting(LanguageId), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_PartnerProductSetting(LanguageId))
            };
        }

        public PagedModel<fnPartnerProductSetting> ExportfnPartnerProductSettings(FilterfnPartnerProductSetting filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportPartnerProductSetting
            });
            return GetfnPartnerProductSettings(filter, true);
        }

        public List<PartnerProductSetting> SavePartnerProductSettings(ApiPartnerProductSettingInput apiPartnerProductSetting, bool checkPermission = true)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreatePartnerProductSetting,
                    ObjectTypeId = ObjectTypes.Product
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!checkPermissionResult.HaveAccessForAllObjects ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != apiPartnerProductSetting.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            if (apiPartnerProductSetting.Volatility.HasValue && !Enum.IsDefined(typeof(VolatilityTypes), apiPartnerProductSetting.Volatility.Value))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var dbProductCategories = GetProductCategories(checkPermission).Select(x => x.Id).ToList();
            if (apiPartnerProductSetting.Rating < 0 || apiPartnerProductSetting.Percent < 0 || apiPartnerProductSetting.OpenMode <= 0 ||
                apiPartnerProductSetting.CategoryIds.Any(x => !dbProductCategories.Contains(x)))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var currentDate = DateTime.UtcNow;
            foreach(var productId in apiPartnerProductSetting.ProductIds)
            {
                var dbProd = Db.PartnerProductSettings.FirstOrDefault(x => x.PartnerId == apiPartnerProductSetting.PartnerId && x.ProductId == productId);
                if(dbProd == null)
                {
                    var product = CacheManager.GetProductById(productId) ?? throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
                    var newItem = new PartnerProductSetting
                    {
                        PartnerId = apiPartnerProductSetting.PartnerId,
                        ProductId = productId,
                        State = apiPartnerProductSetting.State ?? (int)PartnerProductSettingStates.Active,
                        RTP = apiPartnerProductSetting.RTP,
                        OpenMode = apiPartnerProductSetting.OpenMode,
                        Rating = apiPartnerProductSetting.Rating,
                        CategoryIds = apiPartnerProductSetting.CategoryIds != null ? JsonConvert.SerializeObject(apiPartnerProductSetting.CategoryIds) : null,
                        Volatility = apiPartnerProductSetting.Volatility,
                        Percent = apiPartnerProductSetting.Percent ?? 0,
                        HasDemo = apiPartnerProductSetting.HasDemo,
                        CreationTime = currentDate,
                        LastUpdateTime = currentDate
                    };
                    Db.PartnerProductSettings.Add(newItem);
                    Db.SaveChanges();
                }
                else
                {
                    var oldValue = new
                    {
                        dbProd.Id,
                        dbProd.PartnerId,
                        dbProd.ProductId,
                        dbProd.Percent,
                        dbProd.State,
                        dbProd.Rating,
                        dbProd.CategoryIds,
                        dbProd.OpenMode,
                        dbProd.HasDemo,
                        dbProd.RTP,
                        dbProd.Volatility,
                        dbProd.CreationTime,
                        dbProd.LastUpdateTime
                    };

                    if (apiPartnerProductSetting.State.HasValue)
                        dbProd.State = apiPartnerProductSetting.State.Value;
                    if (apiPartnerProductSetting.Percent.HasValue)
                        dbProd.Percent = apiPartnerProductSetting.Percent.Value;
                    if (apiPartnerProductSetting.Rating.HasValue)
                        dbProd.Rating = apiPartnerProductSetting.Rating;
                    if (apiPartnerProductSetting.RTP.HasValue)
                        dbProd.RTP = apiPartnerProductSetting.RTP;
                    if (apiPartnerProductSetting.Volatility.HasValue)
                        dbProd.Volatility = apiPartnerProductSetting.Volatility;
                    
                    dbProd.CategoryIds = apiPartnerProductSetting.CategoryIds != null ? JsonConvert.SerializeObject(apiPartnerProductSetting.CategoryIds) : null;
                    if (apiPartnerProductSetting.OpenMode.HasValue)
                        dbProd.OpenMode = apiPartnerProductSetting.OpenMode;
                    if (apiPartnerProductSetting.HasDemo.HasValue)
                        dbProd.HasDemo = apiPartnerProductSetting.HasDemo;
                    dbProd.LastUpdateTime = currentDate;
                    SaveChangesWithHistory((int)ObjectTypes.PartnerProductSetting, dbProd.Id, JsonConvert.SerializeObject(oldValue), string.Empty);
                }                
            }
            return Db.PartnerProductSettings.Where(x => x.PartnerId == apiPartnerProductSetting.PartnerId &&
                                                        apiPartnerProductSetting.ProductIds.Contains(x.ProductId)).ToList();
        }

        public void RemovePartnerProductSettings(ApiPartnerProductSettingInput apiPartnerProductSetting)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerProductSetting,
                ObjectTypeId = ObjectTypes.Product
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!checkPermissionResult.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != apiPartnerProductSetting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            Db.PartnerProductSettings.Where(x => apiPartnerProductSetting.ProductIds.Contains(x.ProductId) &&
                                                 x.PartnerId == apiPartnerProductSetting.PartnerId).DeleteFromQuery();
        }

        public PagedModel<fnPartnerProductSetting> CopyPartnerProductSetting(int fromPartnerId, int toPartnerId)
        {
            var checkEditPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerProductSetting,
                ObjectTypeId = ObjectTypes.Product
            });

            var checkViewPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartnerProductSetting,
                ObjectTypeId = ObjectTypes.Product
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!checkEditPermission.HaveAccessForAllObjects || !checkViewPermission.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && (!partnerAccess.AccessibleObjects.Contains(fromPartnerId) ||
                !partnerAccess.AccessibleObjects.Contains(toPartnerId))))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbToPartnerProducts = Db.PartnerProductSettings.Where(x => x.PartnerId == toPartnerId).Select(x => x.ProductId).ToList();
            Db.PartnerProductSettings.Where(x => x.PartnerId == fromPartnerId && !dbToPartnerProducts.Contains(x.ProductId))
                                     .InsertFromQuery(x => new
                                     {
                                         PartnerId = toPartnerId,
                                         x.ProductId,
                                         x.Percent,
                                         x.State,
                                         x.Rating,
                                         x.CategoryIds,
                                         x.OpenMode,
                                         x.HasDemo,
                                         x.RTP,
                                         x.Volatility
                                     });
            return GetfnPartnerProductSettings(new FilterfnPartnerProductSetting { PartnerId = toPartnerId }, true);
        }

        public List<GameProvider> GetGameProviders(FilterGameProvider filter, bool checkPermission = true)
        {
            if (checkPermission)
            {
                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewGameProvider,
                    ObjectTypeId = ObjectTypes.GameProvider
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<GameProvider>>
                {
                    new CheckPermissionOutput<GameProvider>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x => checkP.AccessibleObjects.Contains(x.ObjectId)
                    }
                };
            }
            if (filter.ParentId.HasValue)
                return filter.FilterObjects(Db.Products.Where(x => x.ParentId == filter.ParentId && x.SubproviderId.HasValue)
                                                       .SelectMany(x => new[] { x.GameProvider1, x.GameProvider })
                                                       .Where(x => x!=null).Distinct()).ToList();
            if (filter.SettingPartnerId.HasValue)
                return filter.FilterObjects(Db.PartnerProductSettings.Where(x => x.PartnerId == filter.SettingPartnerId.Value)
                                                                     .SelectMany(x =>new[] { x.Product.GameProvider1, x.Product.GameProvider })
                                                                     .Where(x => x!=null).Distinct()).ToList();

            if (filter.PartnerId.HasValue)
                return filter.FilterObjects((from p in Db.Products
                                            where p.GameProviderId != null
                                            join pps in Db.PartnerProductSettings.Where(x => x.PartnerId == filter.PartnerId)
                                            on p.Id equals pps.ProductId
                                            into pp
                                            from x in pp.DefaultIfEmpty()
                                            where x == null
                                            select new[] { p.GameProvider, p.GameProvider1 }).SelectMany(x=>x)).Where(x=>x!=null).Distinct().ToList();

            return filter.FilterObjects(Db.GameProviders).Distinct().ToList();
        }

        public List<GameProvider> SaveGameProvider(ApiGameProvider apiGameProvider)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewGameProvider,
                ObjectTypeId = ObjectTypes.GameProvider
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditGameProvider,
                ObjectTypeId = ObjectTypes.GameProvider
            });
            if ((apiGameProvider.Ids== null || !apiGameProvider.Ids.Any()) && apiGameProvider.Id.HasValue && apiGameProvider.Id <=0)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var dbGameProviders = new List<GameProvider>();
            if (!string.IsNullOrEmpty(apiGameProvider.Name))
                apiGameProvider.Name = apiGameProvider.Name.Replace(" ", string.Empty);
            if (apiGameProvider.Ids == null || !apiGameProvider.Ids.Any())
            {
                if (Db.GameProviders.Any(x => x.Name.ToLower() == apiGameProvider.Name.ToLower() && x.Id != apiGameProvider.Id))
                    throw CreateException(LanguageId, Constants.Errors.NickNameExists);
                var dbGameProvider = Db.GameProviders.FirstOrDefault(x => x.Id == apiGameProvider.Id);
                if (dbGameProvider == null)
                    dbGameProviders.Add(Db.GameProviders.Add(new GameProvider
                    {
                        Id = apiGameProvider.Id.Value,
                        Name = apiGameProvider.Name,
                        GameLaunchUrl = apiGameProvider.GameLaunchUrl,
                        Type = apiGameProvider.Type ?? 0,
                        IsActive = apiGameProvider.IsActive ?? true,
                        SessionExpireTime = apiGameProvider.SessionExpireTime ?? 30
                    }));
                else
                {
                    dbGameProvider.Name = apiGameProvider.Name;
                    if (apiGameProvider.IsActive.HasValue)
                        dbGameProvider.IsActive = apiGameProvider.IsActive.Value;
                    dbGameProvider.GameLaunchUrl = apiGameProvider.GameLaunchUrl;
                    if (apiGameProvider.Type.HasValue)
                        dbGameProvider.Type = apiGameProvider.Type.Value;
                    if (apiGameProvider.SessionExpireTime.HasValue)
                        dbGameProvider.SessionExpireTime = apiGameProvider.SessionExpireTime.Value;
                    if (apiGameProvider.GameProviderCurrencySettings != null)
                    {
                        if (!apiGameProvider.GameProviderCurrencySettings.Any())
                            Db.GameProviderCurrencySettings.Where(x => x.GameProviderId == dbGameProvider.Id).DeleteFromQuery();
                        else
                        {
                            var type = apiGameProvider.GameProviderCurrencySettings.First().Type;
                            var currencies = apiGameProvider.GameProviderCurrencySettings.Select(x => x.CurrencyId).ToList();
                            Db.GameProviderCurrencySettings.Where(x => x.GameProviderId == dbGameProvider.Id &&
                                                                      (x.Type != type || !currencies.Contains(x.CurrencyId))).DeleteFromQuery();
                            var dbCurrencies = Db.GameProviderCurrencySettings.Where(x => x.GameProviderId == dbGameProvider.Id).Select(x => x.CurrencyId).ToList();
                            currencies.RemoveAll(x => dbCurrencies.Contains(x));
                            foreach (var c in currencies)
                                Db.GameProviderCurrencySettings.Add(new GameProviderCurrencySetting { GameProviderId = dbGameProvider.Id, CurrencyId = c, Type = type });

                        }
                        dbGameProviders.Add(dbGameProvider);
                    }
                }

                SaveChanges();
            }
            else
            {
                dbGameProviders = Db.GameProviders.Where(x => apiGameProvider.Ids.Contains(x.Id)).ToList();
                dbGameProviders.ForEach(x =>
                {
                    if (apiGameProvider.Type.HasValue)
                        x.Type = apiGameProvider.Type.Value;
                    if (apiGameProvider.SessionExpireTime.HasValue)
                        x.SessionExpireTime = apiGameProvider.SessionExpireTime.Value;
                    if (apiGameProvider.IsActive.HasValue)
                        x.IsActive = apiGameProvider.IsActive.Value;
                });
                SaveChanges();
            }
            if (apiGameProvider.Id.HasValue && apiGameProvider.Id> 0)
                CacheManager.RemoveGameProviderFromCache(apiGameProvider.Id.Value, apiGameProvider.Name);
            else
                dbGameProviders.ForEach(x =>
                {
                    CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, x.Id));
                    CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, x.Name));
                });

            CacheManager.RemoveKeysFromCache(string.Format("{0}_", Constants.CacheItems.RestrictedGameProviders));
            return dbGameProviders;
        }

        public List<int?> ChangePartnerProductState(PartnerProductSetting partnerProductSetting)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPartnerProductSetting,
                ObjectTypeId = ObjectTypes.Product,
                ObjectId = partnerProductSetting.Id
            });

            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(partnerProductSetting.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var ids = new List<int?>();
            if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                ids = Db.fn_GetChildProductsByProductId(partnerProductSetting.ProductId).ToList();
            else
                ids.Add(partnerProductSetting.ProductId);
            Db.PartnerProductSettings.Where(x => x.PartnerId == partnerProductSetting.PartnerId && ids.Contains(x.ProductId)).
                                      UpdateFromQuery(y => new PartnerProductSetting { State = partnerProductSetting.State });

            var productsSettings = Db.PartnerProductSettings.Where(x => ids.Contains(x.ProductId)).ToList();
            foreach (var productsSetting in productsSettings)
            {
                CacheManager.RemovePartnerProductSetting(productsSetting.PartnerId, productsSetting.ProductId);
            }
            CacheManager.RemovePartnerProductSettingPages(partnerProductSetting.PartnerId);
            return ids;
        }

        #region Export to excel

        public List<fnProduct> ExportFnProducts(FilterfnProduct filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewProduct,
                ObjectTypeId = ObjectTypes.Product
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnProduct>>
            {
                new CheckPermissionOutput<fnProduct>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x=> checkP.AccessibleObjects.Contains(x.ObjectId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            return filter.FilterObjects(Db.fn_Product(LanguageId)).ToList();
        }

        #endregion

        public List<DAL.ProductCategory> GetProductCategories(bool checkPermission = true)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewProduct,
                    ObjectTypeId = ObjectTypes.Product
                });
                if (!checkPermissionResult.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }

            return Db.ProductCategories.ToList();
        }
        public ProductCategory SaveProductCategory(ProductCategory productCategory)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewProduct,
                ObjectTypeId = ObjectTypes.Product
            });
            var checkEditPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerProductSetting,
                ObjectTypeId = ObjectTypes.Product
            });

            if (!checkEditPermissionResult.HaveAccessForAllObjects || !checkPermissionResult.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (productCategory.Id == 0)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var query = Db.ProductCategories.Where(x => x.Id == productCategory.Id);
            var res = query.UpdateFromQuery(x => new DAL.ProductCategory
            {
                Name = productCategory.Name,
                Type = productCategory.Type
            });
            if (res == 0)
            {
                var newProductCategory = new ProductCategory
                {
                    Id = productCategory.Id,
                    Name = productCategory.Name,
                    Type = productCategory.Type,
                    Translation = CreateTranslation(new fnTranslation
                    {
                        ObjectTypeId = (int)ObjectTypes.ProductCategory,
                        Text = productCategory.Name,
                        LanguageId = Constants.DefaultLanguageId
                    })
                };
                Db.ProductCategories.Add(newProductCategory);
                Db.SaveChanges();
                return newProductCategory;
            }
            return query.FirstOrDefault();
        }

        public List<DAL.Models.ProductLimit> GetProductsLimits(int objectTypeId, int objectId, int limitType)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewProduct,
                ObjectTypeId = ObjectTypes.Product
            });
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            if (!checkPermissionResult.HaveAccessForAllObjects || (objectTypeId == (int)ObjectTypes.Partner && !checkPartnerPermission.HaveAccessForAllObjects &&
                !checkPartnerPermission.AccessibleObjects.Contains(objectId)) ||
                 (objectTypeId == (int)ObjectTypes.Client && !checkClientPermission.HaveAccessForAllObjects &&
                !checkClientPermission.AccessibleObjects.Contains(objectId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return Db.ProductLimits.Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId &&
                                                     x.LimitTypeId == limitType && x.ProductId.HasValue && x.Product.GameProvider.IsActive)
                                         .Select(x => new DAL.Models.ProductLimit
                                         {
                                             Id = x.Id,
                                             ObjectTypeId = x.ObjectTypeId,
                                             ObjectId = x.ObjectId,
                                             ProductId = x.ProductId,
                                             MaxLimit = x.MaxLimit,
                                             MinLimit = x.MinLimit
                                         }).ToList();
        }

        public DAL.Models.ProductLimit SaveProductLimit(DAL.Models.ProductLimit limit, bool checkPermission)
        {
            if (checkPermission)
            {
                if (limit.ObjectTypeId == (int)ObjectTypes.Client)
                {
                    var client = CacheManager.GetClientById((int)limit.ObjectId);
                    if (client == null)
                        throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

                    var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditClient,
                        ObjectTypeId = ObjectTypes.Client
                    });
                    var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewAffiliateReferral,
                        ObjectTypeId = ObjectTypes.AffiliateReferral
                    });
                    if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != client.Id)) ||
                   (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                   (!affiliateAccess.HaveAccessForAllObjects &&
                     affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                        throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                }
                else
                {
                    var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewProduct,
                        ObjectTypeId = ObjectTypes.Product
                    });
                    var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditPartnerProductSetting,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!checkPermissionResult.HaveAccessForAllObjects ||
                        (!checkPartnerPermission.HaveAccessForAllObjects && !checkPartnerPermission.AccessibleObjects.Contains(limit.ObjectId)))
                        throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                }
            }
            if (limit.MaxLimit < 0 || limit.MinLimit < 0 || limit.MaxLimit < limit.MinLimit)
                throw BaseBll.CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var limitType = limit.ObjectTypeId == (int)ObjectTypes.Client ? (int)LimitTypes.FixedClientMaxLimit : (int)LimitTypes.FixedProductLimit;

            var result = Db.ProductLimits.FirstOrDefault(x => x.ObjectTypeId == limit.ObjectTypeId
                        && x.ObjectId == limit.ObjectId &&
                        x.LimitTypeId == limitType && x.ProductId.HasValue && x.ProductId == limit.ProductId);

            if (result == null)
            {
                result = new DAL.ProductLimit
                {
                    ObjectId = limit.ObjectId,
                    ObjectTypeId = limit.ObjectTypeId,
                    ProductId = limit.ProductId,
                    LimitTypeId = limitType,
                    MaxLimit = limit.MaxLimit,
                    MinLimit = limit.MinLimit,
                    RowState = (int)LimitRowStates.Active
                };
                Db.ProductLimits.Add(result);
            }
            else
            {
                if (limit.MaxLimit == null && limit.MinLimit == null)
                    Db.ProductLimits.Remove(result);
                else
                {
                    result.MaxLimit = limit.MaxLimit;
                    result.MinLimit = limit.MinLimit;
                }
            }
            Db.SaveChanges();
            CacheManager.RemoveProductLimit(limit.ObjectTypeId, limit.ObjectId, limitType, limit.ProductId.Value);

            limit.Id = result.Id;
            return limit;
        }

        public List<int> SynchronizeProducts(int gameProviderId, List<fnProduct> providerGames)
        {
            var resp = new List<int>();
            var allProducts = Db.Products.Include(x => x.GameProvider).Where(x => x.GameProviderId == gameProviderId &&
                                                                                  x.ExternalId.ToLower() != "lobby" &&
                                                                                  x.ExternalId.ToLower() != "promowin").ToList();
            var maxId = allProducts.Count == 0 ? gameProviderId * 1000 : allProducts.Max(x => x.Id);
            if (maxId == 1999)
                maxId = 100000;
            else if (maxId == 30000)
                maxId = 75000;
            else if (maxId == 79000) 
                maxId = 250000;

            Log.Info("Provider Games Count: " + providerGames.Count);
            var existingGames = allProducts.Where(x => providerGames.Any(y => y.ExternalId.ToLower() == x.ExternalId.ToLower())).ToList();
            Log.Info("Existing Games Count: " + existingGames.Count);
            var newIds = providerGames.Select(x => x.ExternalId.ToLower()).Except(existingGames.Select(x => x.ExternalId.ToLower())).ToList();
            var newGames = providerGames.Where(x => newIds.Contains(x.ExternalId.ToLower())).ToList();
            Log.Info("New Games Count: " + newGames.Count);
            var productCountrySettings = Db.ProductCountrySettings.Where(x => x.Product.GameProviderId == gameProviderId && x.PartnerId == null).ToList();
            var removableCountrySettingIds = new List<int>();
            var insertableCountrySettings = new List<ProductCountrySetting>();
            var currentDate = DateTime.UtcNow;
            Parallel.ForEach(existingGames, dbProd =>
            {
                try
                {
                    var product = providerGames.First(x => x.ExternalId.ToLower() == dbProd.ExternalId.ToLower());
                    dbProd.NickName = product.NickName;
                    dbProd.IsForDesktop = product.IsForDesktop;
                    dbProd.IsForMobile = product.IsForMobile;
                    dbProd.SubproviderId = product.SubproviderId;
                    dbProd.HasDemo = product.HasDemo;
                    dbProd.LastUpdateTime = currentDate;
                    dbProd.Lines = product.Lines;
                    dbProd.Volatility = product.Volatility;
                    dbProd.BetValues = product.BetValues;
                    if (dbProd.State == (int)ProductStates.DisabledByProvider)
                        dbProd.State = (int)ProductStates.Active;
                    if (product.RTP.HasValue && product.RTP.Value <= 100 )
                        dbProd.RTP = product.RTP;
                    if (!string.IsNullOrEmpty(product.WebImageUrl) && (string.IsNullOrEmpty(dbProd.WebImageUrl) || dbProd.WebImageUrl.StartsWith("http")))
                        dbProd.WebImageUrl = product.WebImageUrl;
                    if (!string.IsNullOrEmpty(product.MobileImageUrl) && (string.IsNullOrEmpty(dbProd.MobileImageUrl) || dbProd.MobileImageUrl.StartsWith("http")))
                        dbProd.MobileImageUrl = product.MobileImageUrl;
                    if (!string.IsNullOrEmpty(product.BackgroundImageUrl) && (string.IsNullOrEmpty(dbProd.BackgroundImageUrl) || dbProd.BackgroundImageUrl.StartsWith("http")))
                        dbProd.BackgroundImageUrl = product.BackgroundImageUrl;
                    dbProd.Jackpot = product.Jackpot;
                    dbProd.FreeSpinSupport = product.FreeSpinSupport;
                    if (product.ProductCountrySettings == null || !product.ProductCountrySettings.Any())
                        removableCountrySettingIds.AddRange(productCountrySettings.Where(x => x.ProductId == product.Id).Select(x => x.Id));
                    //else
                    //{
                    //    var countries = product.ProductCountrySettings.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.Select(y => y.CountryId));
                    //    foreach (var t in countries)
                    //    {
                    //        var countriesByType = t.Value.ToList();
                    //        removableCountrySettingIds.AddRange(productCountrySettings.Where(x => x.ProductId == dbProd.Id &&
                    //                                                                              x.Type == t.Key && !countriesByType.Contains(x.CountryId))
                    //                                                                  .Select(x => x.Id));
                    //        var dbCountries = productCountrySettings.Where(x => x.ProductId == dbProd.Id && x.Type == t.Key).Select(x => x.CountryId).ToList();
                    //        countriesByType.RemoveAll(x => dbCountries.Contains(x));
                    //        lock (insertableCountrySettings)
                    //            insertableCountrySettings.AddRange(countriesByType.Select(x => new ProductCountrySetting { ProductId = dbProd.Id, CountryId = x, Type = t.Key }));
                    //    }
                    //    lock (removableCountrySettingIds)
                    //        removableCountrySettingIds.AddRange(productCountrySettings.Where(x => x.ProductId == dbProd.Id && !countries.Keys.Contains(x.Type)).Select(x => x.Id));
                    //}
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
            resp.AddRange(existingGames.Select(x => x.Id));
            try
            {
                if (removableCountrySettingIds.Any())
                    Db.ProductCountrySettings.Where(x => removableCountrySettingIds.Any(y => y == x.Id)).DeleteFromQuery();
                if (insertableCountrySettings.Any())
                    Db.ProductCountrySettings.AddRange(insertableCountrySettings);
            }
            catch (Exception e)
            {
                Log.Error("removableCountrySettingIds: " + JsonConvert.SerializeObject(removableCountrySettingIds));
                Log.Error(e);
            }

            Db.SaveChanges();
            foreach (var product in newGames)
            {
                if (product.ExternalId.Length > 99 || product.NickName.Length > 49)
                {
                    Log.Info(product.ExternalId);
                    continue;
                }
                try
                {
                    if (product.ParentId.HasValue)
                    {
                        var parent = CacheManager.GetProductById(product.ParentId.Value);
						var t = CreateTranslation(new fnTranslation
                        {
                            ObjectTypeId = (int)ObjectTypes.Product,
                            Text = product.Name,
                            LanguageId = Constants.DefaultLanguageId
                        });
                        Db.Translations.Add(t);
                        Db.SaveChanges();
                        ++maxId;
                        var prod = new Product
                        {
                            Id = maxId,
                            GameProviderId = gameProviderId,
                            ParentId = product.ParentId.Value,
                            NickName = product.NickName,
                            ExternalId = product.ExternalId,
                            State = product.State,
                            Level = parent.Level + 1,
                            IsForDesktop = product.IsForDesktop,
                            IsForMobile = product.IsForMobile,
                            SubproviderId = product.SubproviderId,
                            HasDemo = product.HasDemo,
                            WebImageUrl = product.WebImageUrl,
                            MobileImageUrl = product.MobileImageUrl,
                            BackgroundImageUrl = product.BackgroundImageUrl,
                            Jackpot = product.Jackpot,
                            FreeSpinSupport = product.FreeSpinSupport,
                            RTP = product.RTP.HasValue && product.RTP.Value <= 100 ? product.RTP : null,
                            Lines = product.Lines,
                            BetValues = product.BetValues,
                            CreationTime = currentDate,
                            LastUpdateTime = currentDate,
                            Translation = t, 
                            Volatility = product.Volatility
                        };
                        Db.Products.Add(prod);
                        Db.SaveChanges();
                        var parentProduct = Db.Products.First(x => x.Id == product.ParentId.Value);
                        prod.Path += parentProduct.Path + prod.Id + "/";
                        Db.SaveChanges();
                        resp.Add(maxId);
                        if (product.ProductCountrySettings != null)
                            foreach (var cs in product.ProductCountrySettings)
                                Db.ProductCountrySettings.Add(new ProductCountrySetting { ProductId = prod.Id, CountryId = cs.CountryId, Type = cs.Type });
                    }
                    else
                        Log.Info(JsonConvert.SerializeObject(product));
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            };

            var updatingItems = allProducts.Where(x => !resp.Contains(x.Id) && x.Id != Constants.SportsbookProductId &&
            x.ExternalId != "lobby" && (x.GameProvider.Name != Constants.GameProviders.EveryMatrix || !x.ExternalId.Contains("sport")) 
                                    && x.GameProvider.Name != Constants.GameProviders.VisionaryiGaming).ToList();
            Parallel.ForEach(updatingItems, i => { i.LastUpdateTime = currentDate; i.State = (int)ProductStates.DisabledByProvider; }) ; 
            Db.SaveChanges();
            Log.Info("resp count:" + resp.Count().ToString());
            return resp;
        }

        public Dictionary<int, string> GetJackpot(int partnerId)
        {
            return Db.PartnerProductSettings
                     .Where(x => x.PartnerId == partnerId && !string.IsNullOrEmpty(x.Product.Jackpot))
                     .ToDictionary(x => x.ProductId, x => x.Product.Jackpot);
        }

        public GameProviderSetting SaveGameProviderSetting(GameProviderSetting gameProviderSetting)
        {
            var dbGameProviderSetting = Db.GameProviderSettings.Include(x => x.GameProvider).FirstOrDefault(x => x.GameProviderId == gameProviderSetting.GameProviderId &&
                                                                                    x.ObjectTypeId == gameProviderSetting.ObjectTypeId &&
                                                                                    x.ObjectId == gameProviderSetting.ObjectId);
            var currentDate = DateTime.UtcNow;
            if (dbGameProviderSetting == null)
            {
                gameProviderSetting.CreationTime = currentDate;
                gameProviderSetting.LastUpdateTime = currentDate;
                Db.GameProviderSettings.Add(gameProviderSetting);
                Db.SaveChanges();
                return Db.GameProviderSettings.Include(x => x.GameProvider).FirstOrDefault(x => x.Id == gameProviderSetting.Id);
            }
            else
            {
                gameProviderSetting.Id = dbGameProviderSetting.Id;
                dbGameProviderSetting.State = gameProviderSetting.State;
                dbGameProviderSetting.LastUpdateTime = currentDate;
            }
            Db.SaveChanges();
            return dbGameProviderSetting;
        }

        public List<GameProviderSetting> GetGameProviderSettings(int objectTypeId, int objectId)
        {
            return Db.GameProviderSettings.Include(x => x.GameProvider).Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId).ToList();
        }
    }
}