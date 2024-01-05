using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Cache;
using System;
using System.ServiceModel;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class Helpers
    {
        public static FaultException<BllFnErrorType> CreateException( int errorId, decimal? decimalInfo = null, DateTime? dateTimeInfo = null)
        {
            var errorType = new BllFnErrorType { Id = errorId };
            errorType.DecimalInfo = decimalInfo;
            errorType.DateTimeInfo = dateTimeInfo;
            return new FaultException<BllFnErrorType>(errorType);
        }
    }
}