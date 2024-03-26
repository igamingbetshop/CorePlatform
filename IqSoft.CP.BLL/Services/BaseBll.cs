using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using log4net;
using System.Text.RegularExpressions;
using IqSoft.CP.BLL.Interfaces;
using System.Text;
using System.Net;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using Renci.SshNet;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.UserModels;

namespace IqSoft.CP.BLL.Services
{
    public class BaseBll : IBaseBll
    {
        #region Constructors

        public BaseBll(SessionIdentity identity, ILog log, int? timeout = null)
        {
            Identity = identity;
            Log = log;
            Db = CreateEntities();
            if (timeout != null)
                Db.Database.CommandTimeout = timeout.Value;
        }

        public BaseBll(BaseBll baseBl)
        {
            Identity = baseBl.Identity;
            Log = baseBl.Log;
            Db = baseBl.Db;
        }

        #endregion

        #region Properties

        public ILog Log { get; private set; }

        public SessionIdentity Identity { get; private set; }

        protected IqSoftCorePlatformEntities Db;

        public string LanguageId
        {
            get { return string.IsNullOrEmpty(Identity.LanguageId) ? Constants.DefaultLanguageId : Identity.LanguageId; }
        }

        public string CurrencyId
        {
            get { return Identity.CurrencyId; }
        }

        public long SessionId
        {
            get { return Identity.SessionId; }
        }

        #endregion

        #region System

        public void SaveChanges(string comment = null)
        {
            Db.SaveChanges();
        }//To Be deleted

        public void SaveChangesWithHistory(int objectTypeId, long objectId, string data, string comment = null)
        {
            if (SaveHistory(objectTypeId))
            {
                var objectHistory =
                   new ObjectChangeHistory
                   {
                       ChangeDate = DateTime.UtcNow,
                       ObjectId = objectId,
                       ObjectTypeId = objectTypeId,
                       Object = data,
                       SessionId = Identity.IsAdminUser && Identity.SessionId != 0 ? Identity.SessionId : (long?)null,
                       Comment = comment
                   };
                Db.ObjectChangeHistories.Add(objectHistory);
            }
            Db.SaveChanges();
        }

        private bool SaveHistory(int objectTypeId)
        {
            var objectType = Db.ObjectTypes.FirstOrDefault(x => x.Id == objectTypeId);
            if (objectType == null)
                return false;

            return objectType.SaveChangeHistory;
        }

        public void Dispose()
        {
            if (Db != null)
            {
                Db.Dispose();
                Db = null;
            }

            GC.SuppressFinalize(this);
        }

        public void WriteToFile(string content, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            using (var sw = new StreamWriter(fs))
            {
                sw.WriteLine(content);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
        }

        public void UploadFile(string content, string path, FtpModel ftpInput)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                var ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + ftpInput.Url + path));
                ftpRequest.Credentials = new NetworkCredential(ftpInput.UserName, ftpInput.Password);
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpRequest.ContentLength = bytes.Length;
                ftpRequest.UseBinary = true;
                ftpRequest.KeepAlive = false;

                using (Stream ftpStream = ftpRequest.GetRequestStream())
                {
                    ftpStream.Write(bytes, 0, bytes.Length);
                    ftpStream.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void SFTPUpload(string content, string fileName, string folderName, FtpModel ftpInput)
        {
            try
            {
                var ftpHost = ftpInput.Url.Split(':');
                using (var sftpClient = new SftpClient(ftpHost[0], Convert.ToInt32(ftpHost[1]), ftpInput.UserName, ftpInput.Password))
                {
                    sftpClient.Connect();
                    if (!string.IsNullOrEmpty(folderName))
                        sftpClient.ChangeDirectory(folderName);
                    if (sftpClient.IsConnected)
                    {

                        using (var stream = new MemoryStream())
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(content);
                            writer.Flush();
                            stream.Position = 0;
                            sftpClient.BufferSize = 4 * 1024;
                            sftpClient.UploadFile(stream, Path.GetFileName(fileName));
                        }
                    }
                }
            }
            catch (Exception e)
            {
               Log.Error(e);
            }
        }

