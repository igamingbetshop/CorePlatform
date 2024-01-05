using System;
using System.Collections.Generic;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IBaseBll : IDisposable
    {
        Translation SaveTranslation(Translation translation);

        SessionIdentity GetUserIdentity();

        ObjectBalance GetObjectBalanceWithConvertion(int objectTypeId, long objectId, string currencyId);

        decimal GetAccountBalanceByDate(long accountId, DateTime date);

        List<DAL.Models.AccountBalance> GetAccountsBalances(int objectTypeId, int objectId, DateTime date);

        Account GetAccount(long id);

        DateTime GetServerDate();

        void ChangeAccountBalance(decimal amount, Account account);

        List<fnAccount> GetfnAccounts(FilterfnAccount filter);

        List<fnOperationType> GetOperationTypes();

        string ExportToCSV<T>(string fileName, List<T> exportList, DateTime? fromDate, DateTime? endDate, double timeZone, IWebHostEnvironment env);
    }
}
