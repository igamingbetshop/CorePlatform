using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Platforms.Helpers;
using log4net;
using Microsoft.AspNetCore.Mvc;
using System;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public class UtilController : ControllerBase
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log)
        {
            switch (request.Method)
            {
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

       
    }
}