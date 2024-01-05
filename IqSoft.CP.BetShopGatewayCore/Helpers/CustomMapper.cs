using IqSoft.CP.DAL;
using IqSoft.CP.BetShopGatewayWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.BetShopGatewayWebApi.Models.Reports;

namespace IqSoft.CP.BetShopGatewayWebApi.Helpers
{
    public static class CustomMapper
    {
        public static ApiCloseShiftOutput MapToApiCloseShiftOutput(this CashDeskShift info, double timeZone)
        {
            return new ApiCloseShiftOutput
            {
                Id = info.Number == null ? 1 : info.Number.Value,
                CashierFirstName = info.CashierFirstName,
                CashierLastName = info.CashierLastName,
                BetShopId = info.BetShopId,
                CashDeskId = info.CashDeskId,
                BetShopAddress = info.BetShopAddress,
                StartTime = info.StartTime.AddHours(timeZone),
                EndTime = info.EndTime == null ? null : (DateTime?) info.EndTime.Value.AddHours(timeZone),
                StartAmount = info.StartAmount,
                EndAmount = info.EndAmount,
                BetAmount = info.BetAmount,
                PayedWin = info.PayedWinAmount,
                DepositToInternetClient = info.DepositAmount,
                WithdrawFromInternetClient = info.WithdrawAmount,
                DebitCorrectionOnCashDesk = info.DebitCorrectionAmount,
                CreditCorrectionOnCashDesk = info.CreditCorrectionAmount,
                Balance = info.EndAmount ?? 0,
                BonusAmount = info.BonusAmount
            };
        }

        public static List<BetShopBet> MapToBetShopBets(this List<fnBetShopBet> bets, double timeZone)
        {
            return bets.Select(x => x.MapToBetShopBet(timeZone)).ToList();
        }

        public static BetShopBet MapToBetShopBet(this fnBetShopBet bet, double timeZone)
        {
            return new BetShopBet
            {
                BetDocumentId = bet.BetDocumentId,
                TicketNumber = bet.TicketNumber, 
                State = bet.State,
                BetInfo = string.Empty,
                CashDeskId = bet.CashDeskId, 
                BetAmount = bet.BetAmount,
                WinAmount = bet.WinAmount, 
                CurrencyId = bet.CurrencyId, 
                ProductId = bet.ProductId, 
                GameProviderId = bet.GameProviderId, 
                Barcode = bet.Barcode, 
                CashierId = bet.CashierId,
                BetDate = bet.BetDate.GetGMTDateFromUTC(timeZone),
                WinDate = bet.WinDate.GetGMTDateFromUTC(timeZone),
                PayDate = bet.PayDate.GetGMTDateFromUTC(timeZone),
                BetShopId = bet.BetShopId, 
                BetShopName = bet.BetShopName, 
                PartnerId = bet.PartnerId,
                ProductName = bet.ProductName,
                Profit = bet.BetAmount - bet.WinAmount,
                BetType = bet.BetTypeId 
            };
        }

        public static CashDeskOperation MapToCashDeskOperation(this fnCashDeskTransaction operation, double timeZone)
        {
            return new CashDeskOperation
            {
                Id = operation.Id,
                ExternalTransactionId = operation.ExternalTransactionId,
                Amount = operation.Amount - (operation.CommissionAmount ?? 0),
                CurrencyId = operation.CurrencyId,
                State = operation.State,
                Info = operation.Info,
                Creator = operation.Creator,
                CashDeskId = operation.CashDeskId,
                TicketNumber = operation.TicketNumber,
                TicketInfo = operation.TicketInfo,
                CashierId = operation.CashierId,
                CreationTime = operation.CreationTime.GetGMTDateFromUTC(timeZone),
                OperationTypeName = operation.OperationTypeName,
                CashDeskName = operation.CashDeskName,
                BetShopName = operation.BetShopName,
                BetShopId = operation.BetShopId,
                ClientId = operation.ClientId,
                PaymentRequestId = operation.PaymentRequestId
            };
        }

