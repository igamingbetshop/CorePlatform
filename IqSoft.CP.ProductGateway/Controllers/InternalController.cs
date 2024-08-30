using System;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.ProductGateway.Models.IqSoft;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using System.Linq;
using System.Collections.Generic;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.ProductGateway.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class InternalController : ApiController
    {
        #region Admin

        [HttpPost]
        [Route("Internal/CheckPermission")]
        public IHttpActionResult CheckPermission(ApiCheckPermissionInput input)
        {
            ApiResponseBase response = new ApiResponseBase();
            User user = null;
            var actionLog = new ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = input.LanguageId,
                Info = input.Permission
            };
            try
            {
                actionLog.Ip = string.IsNullOrEmpty(input.Ip) ? string.Empty : input.Ip;
                actionLog.Country = string.IsNullOrEmpty(input.Country) ? string.Empty : input.Country;
                actionLog.Source = string.IsNullOrEmpty(input.Source) ? string.Empty : input.Source;
                if (string.IsNullOrEmpty(input.ActionName))
                    input.ActionName = "NotFound";
                
                var action = CacheManager.GetAction(input.ActionName);
                if (action == null)
                    action = CacheManager.GetAction("NotFound");

                actionLog.ActionId = CacheManager.GetAction(input.ActionName).Id;

                var identity = new SessionIdentity();
                if (!string.IsNullOrEmpty(input.Token))
                    identity = CheckUserSession(input.Token, input.UserId, true, out user);
                else if (!string.IsNullOrEmpty(input.ApiKey))
                {
                    using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        var blUser = CacheManager.GetUserById(input.UserId);
                        if (blUser == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.UserNotFound);
                        user = blUser.ToUser();
                        /*var partner = partnerBl.GetPartnerById(blUser.PartnerId);
                        if (partner == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerNotFound);
                        var blockedIps = CacheManager.GetConfigParameters(partner.Id, "AdminBlockedIps").Select(x => x.Key).ToList();
                        if (blockedIps.Contains(actionLog.Ip))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
                        var whitelistedIps = CacheManager.GetConfigParameters(partner.Id, "AdminWhitelistedIps").Select(x => x.Key).ToList();
                        var whitelistedCountries = CacheManager.GetConfigParameters(partner.Id, "AdminWhitelistedCountries").Select(x => x.Key).ToList();
                        */
                        identity = CheckApiAuthorization(input.UserId, input.ApiKey, input.LanguageId);
                    }
                }
                else
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.SessionNotFound);

                actionLog.ObjectId = user.Id;
                actionLog.SessionId = (identity.SessionId == 0 ? (long?)null : identity.SessionId);
                using (var permissionBl = new PermissionBll(identity, WebApiApplication.DbLogger))
                {
                    permissionBl.CheckPermission(input.Permission, false);
                    var permission = CacheManager.GetPermissions().First(x => x.Id == input.Permission);
                    var objectAccess = permissionBl.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = (permission.ObjectTypeId == (int)ObjectTypes.Partner ? Constants.Permissions.ViewPartner : input.Permission),
                        ObjectTypeId = permission.ObjectTypeId
                    });
                    
                    response = new ApiResponseBase
                    {
                        ResponseObject = JsonConvert.SerializeObject(new
                        {
                            Id = user.Id,
                            PartnerId = (user.PartnerId == Constants.MainPartnerId ? (int?)null : user.PartnerId),
                            CurrencyId = user.CurrencyId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Gender = user.Gender,
                            Type = user.Type,
                            HaveAccessToAllObjects = objectAccess.HaveAccessForAllObjects,
                            AccessibleObjects = objectAccess.AccessibleObjects == null ? new List<string>() : objectAccess.AccessibleStringObjects.ToList()
                        })
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = ex.Detail == null
                    ? new ApiResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ApiResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                if (user != null)
                {
                    response.ResponseObject = JsonConvert.SerializeObject(new
                    {
                        Id = user.Id,
                        PartnerId = (user.PartnerId == Constants.MainPartnerId ? (int?)null : user.PartnerId),
                        CurrencyId = user.CurrencyId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Gender = user.Gender,
                        Type = user.Type
                    });
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                if (user != null)
                {
                    response.ResponseObject = JsonConvert.SerializeObject(new
                    {
                        Id = user.Id,
                        PartnerId = (user.PartnerId == Constants.MainPartnerId ? (int?)null : user.PartnerId),
                        CurrencyId = user.CurrencyId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Gender = user.Gender,
                        Type = user.Type
                    });
                }
            }
            finally
            {
                actionLog.ResultCode = response.ResponseCode;
                actionLog.Description = response.Description;
                BaseBll.LogAction(actionLog);
            }
            return Ok(response);
        }
        private SessionIdentity CheckApiAuthorization(int userId, string secureCode, string languageId)
        {
            var user = CacheManager.GetUserById(userId);
            if (user == null)
                throw BaseBll.CreateException(languageId, Constants.Errors.UserNotFound);
            if (user.SecurityCode != secureCode)
                throw BaseBll.CreateException(languageId, Constants.Errors.WrongApiCredentials);

            return new SessionIdentity
            {
                LanguageId = languageId,
                PartnerId = user.PartnerId,
                Token = secureCode,
                Id = userId,
                TimeZone = 0,
                CurrencyId = user.CurrencyId,
                IsAdminUser = user.Type == (int)UserTypes.AdminUser,
                LoginIp = string.Empty,
                SessionId = 0,
                CashDeskId = 0
            };
        }

        #endregion

        private SessionIdentity CheckUserSession(string token, int userId, bool checkExpiration, out User outUser)
        {
            using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var session = userBl.GetUserSession(token, checkExpiration);
                if (session.UserId != userId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);

                var user = userBl.GetUserById(session.UserId.Value);
                var userIdentity = new SessionIdentity
                {
                    LanguageId = session.LanguageId,
                    LoginIp = session.Ip,
                    PartnerId = user.PartnerId,
                    SessionId = session.Id,
                    Token = session.Token,
                    Id = session.UserId.Value,
                    CurrencyId = user.CurrencyId,
                    IsAdminUser = false,
                    CashDeskId = session.CashDeskId ?? 0
                };
                outUser = user;
                return userIdentity;
            }
        }

		/*
        #region Temp

        [HttpGet]
        [Route("ISoftBet/report")]
        public HttpResponseMessage ISoftBetReport([FromUri]ReportInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                using (var reportBl = new ReportBll(WebApiApplication.Identity, WebApiApplication.DbLogger))
                {
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.playerid));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.ISoftBet).Id;
                    var dTo = DateTime.Parse(input.date_to, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    var dFrom = DateTime.Parse(input.date_from, null, System.Globalization.DateTimeStyles.RoundtripKind);
                   /* var toDate = input.date_to.Trim();
                    var fromDate = input.date_from.Trim();
                    var startIndex = toDate.LastIndexOf(" ");
                    var zone = toDate.Substring(startIndex +1, toDate.Length - startIndex - 1).Replace("0",string.Empty);
                    int timeZone = Convert.ToInt32(zone);
                    toDate = toDate.Substring(0, startIndex);
                    var dTo = DateTime.Parse(toDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    dTo = dTo.AddHours(-timeZone);
                    fromDate = fromDate.Substring(0, fromDate.LastIndexOf(" "));
                    var dFrom = DateTime.Parse(fromDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    dFrom = dFrom.AddHours(-timeZone);*/
		/*                    var report = reportBl.GetClientReportByBetsTemp(client.Id, providerId, dFrom, dTo);

							var response = new ReportOutput
							{
								TotalRoundCount = report.TotalRoundCount,
								TotalTransactionsCount = report.TotalTransactionCount,
								TotalBetCount = report.TotalBetCount,
								TotalBetAmount = report.TotalBetAmount * 100,
								TotalCanceledBetCount = report.TotalCanceledBetCount,
								TotalCanceledBetAmount = report.TotalCanceledBetAmount * 100,
								TotalWinCount = report.TotalWinCount,
								TotalWinAmount = report.TotalWinAmount * 100,
								TotalFroundBetsCount = 0,
								Session = 0,
								TotalJackpot = 0,
								TotalFroundWinAmount = 0,
								TotalJackpotWinAmount = 0
							};

							jsonResponse = JsonConvert.SerializeObject(response);
						}
					}
					catch (FaultException<BllFnErrorType> fex)
					{
						jsonResponse = fex.Detail.Message;
					}
					catch (Exception ex)
					{

						jsonResponse = ex.Message;
					}
					WebApiApplication.DbLogger.Info(jsonResponse);
					var httpResponse = new HttpResponseMessage
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Content = new StringContent(jsonResponse, Encoding.UTF8)
					};
					return httpResponse;
				}

				#endregion
				*/

		[HttpPost]
		[Route("Internal/GetPartnerLanguages")]
		public IHttpActionResult GetPartnerLanguages(ApiCheckPermissionInput input)
		{
			using (var languageBl = new LanguageBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var partnerLanguages = languageBl.GetPartnerLanguages(input.PartnerId);
				return Ok(partnerLanguages.Select(x => new { Id = x.LanguageId, Name = x.Language.Name }));
			}
		}
	}
}
