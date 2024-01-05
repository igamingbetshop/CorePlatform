using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.DriveMedia;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using System.Text;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class DriveMediaController : ControllerBase
    {
        [HttpPost]
        [Route("{partnerId}/api/DriveMedia/ApiRequest")]
        public IActionResult ApiRequest(int partnerId, BaseInput input)
        {
            BaseOutput response = null;
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                switch (input.Command)
                {
                    case "getBalance":
                        return GetBalance(partnerId, input);
                    case "writeBet":
                        return WriteBet(partnerId, input);
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(fex));
                response = new BaseOutput
                {
                    Status = DriveMediaHelpers.Statuses.Fail,
                    Code = DriveMediaHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    Login = new string[] { "0" },
                    Balance = new long[] { 0 }
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(ex));
                response = new BaseOutput
                {
                    Status = DriveMediaHelpers.Statuses.Fail,
                    Code = DriveMediaHelpers.GetError(Constants.Errors.GeneralException),
                    Login = new string[] { "0" },
                    Balance = new long[] { 0 }
                };
            }
            Program.DbLogger.Info(JsonConvert.SerializeObject(JsonConvert.SerializeObject(response)));
            return Ok(response);
        }

        private IActionResult GetBalance(int partnerId, BaseInput input)
        {
            BaseOutput response = null;
            var strParamsOfData = AssembleAdditionalParams(input.Data);
            var strParams = string.Format("cmd={0}&space={1}&login%5B0%5D={2}&currency={3}{4}{5}",
                input.Command, input.Space, input.Login[0], input.Currency,
                input.ExternalGameId != null ? ("&gameId=" + input.ExternalGameId) : "", strParamsOfData);
            CheckSign(partnerId, strParams, input.Sign);
            if (input.Data.Token != null)
            {
                CheckClientSession(input.Data.Token);
            }
            var client = CacheManager.GetClientById(Convert.ToInt32(input.Login[0]));
            var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
            response = new BaseOutput
            {
                Status = DriveMediaHelpers.Statuses.Success,
                Code = "",
                Login = new string[] { input.Login[0] },
                Balance = new long[] { (long)(balance * 100) }
            };

            Program.DbLogger.Info(JsonConvert.SerializeObject(JsonConvert.SerializeObject(response)));
            return Ok(response);
        }

        private IActionResult WriteBet(int partnerId, BaseInput input)
        {
            WriteBetOutput response = null;
            decimal balance = 0;
            try
            {
                ProcessBet(partnerId, input, out response, out balance);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(fex));
                response = new WriteBetOutput
                {
                    Status = DriveMediaHelpers.Statuses.Fail,
                    Code = DriveMediaHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id),
                    Login = new string[] { "0" },
                    Balance = new long[] { 0 },
                    OperationId = "0"
                };
                if (fex.Detail.Id != Constants.Errors.GeneralException && fex.Detail.Id != Constants.Errors.ClientNotFound)
                {
                    response.Login = new string[] { input.Login[0] };
                    response.Balance = new long[] { (long)(balance * 100) };
                    if (fex.Detail.Id == Constants.Errors.TransactionAlreadyExists)
                    {
                        response.Login = new string[] { input.Login[0] };
                        response.Balance = new long[] { (long)(balance * 100) };
                        response.OperationId = fex.Detail.DecimalInfo.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(ex));
                response = new WriteBetOutput
                {
                    Status = DriveMediaHelpers.Statuses.Fail,
                    Code = DriveMediaHelpers.GetError(Constants.Errors.GeneralException),
                    Login = new string[] { "0" },
                    Balance = new long[] { 0 },
                    OperationId = "0"
                };
            }
            Program.DbLogger.Info(JsonConvert.SerializeObject(JsonConvert.SerializeObject(response)));
            return Ok(response);
        }

        private void ProcessBet(int partnerId, BaseInput input, out WriteBetOutput response, out decimal balance)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var date = ConvertDateTimeToRequiredFormat(input.Date);
                    var strParamsOfData = AssembleAdditionalParams(input.Data);
                    var strParams =
                        string.Format(
                            "cmd={0}&space={1}&login%5B0%5D={2}&bet={3}&winLose={4}&tradeId={5}&betInfo={6}&gameId={7}&currency={8}&date={9}{10}",
                            input.Command, input.Space, input.Login[0], input.BetAmount, input.WinLoseAmount, input.TransactionId,
                            input.BetInfo, input.ExternalGameId, input.Currency, date, strParamsOfData);
                    CheckSign(partnerId, strParams, input.Sign);
                    var clientSession = CheckClientSession(input.Data.Token);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    if (client.Id.ToString() != input.Login[0])
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    Document canceledDocument = null, betDocument = null, winDocument = null;
                    if (input.BetInfo == DriveMediaHelpers.BetInfo.Rollback)
                    {
                        canceledDocument = Rollback(documentBl, input);
                    }
                    else
                    {
                        if (input.BetAmount > 0 || input.BetInfo == DriveMediaHelpers.BetInfo.SpinomenalFreeSpin ||
                            input.BetInfo == DriveMediaHelpers.BetInfo.Jackpot)
                        {
                            betDocument = Bet(clientBl, client, input, clientSession, documentBl);
                        }

                        if (input.BetAmount + input.WinLoseAmount > 0 || input.BetAmount == 0)
                        {
                            if (betDocument == null)
                            {
                                var providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.DriveMedia).Id;
                                var product = CacheManager.GetProductByExternalId(providerId, input.ExternalGameId);
                                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                                if (partnerProductSetting == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                                betDocument = documentBl.GetLastDocumentByExternalId(input.TransactionId, client.Id, providerId,
                                    partnerProductSetting.Id, (int)OperationTypes.Bet);
                                if (betDocument == null && input.WinLoseAmount > 0)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                            }
                            if (betDocument != null)
                            {
                                if (betDocument.State != (int)BetDocumentStates.Won)
                                    betDocument.State = input.BetAmount + input.WinLoseAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                                winDocument = Win(clientBl, client, input, clientSession, betDocument, documentBl);
                            }
                        }
                    }

                    balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                    long operationId = 0;
                    if (canceledDocument != null && canceledDocument.ParentId.HasValue)
                        operationId = canceledDocument.ParentId.Value;
                    else if (input.BetAmount > 0 || input.BetInfo == DriveMediaHelpers.BetInfo.SpinomenalFreeSpin)
                    {
                        if (betDocument != null)
                            operationId = betDocument.Id;
                    }
                    else if (winDocument == null)
                    {
                        operationId = (new Random()).Next(999999999, int.MaxValue);
                    }
                    else
                    {
                        operationId = winDocument.Id;
                    }
                    response = new WriteBetOutput
                    {
                        Code = "",
                        Status = DriveMediaHelpers.Statuses.Success,
                        Login = new string[] { input.Login[0] },
                        Balance = new long[] { (long)(balance * 100) },
                        OperationId = operationId.ToString()
                    };
                }
            }
        }

        private Document Bet(ClientBll clientBl, BllClient client, BaseInput input, SessionIdentity clientSession, DocumentBll documentBl)
        {
            var winAmount = Convert.ToDecimal(input.BetAmount) + Convert.ToDecimal(input.WinLoseAmount);
            var tradeId = string.Format("{0}{1}", input.TransactionId, input.Data.Sequence != null ? "-" + input.Data.Sequence : "");
            if (input.BetInfo == DriveMediaHelpers.BetInfo.Jackpot)
                tradeId = string.Format("{0}-{1}", DriveMediaHelpers.BetInfo.Jackpot, tradeId);
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                CurrencyId = client.CurrencyId,
                RoundId = input.Data.RoundId != null ? input.Data.RoundId.ToString() : null,
                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.DriveMedia).Id,
                ExternalProductId = input.ExternalGameId,
                TransactionId = tradeId,
                State = winAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost,
                Info = input.BetInfo,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = Convert.ToDecimal(input.BetAmount) / 100,
                DeviceTypeId = clientSession.DeviceType
            });
            var res = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            BaseHelpers.BroadcastBalance(client.Id);
            return res;
        }

        private Document Win(ClientBll clientBl, BllClient client, BaseInput input, SessionIdentity clientSession, Document betDocument, DocumentBll documentBl)
        {
            var winAmount = Convert.ToDecimal(input.BetAmount) / 100 + Convert.ToDecimal(input.WinLoseAmount) / 100;
            var tradeId = string.Format("{0}{1}", input.TransactionId, input.Data.Sequence != null ? "-" + input.Data.Sequence : "");
            if (input.BetInfo == DriveMediaHelpers.BetInfo.Jackpot)
                tradeId = string.Format("{0}-{1}", DriveMediaHelpers.BetInfo.Jackpot, tradeId);
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                CurrencyId = client.CurrencyId,
                RoundId = input.Data.RoundId != null ? input.Data.RoundId.ToString() : null,
                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.DriveMedia).Id,
                ExternalProductId = input.ExternalGameId,
                TransactionId = tradeId,
                CreditTransactionId = betDocument.Id,
                State = winAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost,
                Info = input.BetInfo,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = winAmount,
                DeviceTypeId = clientSession.DeviceType
            });
            var res = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl).FirstOrDefault();
            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            BaseHelpers.BroadcastBalance(client.Id);
            return res;
        }

        private Document Rollback(DocumentBll documentBl, BaseInput input)
        {
            var tradeId = string.Format("{0}{1}", input.TransactionId, input.Data.Sequence != null ? "-" + input.Data.Sequence : "");
            var providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.DriveMedia).Id;
            var productId = CacheManager.GetProductByExternalId(providerId, input.ExternalGameId).Id;
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                GameProviderId = providerId,
                ExternalProductId = input.ExternalGameId,
                ProductId = productId,
                TransactionId = tradeId,
                Info = input.BetInfo
            };
            var res = documentBl.RollbackProductTransactions(operationsFromProduct).FirstOrDefault();
            return res;
        }

        private SessionIdentity CheckClientSession(string token)
        {
            return ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
        }

        private void CheckSign(int partnerId, string message, string sign)
        {
            var hashSource = string.Format("{0}{1}", GetDriveMediaSecretKey(partnerId), message);
            var hash = CommonFunctions.ComputeMd5(hashSource).ToUpper();
            if (hash != sign)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        private string GetDriveMediaSecretKey(int partnerId)
        {
            var gameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.DriveMedia).Id;
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.DriveMediaSecretKey);
            if (partnerKey == null || partnerKey == string.Empty)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
            return partnerKey;
        }

        private string ConvertDateTimeToRequiredFormat(string date)
        {
            var dateString = date.Split(' ');
            var time = dateString[1].Split(':');
            return string.Format("{0}+{1}%3A{2}%3A{3}", dateString[0], time[0], time[1], time[2]);
        }

        private string AssembleAdditionalParams(AdditionalData data)
        {
            string output = "";

            if (data.WinLines != null)
                output += AddDataPropertyToString("winLines", data.WinLines);
            if (data.Time != null)
                output += AddDataPropertyToString("time", ConvertDateTimeToRequiredFormat(data.Time));
            if (data.TicketId != null)
                output += AddDataPropertyToString("ticketId", data.TicketId);
            if (data.System != null)
                output += AddDataPropertyToString("system", data.System);
            if (data.Session != null)
                output += AddDataPropertyToString("session", data.Session);
            if (data.Sequence != null)
                output += AddDataPropertyToString("sequence", data.Sequence);
            if (data.RoundId != null)
                output += AddDataPropertyToString("roundId", data.RoundId);
            if (data.Result != null)
                output += AddDataPropertyToString("result", data.Result);
            if (data.Platform != null)
                output += AddDataPropertyToString("platform", data.Platform);
            if (data.Token != null)
                output += AddDataPropertyToString("params", data.Token);

            return AssembleSecondaryParams(data, output);
        }

        private string AssembleSecondaryParams(AdditionalData data, string output)
        {
            if (data.Matrix != null && data.Matrix != "[]")
            {
                string matrixOutput = AssembleMatrixProperties(data.Matrix);
                output += AddDataPropertyToString("matrix", matrixOutput);
            }
            else if (data.Matrix == "[]")
                output += AddDataPropertyToString("matrix", "%5B%5D");
            if (data.Lines != null)
                output += AddDataPropertyToString("lines", data.Lines);
            if (data.Lang != null)
                output += AddDataPropertyToString("lang", data.Lang);
            if (data.GameUuid != null)
                output += AddDataPropertyToString("game_uuid", data.GameUuid);
            if (data.FreespinWinSum != null)
                output += AddDataPropertyToString("freespin_win_sum", data.FreespinWinSum);
            if (data.Denomination != null)
                output += AddDataPropertyToString("denomination", data.Denomination);
            if (data.BetType != null)
                output += AddDataPropertyToString("bet_type", data.BetType);
            if (data.Bet != null)
                output += AddDataPropertyToString("bet", data.Bet);

            return output;
        }

        private string AssembleMatrixProperties(string matrixInput)
        {
            MatrixType[] matrixArray = JsonConvert.DeserializeObject<MatrixType[]>(matrixInput);
            string matrixFrame = "%7B%22type%22%3A%22{0}%22%2C%22code%22%3A%22{1}%22%2C%22num%22%3A%22{2}%22%7D{3}";
            string comma = "%2C";
            string type = string.Empty, number =  string.Empty;

            string[] codeElements;
            StringBuilder result = new StringBuilder();
            StringBuilder code = new StringBuilder();
            for (int i = 0; i < matrixArray.Length; i++)
            {
                type = matrixArray[i].Type;
                code.Clear();
                codeElements = matrixArray[i].Code.Split(',');
                for (int j = 0; j < codeElements.Length; j++)
                {
                    if (j == codeElements.Length - 1)
                        comma = "";
                    code.Append(codeElements[j]);
                    code.Append(comma);
                }
                if (i != matrixArray.Length - 1)
                    comma = "%2C";
                number = matrixArray[i].Num;
                result.AppendFormat(matrixFrame, type, code, number, comma);
            }
            result.AppendFormat("%5B{0}%5D", result);
            return result.ToString();
        }

        private static string AddDataPropertyToString(string propName, object propValue)
        {
            var result = new StringBuilder();
            result.AppendFormat("&data%5B{0}%5D={1}", propName, propValue);
            return result.ToString();
        }
    }
}