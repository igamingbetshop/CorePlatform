using IqSoft.CP.DataWarehouse;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace IqSoft.CP.DataManager.Services
{
    public class DataCollector
    {
        private static int LastClientId;
        private static int LastUserId;
        private static int LastProductId;
        private static string CorePlatformDbConnectionString = ConfigurationManager.AppSettings["IqSoftCorePlatformEntities"];

        static DataCollector()
        {
            using (var db = new IqSoftDataWarehouseEntities())
            {
                LastClientId = db.Clients.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
                LastUserId = db.Users.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
                LastProductId = db.Products.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
            }
        }

        public static void MigrateDocuments()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 600;

                    var lastDocumentId = db.Documents.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
                    db.sp_InsertDocuments(lastDocumentId);

                }
                Program.DbLogger.Info("MigrateDocuments_Finished");
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateClients()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT TOP(5000) * FROM Client WHERE Id > @clientId";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<Client>(sqlString, new SqlParameter("@clientId", LastClientId)).ToList();
                    if (result.Count > 0)
                    {
                        db.Clients.AddRange(result);
                        db.SaveChanges();
                        LastClientId = result.Max(x => x.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateUsers()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT TOP(5000) * FROM [User] WHERE Id > @userId";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<User>(sqlString, new SqlParameter("@userId", LastUserId)).ToList();
                    if (result.Count > 0)
                    {
                        db.Users.AddRange(result);
                        db.SaveChanges();
                        LastUserId = result.Max(x => x.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateProducts()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT TOP(5000) * FROM Product WHERE Id > @productId";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<Product>(sqlString, new SqlParameter("@productId", LastProductId)).ToList();
                    if (result.Count > 0)
                    {
                        db.Products.AddRange(result);
                        db.SaveChanges();
                        LastProductId = result.Max(x => x.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateGameProviders()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM GameProvider";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<GameProvider>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var gp in result)
                        {
                            var old = db.GameProviders.FirstOrDefault(x => x.Id == gp.Id);
                            if (old == null)
                                db.GameProviders.Add(gp);
                            else
                            {
                                old.Name = gp.Name;
                                old.Type = gp.Type;
                                old.SessionExpireTime = gp.SessionExpireTime;
                                old.GameLaunchUrl = gp.GameLaunchUrl;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }
    }
}