        public void UploadFtpImage(byte[] bytes, FtpModel ftpModel, string path)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(path));
            ftpRequest.Credentials = new NetworkCredential(ftpModel.UserName, ftpModel.Password);
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpRequest.ContentLength = bytes.Length;
            ftpRequest.UseBinary = true;
            ftpRequest.KeepAlive = false;

            using (var ftpStream = ftpRequest.GetRequestStream())
            {
                ftpStream.Write(bytes, 0, bytes.Length);
                ftpStream.Close();
            }
        }

        public void CreateFtpDirectory(FtpModel ftpModel, string path)
        {
            try
            {
                FtpWebRequest requestDir = (FtpWebRequest)WebRequest.Create(new Uri(path));
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestDir.Credentials = new NetworkCredential(ftpModel.UserName, ftpModel.Password);
                requestDir.UseBinary = true;
                requestDir.KeepAlive = false;

                FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                response.Close();
                response.Dispose();
            }
            catch  { }
        }

        public static void GeneratePDF(string filePath, byte[] imageData)
        {
            using (var document = new PdfDocument())
            {
                using (var ms = new MemoryStream(imageData))
                {
                    var source_pdf = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
                    for (int i = 0; i < source_pdf.PageCount; i++)
                        document.AddPage(source_pdf.Pages[i]);
                    document.Save(filePath);
                }
            }
        }

        #endregion

        #region Translation

        public Translation SaveTranslation(Translation translation)
        {
            var dbTranslation = Db.Translations.FirstOrDefault(x => x.Id == translation.Id);
            var currentDate = GetServerDate();
            if (dbTranslation == null)
            {
                translation.CreationTime = currentDate;
                dbTranslation = new Translation();
                Db.Translations.Add(dbTranslation);
            }
            translation.CreationTime = dbTranslation.CreationTime;// change this part
            Db.Entry(dbTranslation).CurrentValues.SetValues(translation);

            foreach (var entry in translation.TranslationEntries)
            {
                var dbEntry =
                    Db.TranslationEntries.FirstOrDefault(
                        x => x.LanguageId == entry.LanguageId && x.TranslationId == entry.TranslationId);
                if (dbEntry == null)
                {
                    entry.CreationTime = currentDate;
                    dbEntry = new TranslationEntry();
                    Db.TranslationEntries.Add(dbEntry);
                }
                Db.Entry(dbEntry).CurrentValues.SetValues(entry);
                dbEntry.LastUpdateTime = currentDate;
                dbEntry.SessionId = SessionId;
            }
            dbTranslation.LastUpdateTime = currentDate;
            dbTranslation.SessionId = SessionId;
            SaveChanges();
            return translation;
        }

        public fnTranslation SaveTranslation(fnTranslation translation)
        {
            var dbTranslation = Db.Translations.FirstOrDefault(x => x.Id == translation.TranslationId);
            var currentDate = GetServerDate();
            if (dbTranslation == null)
            {
                translation.CreationTime = currentDate;
                dbTranslation = new Translation();
                Db.Translations.Add(dbTranslation);
            }
            translation.CreationTime = dbTranslation.CreationTime;// change this part
            Db.Entry(dbTranslation).CurrentValues.SetValues(translation);
            dbTranslation.LastUpdateTime = currentDate;
            dbTranslation.SessionId = SessionId;

            var dbEntry =
                Db.TranslationEntries.FirstOrDefault(
                    x => x.LanguageId == translation.LanguageId && x.TranslationId == translation.TranslationId);
            if (dbEntry == null)
            {
                dbEntry = new TranslationEntry { CreationTime = currentDate, TranslationId = translation.TranslationId };
                Db.TranslationEntries.Add(dbEntry);
            }
            Db.Entry(dbEntry).CurrentValues.SetValues(translation);
            dbEntry.LastUpdateTime = currentDate;
            dbEntry.SessionId = SessionId;
            Db.SaveChanges();
            return Db.fn_Translation(translation.LanguageId).FirstOrDefault(x => x.TranslationId == translation.TranslationId);
        }

        protected Translation CreateTranslation(fnTranslation translation)
        {
            var currentTime = GetServerDate();
            var newTranslation = new Translation
            {
                ObjectTypeId = translation.ObjectTypeId,
                TranslationEntries = new List<TranslationEntry>
                {
                    new TranslationEntry
                    {
                        LanguageId = translation.LanguageId,
                        Text = translation.Text,
                        SessionId = SessionId == 0 ? (long?)null : SessionId,
                        LastUpdateTime = currentTime,
                        CreationTime = currentTime
                    }
                }
            };

            if (translation.LanguageId != Constants.Languages.English)
            {
                newTranslation.TranslationEntries.Add(new TranslationEntry
                {
                    LanguageId = Constants.Languages.English,
                    Text = translation.Text,
                    SessionId = SessionId == 0 ? (long?)null : SessionId,
					LastUpdateTime = currentTime,
                    CreationTime = currentTime
                });
            }

            newTranslation.SessionId = SessionId == 0 ? (long?)null : SessionId;
            newTranslation.LastUpdateTime = currentTime;
            newTranslation.CreationTime = currentTime;
            return newTranslation;
        }

        protected TranslationEntry UpdateTranslation(fnTranslation translation)
        {
            var currentTime = GetServerDate();
            var dbTranslation = Db.Translations.FirstOrDefault(x => x.Id == translation.TranslationId);
            if (dbTranslation == null)
                throw CreateException(LanguageId, Constants.Errors.TranslationNotFound);
            var dbEntry =
                Db.TranslationEntries.FirstOrDefault(x => x.TranslationId == translation.TranslationId && x.LanguageId == translation.LanguageId);
            if (dbEntry == null)
            {
                dbEntry = new TranslationEntry
                {
                    TranslationId = translation.TranslationId,
                    LanguageId = translation.LanguageId,
                    Text = translation.Text,
                    SessionId = SessionId,
                    LastUpdateTime = currentTime,
                    CreationTime = currentTime
                };
                Db.TranslationEntries.Add(dbEntry);
                dbTranslation.LastUpdateTime = currentTime;
            }
            else if (dbEntry.Text != translation.Text)
            {
                dbEntry.Text = translation.Text;
                dbTranslation.LastUpdateTime = currentTime;
            }
            return dbEntry;
        }

        public PagedModel<Translation> GetTranslationsPagedModel(FilterTranslation filter)
        {
            return new PagedModel<Translation>
            {
                Entities = filter.FilterObjects(Db.Translations, x => x.OrderBy(y => y.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.Translations)
            };
        }

        public List<fnTranslationEntry> GetfnTranslationEntries(FilterfnTranslationEntry filter)
        {
            return filter.FilterObjects(Db.fn_TranslationEntry()).ToList();
        }

        public List<Translation> GetTranslations(FilterTranslation filter)
        {
            return filter.FilterObjects(Db.Translations).ToList();
        }

        public List<TranslationEntry> GetTranslationEntries(long translationId)
        {
            return Db.TranslationEntries.Where(x => x.TranslationId == translationId).ToList();
        }

        #endregion

        public SessionIdentity GetUserIdentity()
        {
            return Identity;
        }

        public ObjectBalance GetObjectBalanceWithConvertion(int objectTypeId, long objectId, string currencyId)
        {

            var accounts =
                Db.Accounts.Where(
                    x =>
                        x.ObjectId == objectId && x.ObjectTypeId == objectTypeId &&
                        x.AccountType.Kind != (int)AccountTypeKinds.Booked &&
                        x.TypeId != (int)AccountTypes.ClientCoinBalance &&
                        x.TypeId != (int)AccountTypes.ClientCompBalance).ToList();
            var availableBalance = string.IsNullOrEmpty(currencyId)
                ? accounts.Sum(x => x.Balance)
                : accounts.Sum(x => ConvertCurrency(x.CurrencyId, currencyId, x.Balance));
            var balance = new ObjectBalance
            {
                CurrencyId = currencyId,
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                AvailableBalance = availableBalance,
                Balances = accounts.Select(x => (new ObjectAccount
                {
                    Id = x.Id,
                    Balance = x.Balance,
                    CurrencyId = x.CurrencyId,
                    TypeId = x.TypeId
                })).ToList()
            };
            return balance;
        }

        public decimal GetAccountBalanceByDate(long accountId, DateTime date)
        {
            var account = Db.Accounts.Include(x => x.AccountType).FirstOrDefault(x => x.Id == accountId);
            if (account == null)
                throw CreateException(LanguageId, Constants.Errors.AccountNotFound);

            var accountBalance =
                Db.AccountBalances.Include(x => x.Account).Where(x => x.AccountId == account.Id && x.Date >= date)
                    .OrderBy(x => x.Date)
                    .FirstOrDefault();
            if (accountBalance != null)
                return accountBalance.Balance;
            else
                return account.Balance;
        }

        public List<DAL.Models.AccountBalance> GetAccountsBalances(int objectTypeId, int objectId, DateTime date)
        {
            var accounts = Db.Accounts.Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId).ToList();
            var balances = new List<DAL.Models.AccountBalance>();
            foreach (var account in accounts)
            {
                decimal balance = 0;
                var accountBalance = Db.AccountBalances.Where(x => x.AccountId == account.Id && x.Date <= date)
                        .OrderByDescending(x => x.Date).FirstOrDefault();

                if (accountBalance != null)
                {
                    balance = accountBalance.Balance;
                    var fDate = (long)accountBalance.Date.Year * 1000000 + (long)accountBalance.Date.Month * 10000 + accountBalance.Date.Day * 100 + accountBalance.Date.Hour;
                    var nextDay = accountBalance.Date.AddDays(1);
                    var toDate = date < nextDay ? date : nextDay;

                    var tDate = (long)toDate.Year * 1000000 + (long)toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;
                    long accountId = account.Id;
                    var operations = Db.Transactions.Where(x => x.AccountId == accountId && x.Date >= fDate && x.Date < tDate).ToList().OrderBy(x => x.CreationTime).ToList();
                    foreach (var operation in operations)
                    {
                        balance += (operation.Type == (int)TransactionTypes.Credit ? -operation.Amount : operation.Amount);
                    }
                }
                balances.Add(new DAL.Models.AccountBalance { AccountId = account.Id, Balance = balance });
            }
            return balances;
        }

        public static decimal ConvertCurrency(string fromCurrencyId, string toCurrencyId, decimal amount)
        {
            if (amount == 0)
                return amount;
            return amount * GetCurrenciesDifference(fromCurrencyId, toCurrencyId);
        }

        public static decimal GetCurrenciesDifference(string fromCurrencyId, string toCurrencyId)
        {
            if (fromCurrencyId == toCurrencyId)
                return 1;
            var fromCurrency = CacheManager.GetCurrencyById(fromCurrencyId);
            if (fromCurrency == null)
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);
            var toCurrency = CacheManager.GetCurrencyById(toCurrencyId);
            if (toCurrency == null)
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);
            return fromCurrency.CurrentRate / toCurrency.CurrentRate;
        }

        public static decimal GetPaymentCurrenciesDifference(string fromCurrencyId, string toCurrencyId, BllPartnerPaymentSetting bllPartnerPaymentSetting)
        {
            if (fromCurrencyId == toCurrencyId)
                return 1;
            var fromRate = 0m;
            var toRate = 0m;
            if (bllPartnerPaymentSetting != null && bllPartnerPaymentSetting.CurrencyRates.Any(x => x.CurrencyId == fromCurrencyId))
                fromRate = bllPartnerPaymentSetting.CurrencyRates.First(x => x.CurrencyId == fromCurrencyId).Rate;
            else
            {
                var fromCurrency = CacheManager.GetCurrencyById(fromCurrencyId);
                if (fromCurrency == null)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);
                fromRate = fromCurrency.CurrentRate;
            }
            if (bllPartnerPaymentSetting != null && bllPartnerPaymentSetting.CurrencyRates.Any(x => x.CurrencyId == toCurrencyId))
                toRate = bllPartnerPaymentSetting.CurrencyRates.First(x => x.CurrencyId == toCurrencyId).Rate;
            else
            {
                var toCurrency = CacheManager.GetCurrencyById(toCurrencyId);
                if (toCurrency == null)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);
                toRate = toCurrency.CurrentRate;
            }
            return fromRate / toRate;
        }

        public Account GetAccount(long id)
        {
            var account = Db.Accounts.FirstOrDefault(x => x.Id == id);
            if (account == null)
                throw CreateException(LanguageId, Constants.Errors.AccountNotFound);

            return account;
        }

        public void ChangeAccountBalance(decimal amount, Account account)
        {
            if (amount == 0)
                return;

            var currentDate = GetServerDate();
            var date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);

            var accountBalanceInCache = CacheManager.GetAccountBalanceByDate(account.Id, date);
            if (accountBalanceInCache == null)
            {
                var accountBalance = Db.AccountBalances.FirstOrDefault(x => x.AccountId == account.Id && x.Date == date);
                if (accountBalance == null)
                {
                    try
                    {
                        accountBalance = new DAL.AccountBalance
                        {
                            AccountId = account.Id,
                            Date = date,
                            Balance = account.Balance
                        };
                        Db.AccountBalances.Add(accountBalance);
                        Db.SaveChanges();
                    }
                    catch (Exception exc)
                    {
                        Log.Error(exc);
                    }
                }
                CacheManager.SetAccountBalanceByDate(new BllAccountBalance
                {
                    AccountId = accountBalance.AccountId,
                    Date = date,
                    Balance = accountBalance.Balance
                });
            }
            account.Balance += amount;
        }

        public void ChangeAccountBalanceForJob(decimal amount, Account account)
        {
            if (amount == 0)
                return;
            var currentDate = GetServerDate();
            var date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);

            var accountBalance = Db.AccountBalances.FirstOrDefault(x => x.AccountId == account.Id && x.Date == date);
            if (accountBalance == null)
            {
                try
                {
                    accountBalance = new DAL.AccountBalance
                    {
                        AccountId = account.Id,
                        Date = date,
                        Balance = account.Balance
                    };
                    Db.AccountBalances.Add(accountBalance);
                    Db.SaveChanges();
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }
            }
            account.Balance += amount;
        }

        public List<fnAccount> GetfnAccounts(FilterfnAccount filter)
        {
            return filter.FilterObjects(Db.fn_Account(LanguageId)).ToList();
        }

        public static List<BllFnEnumeration> GetEnumerations(string enumType, string languageId)
        {
            return CacheManager.GetEnumerations(enumType, languageId);
        }

        public List<fnOperationType> GetOperationTypes()
        {
            return Db.fn_OperationType(LanguageId).ToList();
        }

       /* public string ExportToCSV<T>(string fileName, List<T> exportList, DateTime? fromDate, DateTime? endDate, double timeZone, int? adminMenuId = null, int? adminMenuGridIndex = null)
        {
            List<UserMenuState> userMenuColumns = null;
            if (adminMenuId.HasValue)
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var state = db.UserStates.FirstOrDefault(x => x.UserId == Identity.Id && x.AdminMenuId == adminMenuId)?.State;
                    if (state != null)
                    {
                        var adminMenu = JsonConvert.DeserializeObject<List<List<UserMenuState>>>(state);
                        if (adminMenuGridIndex.HasValue)
                            userMenuColumns = adminMenu[adminMenuGridIndex.Value];
                        else
                            userMenuColumns = adminMenu[0];
                    }
                }
            }
            var localPath = HttpContext.Current.Server.MapPath("~/ExportExcelFiles");
            string fileAbsPath = string.Empty;
            DateTime currentDate = Convert.ToDateTime(GetServerDate().ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            int maxExcelRowAccount = 1000000;
            object locker = new object();

            if (exportList.Count > maxExcelRowAccount)
            {
                List<List<T>> rootList = new List<List<T>>();
                var itemList = new List<T>();

                for (int i = 0; i < exportList.Count; i++)
                {
                    itemList.Add(exportList[i]);
                    if ((i + 1) % maxExcelRowAccount == 0)
                    {
                        rootList.Add(itemList);
                        itemList = new List<T>();
                    }
                }

                if (itemList != null && itemList.Count > 0)
                {
                    rootList.Add(itemList);
                }

                string dirFilesName = string.Empty;
                string fileNameWithoutFormat = fileName.Replace(".csv", "");

                if (fromDate != null && endDate != null)
                {
                    dirFilesName = (string.Format("{0:dd_MM_yyyy_HH_mm_ss}", fromDate.GetGMTDateFromUTC(timeZone)) + "_" + string.Format("{0:dd_MM_yyyy_HH_mm_ss}", endDate.GetGMTDateFromUTC(timeZone)) + "_" + Guid.NewGuid().ToString() + "_" + fileNameWithoutFormat);
                }
                else
                {
                    dirFilesName = Guid.NewGuid().ToString() + "_" + fileNameWithoutFormat;
                }

                string dirPath = Path.Combine(localPath, dirFilesName);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var tasks = new List<Task>();
                int index = 0;
                foreach (var itmList in rootList)
                {
                    string filePath = string.Empty;
                    lock (locker)
                    {
                        filePath = Path.Combine(dirPath, (index + 1).ToString() + "_" + fileName);
                    }
                    var task = Task.Run(() =>
                    {
                        ExportExcelHelper.SaveToCSV<T>(itmList, fromDate, endDate, currentDate, timeZone, filePath, userMenuColumns);
                    });
                    index++;
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                string dirFileZipName = dirFilesName + ".zip";
                ZipFile.CreateFromDirectory(dirPath, Path.Combine(localPath, dirFileZipName));
                Directory.Delete(dirPath, true);
                fileAbsPath = "ExportExcelFiles/" + dirFileZipName;
            }
            else
            {
                string fName = string.Empty;
                if (fromDate != null && endDate != null)
                {
                    fName = (string.Format("{0:dd_MM_yyyy_HH_mm_ss}", fromDate.GetGMTDateFromUTC(timeZone)) + "_" + string.Format("{0:dd_MM_yyyy_HH_mm_ss}", endDate.GetGMTDateFromUTC(timeZone)) + "_" + Guid.NewGuid().ToString() + "_" + fileName);
                }
                else
                {
                    fName = Guid.NewGuid().ToString() + "_" + fileName;
                }

                string filePath = Path.Combine(localPath, fName);

                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }

                ExportExcelHelper.SaveToCSV<T>(exportList, fromDate, endDate, currentDate, timeZone, filePath, userMenuColumns);
                fileAbsPath = "ExportExcelFiles/" + fName;
            }

            return fileAbsPath;
        }*/
        public string ExportToCSV<T>(string fileName, List<T> exportList, DateTime? fromDate, DateTime? endDate, double timeZone, int? adminMenuId = null, int? adminMenuGridIndex = null)
        {
            List<UserMenuState> userMenuColumns = null;
            if (adminMenuId.HasValue)
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var state = db.UserStates.FirstOrDefault(x => x.UserId == Identity.Id && x.AdminMenuId == adminMenuId)?.State;
                    if (state != null)
                    {
                        var adminMenu = JsonConvert.DeserializeObject<List<List<UserMenuState>>>(state);
                        if (adminMenuGridIndex.HasValue)
                            userMenuColumns = adminMenu[adminMenuGridIndex.Value];
                        else
                            userMenuColumns = adminMenu[0];
                    }
                }
            }
            DateTime currentDate = Convert.ToDateTime(GetServerDate().ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            fileName= $"{Guid.NewGuid()}_{fileName}";
            if (fromDate != null && endDate != null)
                fileName = $"/ExportExcelFiles/{fromDate.Value.GetGMTDateFromUTC(timeZone):dd_MM_yyyy_HH_mm_ss}_{endDate.Value.GetGMTDateFromUTC(timeZone):dd_MM_yyyy_HH_mm_ss}_{fileName}";
            var lines = ExportExcelHelper.SaveToCSV<T>(exportList, fromDate, endDate, currentDate, timeZone, fileName, userMenuColumns);

            var ftpModel = new FtpModel
            {
                Url = CacheManager.GetPartnerSettingByKey(Constants.MainPartnerId, Constants.PartnerKeys.StatementFTPServer).StringValue,
                UserName = CacheManager.GetPartnerSettingByKey(Constants.MainPartnerId, Constants.PartnerKeys.StatementFTPUsername).StringValue,
                Password = CacheManager.GetPartnerSettingByKey(Constants.MainPartnerId, Constants.PartnerKeys.StatementFTPPassword).StringValue
            };
            UploadFile(string.Join(Environment.NewLine, lines), fileName, ftpModel);
            return fileName;
        }

        protected static IqSoftCorePlatformEntities CreateEntities()
        {
            return new IqSoftCorePlatformEntities();
        }

        public static ObjectBalance GetObjectBalance(int objectTypeId, long objectId)
        {
            using (var db = CreateEntities())
            {
                var accounts = db.Accounts.Where(x => x.ObjectId == objectId && x.ObjectTypeId == objectTypeId &&
                                                   x.AccountType.Kind != (int)AccountTypeKinds.Booked &&
                                                   x.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                   x.TypeId != (int)AccountTypes.ClientCompBalance).ToList();
                return new ObjectBalance
                {
                    ObjectId = objectId,
                    ObjectTypeId = objectTypeId,
                    AvailableBalance = Math.Floor(accounts.Sum(x => x.Balance) * 100) / 100,
                    CurrencyId = accounts.FirstOrDefault() == null ? string.Empty : accounts.First().CurrencyId,
                    Balances = accounts.Select(x => new ObjectAccount
                    {
                        Id = x.Id,
                        Balance = Math.Floor(x.Balance * 100) / 100,
                        CurrencyId = x.CurrencyId,
                        TypeId = x.TypeId
                    }).ToList()
                };
            }
        }

        public DateTime GetServerDate()
        {
            return DateTime.UtcNow;
        }

        public static FaultException<BllFnErrorType> CreateException(string languageId, int errorId, decimal? decimalInfo = null, 
            DateTime? dateTimeInfo = null, long? integerInfo = null, string info = null)
        {
            if (string.IsNullOrEmpty(languageId))
                languageId = Constants.DefaultLanguageId;

            var errorType = CacheManager.GetfnErrorTypes(languageId).FirstOrDefault(x => x.Id == errorId) ?? new BllFnErrorType { Id = errorId };
            errorType.DecimalInfo = decimalInfo;
            errorType.DateTimeInfo = dateTimeInfo;
            errorType.IntegerInfo = integerInfo;
            errorType.Info = info;

            return new FaultException<BllFnErrorType>(errorType);
        }

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^([\w-\.\+\-]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,10}|[0-9]{1,3})(\]?)$");
        }

        public static bool IsMobileNumber(string mobile)
        {
            return Regex.IsMatch(mobile, @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$");
        }

        public TicketMessage AddMessageToTicket(TicketMessage ticketMessage, out int clientId,  out int unreadMessageCount)
        {            
            var ticket = Db.Tickets.FirstOrDefault(x => x.Id == ticketMessage.TicketId);
            if (ticket == null || ticket.Status != (int)MessageTicketState.Active)
                throw CreateException(Identity.LanguageId, Constants.Errors.TicketNotFound);
            unreadMessageCount = CacheManager.GetClientUnreadTicketsCount(ticket.ClientId.Value).Count;
            clientId = ticket.ClientId.Value;
            var curretDate = GetServerDate();
            ticket.LastMessageDate = (long)curretDate.Year * 100000000 + curretDate.Month * 1000000 + curretDate.Day * 10000 + curretDate.Hour * 100 + curretDate.Minute;
            ticket.LastMessageTime = curretDate;
            ticket.LastMessageUserId = ticketMessage.UserId;
            if (ticket.Type != (int)TicketTypes.Email && ticket.Type != (int)TicketTypes.Sms)
            {
                if (ticketMessage.UserId.HasValue)
                {
                    ticketMessage.User = Db.Users.First(x => x.Id == ticketMessage.UserId);
                    if (ticket.ClientUnreadMessagesCount == 0)
                    {
                        ++unreadMessageCount;
                        CacheManager.UpdateClientUnreadTicketsCount(ticket.ClientId.Value, unreadMessageCount); 
                    }
                    ++ticket.ClientUnreadMessagesCount;
                }
                else
                    ++ticket.UserUnreadMessagesCount;
            }
            ticketMessage = Db.TicketMessages.Add(ticketMessage);
            Db.SaveChanges();
            return ticketMessage;
        }

        public static void CheckIp(List<string> whitelistedIps)
        {
            var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.UserHostAddress;
            if (string.IsNullOrEmpty(ip) || (!whitelistedIps.Contains(ip) && !whitelistedIps.Any(x => x.IsIpEqual(ip))))
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.DontHavePermission, info: ip);
        }

        public static void LogAction(ActionLog action)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentDate = DateTime.UtcNow;
                action.CreationTime = currentDate;
                action.Date = currentDate.Year * 1000000 + currentDate.Month * 10000 + currentDate.Day * 100 + currentDate.Hour;

                if (string.IsNullOrEmpty(action.Domain))
                    action.Domain = string.Empty;
                if (string.IsNullOrEmpty(action.Source))
                    action.Source = string.Empty;
                if (string.IsNullOrEmpty(action.Ip))
                    action.Ip = string.Empty;
                if (string.IsNullOrEmpty(action.Country))
                    action.Country = string.Empty;
                if (action.ActionId == 0)
                    action.ActionId = CacheManager.GetAction("NotFound").Id;
                db.ActionLogs.Add(action);
                db.SaveChanges();
            }
        }
    }
}