        public static GetBetShopOperationsOutput MapToBetShopOperations(this List<DAL.fnPaymentRequest> operations, double timeZone)
        {
            return new GetBetShopOperationsOutput
            {
                Operations = operations.Select(x => x.MapTopaymentRequest(timeZone)).ToList()
            };
        }

        public static BetShopOperation MapTopaymentRequest(this DAL.fnPaymentRequest operation, double timeZone)
        {
            return new BetShopOperation
            {
                Id = operation.Id,
                Barcode = operation.Barcode ?? 0,
                ClientId = operation.ClientId,
                Amount = Math.Floor((operation.Amount - (operation.CommissionAmount ?? 0)) * 100) / 100,
                CurrencyId = operation.CurrencyId,
                ClientFirstName = operation.FirstName,
                ClientLastName = operation.LastName,
                UserName = operation.UserName,
                DocumentNumber = operation.ClientDocumentNumber,
                ClientEmail = operation.Email,
                Type = operation.Type,
                CreationTime = operation.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = operation.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static ApiCashierSession ToApiCashierSession(this UserSession session)
        {
            return new ApiCashierSession
            {
                Id = session.Id,
                UserId = session.UserId,
                LanguageId = session.LanguageId,
                Ip = session.Ip,
                Token = session.Token,
                ProductId = session.ProductId,
                CashDeskId = session.CashDeskId,
                StartTime = session.StartTime,
                LastUpdateTime = session.LastUpdateTime,
                EndTime = session.EndTime,
                State = session.State,
                ProjectTypeId = session.ProjectTypeId,
                ParentId = session.ParentId
            };
        }

        public static ApiShift ToApiShift(this DAL.Models.ShiftInfo input, double timeZone)
        {
            return new ApiShift
            {
                Id = input.Number == null ? int.MaxValue : input.Number.Value,
                CashierFirstName = input.CashierFirstName,
                CashierLastName = input.CashierLastName,
                BetShopId = input.BetShopId,
                CashDeskId = input.CashDeskId,
                BetShopAddress = input.BetShopAddress,
                StartTime = input.StartTime.GetGMTDateFromUTC(timeZone),
                EndTime = input.EndTime.GetGMTDateFromUTC(timeZone),
                StartAmount = input.StartAmount,
                BetAmounts = input.BetAmount,
                PayedWins = input.PayedWin,
                DepositToInternetClients = input.DepositToInternetClient,
                WithdrawFromInternetClients = input.WithdrawFromInternetClient,
                DebitCorrectionOnCashDesk = input.DebitCorrectionOnCashDesk,
                CreditCorrectionOnCashDesk = input.CreditCorrectionOnCashDesk,
                BonusAmount = input.BonusAmount,
                Balance = input.EndAmount ?? 0,
                EndAmount = input.EndAmount
            };
        }

        public static Models.BetShopBet ToBetShopBet(this fnBetShopBet input)
        {
            return new Models.BetShopBet
            {
                BetDocumentId = input.BetDocumentId,
                TicketNumber = input.TicketNumber,
                State = input.State,
                BetInfo = string.Empty,
                CashDeskId = input.CashDeskId,
                BetAmount = input.BetAmount,
                WinAmount = input.WinAmount,
                CurrencyId = input.CurrencyId,
                ProductId = input.ProductId,
                GameProviderId = input.GameProviderId,
                Barcode = input.Barcode,
                CashierId = input.CashierId,
                BetDate = input.BetDate,
                WinDate = input.WinDate,
                PayDate = input.PayDate,
                BetShopId = input.BetShopId,
                BetShopName = input.BetShopName,
                PartnerId = input.PartnerId,
                ProductName = input.ProductName,
                Profit = input.BetAmount - input.WinAmount,
                BetType = input.BetTypeId
            };
        }

        public static Models.BetShopBet ToBetShopBet(this GetBetByBarcodeOutput input)
        {
            return new Models.BetShopBet
            {
                BetDocumentId = input.BetDocumentId,
                TicketNumber = input.TicketNumber,
                Barcode = input.Barcode,
                State = input.State,
                CashDeskId = input.CashDeskId,
                BetAmount = input.BetAmount,
                WinAmount = input.WinAmount,
                ProductId = input.ProductId,
                GameProviderId = input.GameProviderId,
                CashierId = input.CashierId,
                BetDate = input.BetDate,
                WinDate = input.WinDate,
                PayDate = input.PayDate,
                ProductName = input.ProductName
            };
        }

        public static ApiBetShopTicket ToApiBetShopTicket(this BetShopTicket ticket)
        {
            return new ApiBetShopTicket
            {
                Id = ticket.Id,
                DocumentId = ticket.DocumentId,
                GameId = ticket.GameId,
                BarCode = ticket.BarCode,
                NumberOfPrints = ticket.NumberOfPrints,
                LastPrintTime = ticket.LastPrintTime,
                CreationTime = ticket.CreationTime
            };
        }

        public static Client MapToClient(this ClientModel client)
        {
            int regionId = 0;
            if (client.City != null)
                regionId = client.City.Value;
            else if (client.Country != null)
                regionId = client.Country.Value;

            return new Client
            {
                Id = client.Id,
                Email = string.IsNullOrWhiteSpace(client.Email) ? string.Empty : client.Email.ToLower(),
                IsEmailVerified = false,
                CurrencyId = client.CurrencyId,
                UserName = string.IsNullOrWhiteSpace(client.UserName) ? string.Empty : client.UserName,
                Password = client.Password,
                PartnerId = client.PartnerId,
                RegionId = regionId,
                Gender = client.Gender,
                BirthDate = new DateTime(client.BirthYear ?? DateTime.MinValue.Year, client.BirthMonth ?? DateTime.MinValue.Month, client.BirthDay ?? DateTime.MinValue.Day),
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                DocumentType = client.DocumentType,
                DocumentNumber = client.DocumentNumber,
                PhoneNumber = client.PhoneNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = string.IsNullOrWhiteSpace(client.MobileNumber) ? string.Empty : (client.MobileNumber.StartsWith("+") ? client.MobileNumber : "+" + client.MobileNumber),
                IsMobileNumberVerified = false,
                LanguageId = client.LanguageId,
                Token = client.Token,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                SendPromotions = client.SendPromotions,
                IsDocumentVerified = client.IsDocumentVerified,
                CategoryId = client.CategoryId == null ? 0 : client.CategoryId.Value,
                ZipCode = client.ZipCode,
                Info = client.PromoCode,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                Apartment = client.Apartment,
                BuildingNumber = client.BuildingNumber
            };
        }

        public static Common.Models.WebSiteModels.ApiLoginClientOutput MapToApiLoginClientOutput(this Client client, double timeZone)
        {
            return new Common.Models.WebSiteModels.ApiLoginClientOutput  
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = client.CurrencyId,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                RegionId = client.RegionId,
                CountryId = client.RegionId,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static GetClientOutput ToGetClientOutput(this Client client)
        {
            return new GetClientOutput
            {
                Id = client.Id,
                Email = client.Email,
                CurrencyId = client.CurrencyId,
                UserName = client.UserName,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                SecondName = client.SecondName,
                LastName = client.LastName,
                SecondSurname = client.SecondSurname,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                LastUpdateTime = client.LastUpdateTime,
                DocumentType = client.DocumentType,
                DocumentNumber = client.DocumentNumber
            };
        }

        public static GetProductSessionOutput ToProductSessionOutput(this UserSession userSession)
        {
            return new GetProductSessionOutput
            {
                ProductId = (int)userSession.ProductId,
                ProductToken = userSession.Token
            };
        }
    }
}