using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Integration.Payments.Models;
using System;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using System.Linq;
using IqSoft.CP.Integration.Payments.Models.Payment;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PaymentHelpers
    {
        public static CryptoAddress GetClientPaymentAddress(int paymentSystemId, int clientId, ILog log)
        {
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentSystemId);
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.CryptoPayBTC:
                case Constants.PaymentSystems.CryptoPayBCH:
                case Constants.PaymentSystems.CryptoPayETH:
                case Constants.PaymentSystems.CryptoPayLTC:
                case Constants.PaymentSystems.CryptoPayUSDT:
                case Constants.PaymentSystems.CryptoPayUSDTERC20:
                case Constants.PaymentSystems.CryptoPayUSDC:
                case Constants.PaymentSystems.CryptoPayDAI:
                case Constants.PaymentSystems.CryptoPayXRP:
                case Constants.PaymentSystems.CryptoPayXLM:
                case Constants.PaymentSystems.CryptoPayADA:
                case Constants.PaymentSystems.CryptoPaySHIB:
                case Constants.PaymentSystems.CryptoPaySOL:
                    return CryptoPayHelpers.CreateCryptoPayChannel(clientId, paymentSystemId, log);
                default:
                    break;
            }

            return new CryptoAddress();
        }

        public static string InitializeWithdrawalsRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var initializeUrl = string.Empty;
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            paymentRequest = UpdatePaymentRequestRegionSettings(paymentRequest, session, log);
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.PraxisCard:
                case Constants.PaymentSystems.PraxisWallet:
                    initializeUrl = PraxisHelpers.CallPraxisApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.FinalPaySkrill:
                case Constants.PaymentSystems.FinalPayCrypto:
                    initializeUrl = FinalPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                default:
                    break;
            }
            return initializeUrl;
        }

        public static PaymentResponse SendPaymentWithdrawalsRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var response = new PaymentResponse
            {
                Status = PaymentRequestStates.Failed,
                Type = (int)PaymentRequestTypes.Withdraw
            };
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.PayBox:
                    response = PayBoxHelpers.CreatePayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PayBoxATM:
                    response = PayBoxHelpers.CreateATMPayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Skrill:
                    response = SkrillHelpers.SendPaymentRequestToSkrill(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.LuckyPay:
                    response = LuckyPayHelpers.SendWithdrawRequestToProvider(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.WalletOne:
                    response = WalletOneHelpers.SendPaymentRequestToWalletOne(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.CreditCards:
                    response = WalletOneHelpers.SendPaymentRequestToCreditCards(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Wooppay:
                    response.Status = WooppayHelpers.SendPaymentRequestToWooppay(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.BankTransferSwift:
                case Constants.PaymentSystems.BankTransferSepa:
                case Constants.PaymentSystems.BankWire:
                case Constants.PaymentSystems.CepBank:
                case Constants.PaymentSystems.ShebaTransfer:
                    response.Status = PaymentRequestStates.Approved;
                    break;
                case Constants.PaymentSystems.Help2Pay:
                    response = Help2PayHelpers.CreatePayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PaySec:
                    response = PaySecHelpers.CreatePayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PayTrust88:
                    response = PayTrust88Helpers.CreatePayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.SDPayQuickPay:
                    // response = SDPayHelpers.CreatePayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PiastrixWallet:
                    response = PiastrixHelpers.CreatePayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PiastrixVisaMaster:
                case Constants.PaymentSystems.PiastrixAlfaclick:
                case Constants.PaymentSystems.PiastrixQiwi:
                case Constants.PaymentSystems.PiastrixYandex:
                case Constants.PaymentSystems.PiastrixPayeer:
                case Constants.PaymentSystems.PiastrixBeeline:
                case Constants.PaymentSystems.PiastrixMTS:
                case Constants.PaymentSystems.PiastrixMegafon:
                case Constants.PaymentSystems.PiastrixTele2:
                case Constants.PaymentSystems.PiastrixBTC:
                case Constants.PaymentSystems.PiastrixTether:
                case Constants.PaymentSystems.PiastrixTinkoff:
                    response = PiastrixHelpers.CreateVisaCardPayment(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.CardPayCard:
                case Constants.PaymentSystems.CardPayBank:
                case Constants.PaymentSystems.CardPayQIWI:
                case Constants.PaymentSystems.CardPayYandex:
                case Constants.PaymentSystems.CardPayWebMoney:
                case Constants.PaymentSystems.CardPayBoleto:
                case Constants.PaymentSystems.CardPayLoterica:
                case Constants.PaymentSystems.CardPaySpei:
                case Constants.PaymentSystems.CardPayDirectBankingEU:
                    response = CardPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ApcoPayAstroPay:
                case Constants.PaymentSystems.ApcoPayEcoPayz:
                case Constants.PaymentSystems.ApcoPayDirect24:
                case Constants.PaymentSystems.ApcoPayVisaMaster:
                case Constants.PaymentSystems.ApcoPayNeoSurf:
                case Constants.PaymentSystems.ApcoPayMuchBetter:
                case Constants.PaymentSystems.ApcoPaySafetyPayV2:
                case Constants.PaymentSystems.ApcoPayBankTransfer:
                case Constants.PaymentSystems.ApcoPayBoleto:
                case Constants.PaymentSystems.ApcoPayCashPayment:
                    response = ApcoPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.IqWallet:
                    response = IqWalletHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.TotalProcessingVisa:
                case Constants.PaymentSystems.TotalProcessingMaster:
                case Constants.PaymentSystems.TotalProcessingMaestro:
                case Constants.PaymentSystems.TotalProcessingPaysafe:
                    response = TotalProcessingHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Neteller:
                    response = NetellerHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PayOne:
                    response.Status = PaymentRequestStates.PayPanding;
                    break;
                case Constants.PaymentSystems.CardToCard:
                    response = CardToCardHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PerfectMoneyWallet:
                    response = PerfectMoneyHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PerfectMoneyVoucher:
                    response = PerfectMoneyHelpers.CreateVoucher(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.FreeKassaWallet:
                case Constants.PaymentSystems.FreeKassaCard:
                case Constants.PaymentSystems.FreeKassaQIWI:
                case Constants.PaymentSystems.FreeKassaYandex:
                case Constants.PaymentSystems.FreeKassaAdnvCash:
                case Constants.PaymentSystems.FreeKassaExmo:
                case Constants.PaymentSystems.FreeKassaBitcoinCash:
                case Constants.PaymentSystems.FreeKassaOOOPay:
                case Constants.PaymentSystems.FreeKassaTether:
                case Constants.PaymentSystems.FreeKassaMonero:
                case Constants.PaymentSystems.FreeKassaZCash:
                case Constants.PaymentSystems.FreeKassaSkinPay:
                case Constants.PaymentSystems.FreeKassaPayeer:
                case Constants.PaymentSystems.FreeKassaAlfaBank:
                case Constants.PaymentSystems.FreeKassaSberBank:
                case Constants.PaymentSystems.FreeKassaMTS:
                case Constants.PaymentSystems.FreeKassaBeeline:
                case Constants.PaymentSystems.FreeKassaPayPal:
                case Constants.PaymentSystems.FreeKassaTele2:
                    response = FreeKassaHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Freelanceme:
                    response = FreelancemeHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyCash:
                    response = ZippyHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyCashGenerator:
                    response = ZippyHelpers.CreatePayoutGeneratorRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.P2P:
                    response = P2PHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Cartipal:
                case Constants.PaymentSystems.Cartipay:
                    response = CartipalHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.EZeeWallet:
                    response = EZeeWalletHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.EasyPayCard:
                    response = EasyPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.TronLink:
                    response = TronLinkHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaMFT:
                case Constants.PaymentSystems.InstaPapara:
                case Constants.PaymentSystems.InstaPeple:
                    response = InstapayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaCepbank:
                    response = InstapayHelpers.CreateCepbankPayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Ecopayz:
                    response = EcopayzHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Astropay:
                    response = AstropayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Pay4Fun:
                    response = Pay4FunHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.QaicashBankTransfer:
                case Constants.PaymentSystems.QaicashNissinPay:
                case Constants.PaymentSystems.QaicashJPay:
                    response = QaicashHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PraxisCard:
                case Constants.PaymentSystems.PraxisWallet:
                    response = PraxisHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PaymentIQLuxon:
                case Constants.PaymentSystems.PaymentIQSirenPayCard:
                case Constants.PaymentSystems.PaymentIQNeteller:
                case Constants.PaymentSystems.PaymentIQSkrill:
                case Constants.PaymentSystems.PaymentIQCryptoPay:
                case Constants.PaymentSystems.PaymentIQInterac:
                case Constants.PaymentSystems.PaymentIQJeton:
                case Constants.PaymentSystems.PaymentIQPaynetEasyCreditCard:
                case Constants.PaymentSystems.PaymentIQPaynetEasyWebRedirect:
                case Constants.PaymentSystems.PaymentIQPaynetEasyBank:
                    response = PaymenIQHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PaymentAsia:
                    response = PaymentAsiaHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.CryptoPayBTC:
                case Constants.PaymentSystems.CryptoPayBCH:
                case Constants.PaymentSystems.CryptoPayETH:
                case Constants.PaymentSystems.CryptoPayLTC:
                case Constants.PaymentSystems.CryptoPayUSDT:
                case Constants.PaymentSystems.CryptoPayUSDTERC20:
                case Constants.PaymentSystems.CryptoPayUSDC:
                case Constants.PaymentSystems.CryptoPayDAI:
                case Constants.PaymentSystems.CryptoPayXRP:
                case Constants.PaymentSystems.CryptoPayXLM:
                case Constants.PaymentSystems.CryptoPayADA:
                case Constants.PaymentSystems.CryptoPaySHIB:
                case Constants.PaymentSystems.CryptoPaySOL:
                    response = CryptoPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Runpay:
                    response = RunpayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Interac:
                    response = InteracHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.JetonCheckout:
                    response = JetonHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.JetonCash:
                    response = JetonHelpers.CreateVoucher(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Mifinity:
                    response = MifinityHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.CorefyCreditCard:
                case Constants.PaymentSystems.CorefyBankTransfer:
                case Constants.PaymentSystems.CorefyHavale:
                case Constants.PaymentSystems.CorefyPep:
                case Constants.PaymentSystems.CorefyPayFix:
                case Constants.PaymentSystems.CorefyMefete:
                case Constants.PaymentSystems.CorefyParazula:
                case Constants.PaymentSystems.CorefyPapara:
                    response = CorefyHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.NOWPay:
                    response = NOWPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Pix:
                    response = PixHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Transact365:
                    response = Transact365Helpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.CoinsPaid:
                    response = CoinsPaidHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.BRPay:
                    response = BRPayHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                default:
                    response.Status = PaymentRequestStates.Failed;
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            }
            if (response.Status == PaymentRequestStates.Failed)
                throw new Exception(response.Description);
            return response;
        }

        public static PaymentResponse SendPaymentDepositRequest(PaymentRequest paymentRequest, int partnerId, SessionIdentity session, ILog log)
        {
            var paymentResponse = new PaymentResponse { Type = (int)PaymentRequestTypes.Deposit };
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            paymentRequest = UpdatePaymentRequestRegionSettings(paymentRequest, session, log);
            paymentRequest.PaymentSystemName = paymentSystem.Name;

            var notifyUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
            if (string.IsNullOrEmpty(cashierPageUrl))
                cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
            else
                cashierPageUrl = string.Format(cashierPageUrl, session.Domain);

            paymentResponse.CancelUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain) +
                                        string.Format("/notify/NotifyResult?orderId={0}&returnUrl={1}&domain={2}&providerName=Payment&methodName=CancelRequest",
                                        paymentRequest.Id, cashierPageUrl, notifyUrl);
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.KazPost:
                    paymentResponse.Url = KazPostHelpers.CallKazPostApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PayBox:
                case Constants.PaymentSystems.PayBoxMobile:
                case Constants.PaymentSystems.OnlineBanking:
                    paymentResponse.Url = PayBoxHelpers.CallPayBoxApi(paymentRequest, partnerId, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Skrill:
                    paymentResponse.Url = SkrillHelpers.CallSkrillApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.LuckyPay:
                    paymentResponse.Url = LuckyPayHelpers.SendDepositRequestToProvider(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Wooppay:
                    paymentResponse.Url = WooppayHelpers.CallWooppayApi(paymentRequest, partnerId, session, log);
                    break;
                case Constants.PaymentSystems.WooppayMobile:
                    paymentResponse.Url = WooppayHelpers.CallWooppayMobileApi(paymentRequest, partnerId, session, log);
                    break;
                case Constants.PaymentSystems.WalletOne:
                case Constants.PaymentSystems.CreditCards:
                    paymentResponse.Url = WalletOneHelpers.CallWalletOneApi(paymentRequest, partnerId, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.BankTransferSwift:
                case Constants.PaymentSystems.BankTransferSepa:
                case Constants.PaymentSystems.BankWire:
                case Constants.PaymentSystems.CepBank:
                case Constants.PaymentSystems.Bitcoin:
                case Constants.PaymentSystems.ShebaTransfer:
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.QiWiWallet:
                    paymentResponse.Url = QiwiWalletHelpers.CallQiwiWalletApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Help2Pay:
                    paymentResponse.Url = Help2PayHelpers.CallHelp2PayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PaySec:
                    paymentResponse.Url = PaySecHelpers.CallPaySecApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.SDPayQuickPay:
                case Constants.PaymentSystems.SDPayP2P:
                    paymentResponse.Data = SDPayHelpers.CallSDPayApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PayTrust88:
                    paymentResponse.Url = PayTrust88Helpers.CallPayTrust88Api(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PiastrixWallet:
                    paymentResponse.Url = PiastrixHelpers.CallPiastrixBillApi(paymentRequest, cashierPageUrl, session, log);
                    break;

                case Constants.PaymentSystems.PiastrixVisaMaster:
                case Constants.PaymentSystems.PiastrixQiwi:
                case Constants.PaymentSystems.PiastrixYandex:
                case Constants.PaymentSystems.PiastrixPayeer:
                case Constants.PaymentSystems.PiastrixPerfectMoney:
                case Constants.PaymentSystems.PiastrixAlfaclick:
                case Constants.PaymentSystems.PiastrixBTC:
                case Constants.PaymentSystems.PiastrixETH:
                case Constants.PaymentSystems.PiastrixTerminal:
                case Constants.PaymentSystems.PiastrixTether:
                case Constants.PaymentSystems.PiastrixTinkoff:
                    paymentResponse.Url = PiastrixHelpers.CallPiastrixInvoiceApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PiastrixBeeline:
                case Constants.PaymentSystems.PiastrixMTS:
                case Constants.PaymentSystems.PiastrixMegafon:
                case Constants.PaymentSystems.PiastrixTele2:
                    paymentResponse.Data = PiastrixHelpers.CallPiastrixInvoiceApi(paymentRequest, cashierPageUrl, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.ApcoPayAstroPay:
                case Constants.PaymentSystems.ApcoPayEcoPayz:
                case Constants.PaymentSystems.ApcoPayDirect24:
                case Constants.PaymentSystems.ApcoPayVisaMaster:
                case Constants.PaymentSystems.ApcoPayNeoSurf:
                case Constants.PaymentSystems.ApcoPayMuchBetter:
                case Constants.PaymentSystems.ApcoPaySafetyPayV2:
                case Constants.PaymentSystems.ApcoPayBankTransfer:
                case Constants.PaymentSystems.ApcoPayBoleto:
                case Constants.PaymentSystems.ApcoPayCashPayment:
                    paymentResponse.Url = ApcoPayHelpers.CallApcoPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.CardPayCard:
                case Constants.PaymentSystems.CardPayBank:
                case Constants.PaymentSystems.CardPayQIWI:
                case Constants.PaymentSystems.CardPayYandex:
                case Constants.PaymentSystems.CardPayWebMoney:
                case Constants.PaymentSystems.CardPayBoleto:
                case Constants.PaymentSystems.CardPayLoterica:
                case Constants.PaymentSystems.CardPaySpei:
                case Constants.PaymentSystems.CardPayDirectBankingEU:
                    paymentResponse.Url = CardPayHelpers.CallCardPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.IqWallet:
                    var resp = IqWalletHelpers.CallIqWalletApi(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    paymentResponse.Status = resp.Status;
                    paymentResponse.Description = resp.Description;
                    break;
                case Constants.PaymentSystems.TotalProcessingVisa:
                case Constants.PaymentSystems.TotalProcessingMaster:
                case Constants.PaymentSystems.TotalProcessingMaestro:
                case Constants.PaymentSystems.TotalProcessingPaysafe:
                    paymentResponse.Url = TotalProcessingHelpers.CallTotalProcessingApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Neteller:
                    paymentResponse.Url = NetellerHelpers.CallNetellerApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PayOne:
                    paymentResponse.Url = PayOneHelpers.CallPayOneApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PerfectMoneyWallet:
                case Constants.PaymentSystems.PerfectMoneyWire:
                case Constants.PaymentSystems.PerfectMoneyMobile:
                    paymentResponse.Url = PerfectMoneyHelpers.CallPerfectMoneyApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PerfectMoneyVoucher:
                    var vResp = PerfectMoneyHelpers.PayVoucher(paymentRequest, session, log);
                    paymentResponse.Status = vResp.Status;
                    paymentResponse.Description = vResp.Description;
                    break;
                case Constants.PaymentSystems.TronLink:
                    var tResp = TronLinkHelpers.PayVoucher(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    paymentResponse.Status = tResp.Status;
                    paymentResponse.Description = tResp.Description;
                    break;
                case Constants.PaymentSystems.CardToCard:
                    paymentResponse.Url = CardToCardHelpers.CallCardToCardApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.FreeKassaWallet:
                case Constants.PaymentSystems.FreeKassaMasterCard:
                case Constants.PaymentSystems.FreeKassaYoomoney:
                case Constants.PaymentSystems.FreeKassaCard:
                case Constants.PaymentSystems.FreeKassaQIWI:
                case Constants.PaymentSystems.FreeKassaYandex:
                case Constants.PaymentSystems.FreeKassaAdnvCash:
                case Constants.PaymentSystems.FreeKassaExmo:
                case Constants.PaymentSystems.FreeKassaBitcoinCash:
                case Constants.PaymentSystems.FreeKassaBitcoin:
                case Constants.PaymentSystems.FreeKassaLitecoin:
                case Constants.PaymentSystems.FreeKassaEthereum:
                case Constants.PaymentSystems.FreeKassaTRC20:
                case Constants.PaymentSystems.FreeKassaERC20:
                case Constants.PaymentSystems.FreeKassaMir:
                case Constants.PaymentSystems.FreeKassaRipple:
                case Constants.PaymentSystems.FreeKassaOOOPay:
                case Constants.PaymentSystems.FreeKassaTether:
                case Constants.PaymentSystems.FreeKassaMonero:
                case Constants.PaymentSystems.FreeKassaZCash:
                case Constants.PaymentSystems.FreeKassaSkinPay:
                case Constants.PaymentSystems.FreeKassaPayeer:
                case Constants.PaymentSystems.FreeKassaAlfaBank:
                case Constants.PaymentSystems.FreeKassaSberBank:
                case Constants.PaymentSystems.FreeKassaMTS:
                case Constants.PaymentSystems.FreeKassaBeeline:
                case Constants.PaymentSystems.FreeKassaPayPal:
                case Constants.PaymentSystems.FreeKassaTele2:
                case Constants.PaymentSystems.FreeKassaDash:
                case Constants.PaymentSystems.FreeKassaOnlineBank:
                    paymentResponse.Url = FreeKassaHelpers.CallFreeKassaApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyCash:
                    paymentResponse.Url = ZippyHelpers.CallZippyCashInApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyWebpay:
                    paymentResponse.Url = ZippyHelpers.CallZippyWebpayApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyWebpayV2:
                    paymentResponse.Url = ZippyHelpers.CallZippyWebpayV2Api(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyOneClick:
                    paymentResponse.Url = ZippyHelpers.CallZippyOneClickApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.ZippyPayIn:
                case Constants.PaymentSystems.ZippyCard:
                case Constants.PaymentSystems.ZippyBankTransfer:
                    paymentResponse.Url = ZippyHelpers.CallZippyPayInApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Freelanceme:
                    paymentResponse.Url = FreelancemeHelpers.CallFreelancemeApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Omid:
                    paymentResponse.Url = OmidHelpers.CallOmidApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.P2P:
                    paymentResponse.Url = P2PHelpers.CallP2PApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Cartipal:
                case Constants.PaymentSystems.Cartipay:
                    paymentResponse.Url = CartipalHelpers.CallCartipalApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.EasyPayCard:
                    paymentResponse.Url = EasyPayHelpers.CallEasyPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.EZeeWallet:
                    paymentResponse.Url = EZeeWalletHelpers.CallEZeeWalletApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.InstaMFT:
                case Constants.PaymentSystems.InstaPapara:
                case Constants.PaymentSystems.InstaPeple:
                    paymentResponse.Url = InstapayHelpers.CallInstapayApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaKK:
                    paymentResponse.Url = InstapayHelpers.CallInstaApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaCepbank:
                    paymentResponse.Url = InstapayHelpers.CallInstaCepbank(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Ecopayz:
                    paymentResponse.Url = EcopayzHelpers.CallEcopayzApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.EcopayzVoucher:
                    paymentResponse.Url = EcopayzHelpers.PayVoucher(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Astropay:
                    paymentResponse.Url = AstropayHelpers.CallAstroPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.QaicashBankTransfer:
                case Constants.PaymentSystems.QaicashJPay:
                case Constants.PaymentSystems.QaicashBankTransferOnline:
                case Constants.PaymentSystems.QaicashDirect:
                case Constants.PaymentSystems.QaicashOnRampEWallet:
                    paymentResponse.Url = QaicashHelpers.CallQaicashApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.QaicashQR:
                    paymentResponse.Url = QaicashHelpers.CallQaicashApi(paymentRequest, cashierPageUrl, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.Pay4Fun:
                    paymentResponse.Url = Pay4FunHelpers.CallPay4FunApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Praxis:
                    paymentResponse.Url = PraxisHelpers.CallPraxisApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PraxisCard:
                case Constants.PaymentSystems.PraxisWallet:
                    paymentResponse.Url = PraxisHelpers.CallPraxisGatewayApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.FinalPaySkrill:
                case Constants.PaymentSystems.FinalPayCrypto:
                    paymentResponse.Url = FinalPayHelpers.CallFinalPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PaymentIQLuxon:
                case Constants.PaymentSystems.PaymentIQSirenPayCard:
                case Constants.PaymentSystems.PaymentIQSirenPayWebDirect:
                case Constants.PaymentSystems.PaymentIQNeteller:
                case Constants.PaymentSystems.PaymentIQSkrill:
                case Constants.PaymentSystems.PaymentIQCryptoPay:
                case Constants.PaymentSystems.PaymentIQInterac:
                case Constants.PaymentSystems.PaymentIQJeton:
                case Constants.PaymentSystems.PaymentIQPaynetEasyCreditCard:
                case Constants.PaymentSystems.PaymentIQPaynetEasyWebRedirect:
                case Constants.PaymentSystems.PaymentIQPaynetEasyBank:
                    paymentResponse.Url = PaymenIQHelpers.CallPaymentIQApi(paymentRequest, cashierPageUrl, session);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.PaymentAsia:
                    paymentResponse.Url = PaymentAsiaHelpers.CallPaymentAsiaApi(paymentRequest, cashierPageUrl, session, log);
                    break;              
                //case Constants.PaymentSystems.JazzCashierBTC:
                //case Constants.PaymentSystems.JazzCashierETH:
                //case Constants.PaymentSystems.JazzCashierBCH:
                //case Constants.PaymentSystems.JazzCashierLTC:
                //case Constants.PaymentSystems.JazzCashierRia:
                //case Constants.PaymentSystems.JazzCashierMoneyGram:
                //    paymentResponse.Url = JazzCashierHelpers.CallJazzCashierApi(paymentRequest, session, log);
                //    break;
                case Constants.PaymentSystems.IPS:
                    paymentResponse.Url = IPSHelpers.CallIPSApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PayOpPIX:
                case Constants.PaymentSystems.PayOpNeosurf:
                case Constants.PaymentSystems.PayOpRevolut:
                case Constants.PaymentSystems.PayOpLocalBankTransfer:
                case Constants.PaymentSystems.PayOpInstantBankTransfer:
                    paymentResponse.Url = PayOpHelpers.CallPayOpApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.CashLib:
                    paymentResponse.Url = CashLibHelpers.CallCashLibApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Runpay:
                    paymentResponse.Url = RunpayHelpers.CallRunpayApi(paymentRequest);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.DPOPay:
                    paymentResponse.Url = DPOPayHelpers.CallDPOPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Interac:
                    paymentResponse.Url = InteracHelpers.CallInteracApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.OptimumWay:
                    paymentResponse.Url = OptimumWayHelpers.CallOptimumWayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Eway:
                    paymentResponse.Url = EwayHelpers.CallEwayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Impaya:
                    paymentResponse.Url = ImpayaHelpers.CallImpayaApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.JetonCheckout:
                case Constants.PaymentSystems.JetonDirect:
                    paymentResponse.Url = JetonHelpers.CallJetonApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.JetonQR:
                    paymentResponse.Url = JetonHelpers.CallJetonApi(paymentRequest, cashierPageUrl, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.Flexepin:
                    var radeemationResult = FlexepinHelpers.RedeemVoucher(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    paymentResponse.Status = radeemationResult.Status;
                    paymentResponse.Description = radeemationResult.Description;
                    break;
                case Constants.PaymentSystems.Mifinity:
                    paymentResponse.Url = MifinityHelpers.CallMifinityApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.CorefyCreditCard:
                case Constants.PaymentSystems.CorefyBankTransfer:
                case Constants.PaymentSystems.CorefyHavale:
                case Constants.PaymentSystems.CorefyPep:
                case Constants.PaymentSystems.CorefyPayFix:
                case Constants.PaymentSystems.CorefyMefete:
                case Constants.PaymentSystems.CorefyParazula:
                case Constants.PaymentSystems.CorefyPapara:
                    paymentResponse.Url = CorefyHelpers.CallCorefyApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.MoneyPayVisaMaster:
                case Constants.PaymentSystems.MoneyPayAmericanExpress:
                    paymentResponse.Url = MoneyPayHelpers.CallMoneyPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.NOWPay:
                    paymentResponse.Url = NOWPayHelpers.CallNOWPayApi(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.Pix:
                    paymentResponse.Url = PixHelpers.CreateSaleByQRCode(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.Azulpay:
                    paymentResponse.Url = AzulpayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Transact365:
                    paymentResponse.Url = Transact365Helpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.CoinsPaid:
                    paymentResponse.Url = CoinsPaidHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.BRPay:
                    paymentResponse.Url = BRPayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                default:
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            }
            return paymentResponse;
        }

        private static PaymentRequest UpdatePaymentRequestRegionSettings(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var regionBl = new RegionBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId);
                    var regionPath = regionBl.GetRegionPath(paymentInfo.CountryId ?? client.RegionId);
                    if (string.IsNullOrEmpty(paymentInfo.Country))
                    {
                        var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                        paymentInfo.Country = country?.IsoCode;
                        paymentRequest.CountryCode = country?.IsoCode;
                    }
                    else
                    {
                        paymentInfo.Country = paymentInfo.Country;
                        paymentRequest.CountryCode = regionBl.GetRegionByName(paymentInfo.Country, session.LanguageId)?.IsoCode;
                    }
                    if (string.IsNullOrEmpty(paymentInfo.City))
                    {
                        var city = client.City;
                        if (string.IsNullOrEmpty(city))
                        {
                            var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                            if (cityPath != null)
                                city = CacheManager.GetRegionById(cityPath.Id ?? 0, session.LanguageId)?.Name;
                        }
                        paymentInfo.City = city;
                    }
                    else
                        paymentInfo.City = paymentInfo.City;
                    paymentInfo.TransactionIp = session.LoginIp;
                    paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });

                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    return paymentRequest;
                }
            }
        }
    }
}