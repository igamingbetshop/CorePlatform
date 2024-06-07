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
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Helpers;

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
                case Constants.PaymentSystems.CoinsPaidTRX:
                case Constants.PaymentSystems.CoinsPaidBTC:
                case Constants.PaymentSystems.CoinsPaidCPD:
                case Constants.PaymentSystems.CoinsPaidDOGE:
                case Constants.PaymentSystems.CoinsPaidBNB:
                case Constants.PaymentSystems.CoinsPaidADA:
                case Constants.PaymentSystems.CoinsPaidBCH:
                case Constants.PaymentSystems.CoinsPaidBSC:
                case Constants.PaymentSystems.CoinsPaidUSDTT:
                case Constants.PaymentSystems.CoinsPaidETH:
                case Constants.PaymentSystems.CoinsPaidUSDC:
                case Constants.PaymentSystems.CoinsPaidLTC:
                case Constants.PaymentSystems.CoinsPaidUSDTE:
                case Constants.PaymentSystems.CoinsPaidXRP:
                case Constants.PaymentSystems.CoinsPaidBNBBSC:
                    return CoinsPaidHelpers.PaymentRequest(clientId, paymentSystemId, log);
                default:
                    break;
            }

            return new CryptoAddress();
        }

        public static PaymentRequestStates CancelWithdrawalRequest(PaymentRequest paymentRequest, PaymentRequestStates paymentStatus, SessionIdentity session, ILog log)
        {
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.PaymentIQ:
                    paymentStatus = PaymentIQHelpers.CancelPayoutRequest(paymentRequest, session, log);
                    break;
                default:
                    break;
            }
            return paymentStatus;

        }

        public static PayoutCancelationTypes GetPaymentSystemIntegrationType(BllPaymentSystem paymentSystem)
        {
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.PaymentIQ:
                    return PayoutCancelationTypes.ExternallyWithCallback;
                default:
                    return PayoutCancelationTypes.Internally;
            }
        }

        public static void CancelPayoutRequest(BllPaymentSystem paymentSystem, PaymentRequest r, SessionIdentity session, ILog log)
        {
            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.PaymentIQ:
                    PaymentIQHelpers.CancelPayoutRequest(r, session, log);
                    break;
                case Constants.PaymentSystems.PremierCashier:
                    PremierCashierHelpers.CreatePayoutRequest(r, session, log, false);
                    break;
                default:
                    break;
            }
        }

        public static string InitializeWithdrawalsRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var initializeUrl = string.Empty;
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            paymentRequest = UpdatePaymentRequestRegionSettings(paymentRequest, session, log);
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var cashierPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
            if (string.IsNullOrEmpty(cashierPageUrl))
                cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
            else
                cashierPageUrl = string.Format(cashierPageUrl, session.Domain);

            switch (paymentSystem.Name)
            {
                case Constants.PaymentSystems.Praxis:
                case Constants.PaymentSystems.PraxisFiat:
                    initializeUrl = PraxisHelpers.CallPraxisApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.FinalPaySkrill:
                case Constants.PaymentSystems.FinalPayCrypto:
                    initializeUrl = FinalPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PaymentIQ:
                    initializeUrl = PaymentIQHelpers.CallPaymentIQApi(paymentRequest, cashierPageUrl, session);
                    break;
                case Constants.PaymentSystems.PremierCashier:
                    initializeUrl = PremierCashierHelpers.CallPremierCashierApi(paymentRequest, cashierPageUrl, session, log);
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
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
            if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
            {
                switch (verificationPatformId)
                {
                    case (int)VerificationPlatforms.Insic:
                        OASISHelpers.CheckClientStatus(client, null, session.LanguageId, session, log);
                        InsicHelpers.PaymentRequest(client.PartnerId, paymentRequest.ClientId.Value, paymentRequest.Id, paymentRequest.Type, paymentRequest.Amount, log);
                        break;
                    default:
                        break;
                }
            }

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
                case Constants.PaymentSystems.CryptoTransfer:
                    BankTransferHelpers.PayPayoutRequest(paymentRequest, session, log);
                    response.Status = PaymentRequestStates.PayPanding;
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
                    response = SDPayHelpers.CreatePayment(paymentRequest, session, log);
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
                case Constants.PaymentSystems.InstaVipHavale:
                    response = InstapayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaCepbank:
                    response = InstapayHelpers.CreateCepbankPayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaExpressHavale:
                    response = InstapayHelpers.CreateExpressHavalePayoutRequest(paymentRequest, session, log);
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
                case Constants.PaymentSystems.Praxis:
                case Constants.PaymentSystems.PraxisFiat:
                    response = PraxisHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PaymentIQ:
                    response = PaymentIQHelpers.PayPayoutRequest(paymentRequest, session, log);
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
                    response = PaymentIQHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.JazzCashierCreditCard:
                case Constants.PaymentSystems.JazzCashierCreditCard3D:
                case Constants.PaymentSystems.JazzCashierBitcoins:
                case Constants.PaymentSystems.JazzCashierEthereum:
                case Constants.PaymentSystems.JazzCashierLiteCoin:
                case Constants.PaymentSystems.JazzCashierBitCoinCash:
                case Constants.PaymentSystems.JazzCashierSoldana:
                case Constants.PaymentSystems.JazzCashierCardano:
                case Constants.PaymentSystems.JazzCashierDogecoin:
                case Constants.PaymentSystems.JazzCashierUSDT:
                case Constants.PaymentSystems.JazzCashierBinanceUSD:
                case Constants.PaymentSystems.JazzCashierWBTC:
                case Constants.PaymentSystems.JazzCashierUSDC:
                case Constants.PaymentSystems.JazzCashierBNB:
                case Constants.PaymentSystems.JazzCashierEUROC:
                case Constants.PaymentSystems.JazzCashierTRX:
                case Constants.PaymentSystems.JazzCashierCryptoCreditCard:
                case Constants.PaymentSystems.JazzCashierMoneyGram:
                case Constants.PaymentSystems.JazzCashierRia:
                case Constants.PaymentSystems.JazzCashierRemitly:
                case Constants.PaymentSystems.JazzCashierBoss:
                case Constants.PaymentSystems.JazzCashierZelle:
                case Constants.PaymentSystems.JazzCashierPagoefectivo:
                case Constants.PaymentSystems.JazzCashierAstropay:
                case Constants.PaymentSystems.JazzCashierBancoVenezuela:
                case Constants.PaymentSystems.JazzCashierBoleto:
                case Constants.PaymentSystems.JazzCashierJustPay:
                case Constants.PaymentSystems.JazzCashierKhipu:
                case Constants.PaymentSystems.JazzCashierTransbank:
                case Constants.PaymentSystems.JazzCashierMonnet:
                case Constants.PaymentSystems.JazzCashierPagadito:
                case Constants.PaymentSystems.JazzCashierPaycash:
                case Constants.PaymentSystems.JazzCashierMercadopago:
                case Constants.PaymentSystems.JazzCashierPaycips:
                case Constants.PaymentSystems.JazzCashierPix:
                    response = JazzCashierHelpers.CreatePayoutRequest(paymentRequest, session, log);
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
                case Constants.PaymentSystems.CorefyIPague:
                case Constants.PaymentSystems.CorefyMaldoCryptoBTC:
                case Constants.PaymentSystems.CorefyMaldoCryptoETH:
                case Constants.PaymentSystems.CorefyMaldoCryptoETHBEP20:
                case Constants.PaymentSystems.CorefyMaldoCryptoLTC:
                case Constants.PaymentSystems.CorefyMaldoCryptoXRP:
                case Constants.PaymentSystems.CorefyMaldoCryptoXLM:
                case Constants.PaymentSystems.CorefyMaldoCryptoBCH:
                case Constants.PaymentSystems.CorefyMaldoCryptoLINKERC20:
                case Constants.PaymentSystems.CorefyMaldoCryptoUSDCERC20:
                case Constants.PaymentSystems.CorefyMaldoCryptoUSDCBEP20:
                case Constants.PaymentSystems.CorefyMaldoCryptoUSDCTRC20:
                case Constants.PaymentSystems.CorefyMaldoCryptoUSDTERC20:
                case Constants.PaymentSystems.CorefyMaldoCryptoUSDTBEP20:
                case Constants.PaymentSystems.CorefyMaldoCryptoUSDTTRC20:
                    response = CorefyHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.AsPayHavale:
                case Constants.PaymentSystems.AsPayHoppa:
                case Constants.PaymentSystems.AsPayPapara:
                    response = AsPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.PremierCashier:
                     response = PremierCashierHelpers.CreatePayoutRequest(paymentRequest, session, log, true);
                    break;
                case Constants.PaymentSystems.NodaPay:
                    response = NodaPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.MaldoPayBankTransfer:
                case Constants.PaymentSystems.MaldoPayPapara:
                case Constants.PaymentSystems.MaldoPayMefete:
                case Constants.PaymentSystems.MaldoPayPayFix:
                case Constants.PaymentSystems.MaldoPayPix:
                case Constants.PaymentSystems.MaldoPayParazula:
                case Constants.PaymentSystems.MaldoPayCryptoBTC:
                case Constants.PaymentSystems.MaldoPayCryptoETH:
                case Constants.PaymentSystems.MaldoPayCryptoETHBEP20:
                case Constants.PaymentSystems.MaldoPayCryptoLTC:
                case Constants.PaymentSystems.MaldoPayCryptoXRP:
                case Constants.PaymentSystems.MaldoPayCryptoXLM:
                case Constants.PaymentSystems.MaldoPayCryptoBCH:
                case Constants.PaymentSystems.MaldoPayCryptoLINKERC20:
                case Constants.PaymentSystems.MaldoPayCryptoUSDCERC20:
                case Constants.PaymentSystems.MaldoPayCryptoUSDCBEP20:
                case Constants.PaymentSystems.MaldoPayCryptoUSDCTRC20:
                case Constants.PaymentSystems.MaldoPayCryptoUSDTERC20:
                case Constants.PaymentSystems.MaldoPayCryptoUSDTBEP20:
                case Constants.PaymentSystems.MaldoPayCryptoUSDTTRC20:
                    response = MaldoPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.FugaPayBankTransfer:
                case Constants.PaymentSystems.FugaPayPapara:
                case Constants.PaymentSystems.FugaPayPayFix:
                    response = FugaPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.XcoinsPayCrypto:
                    response = XcoinsPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
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
                case Constants.PaymentSystems.CoinsPaidDOGE:
                case Constants.PaymentSystems.CoinsPaidBTC:
                case Constants.PaymentSystems.CoinsPaidCPD:
                case Constants.PaymentSystems.CoinsPaidTRX:
                case Constants.PaymentSystems.CoinsPaidBNB:
                case Constants.PaymentSystems.CoinsPaidADA:
                case Constants.PaymentSystems.CoinsPaidBCH:
                case Constants.PaymentSystems.CoinsPaidBSC:
                case Constants.PaymentSystems.CoinsPaidUSDTT:
                case Constants.PaymentSystems.CoinsPaidETH:
                case Constants.PaymentSystems.CoinsPaidUSDC:
                case Constants.PaymentSystems.CoinsPaidLTC:
                case Constants.PaymentSystems.CoinsPaidUSDTE:
                case Constants.PaymentSystems.CoinsPaidXRP:
                case Constants.PaymentSystems.CoinsPaidBNBBSC:
                    response = CoinsPaidHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.KralPayPapara:
                case Constants.PaymentSystems.KralPayMefete:
                case Constants.PaymentSystems.KralPayCrypto:
                case Constants.PaymentSystems.KralPayBankTransfer:
                    response = KralPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.BRPay:
                    response = BRPayHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InterkassaVisa:
                case Constants.PaymentSystems.InterkassaMC:
                case Constants.PaymentSystems.InterkassaDeVisa:
                case Constants.PaymentSystems.InterkassaDeMC:
                case Constants.PaymentSystems.InterkassaCommunitybanking:
				case Constants.PaymentSystems.InterkassaTrBanking:
				case Constants.PaymentSystems.InterkassaPaybol:
				case Constants.PaymentSystems.InterkassaPayfix:
				case Constants.PaymentSystems.InterkassaPapara:
				case Constants.PaymentSystems.InterkassaJeton:
				case Constants.PaymentSystems.InterkassaMefete:
				case Constants.PaymentSystems.InterkassaPep:
					response = InterkassaHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Ngine:
                    response = NgineHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.IXOPayPayPal:
                case Constants.PaymentSystems.IXOPayTrustly:
                case Constants.PaymentSystems.IXOPayCC:
                    response = IXOPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Paylado:
                    response = PayladoHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.OktoPay:
                    response = OktoPayHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.VevoPayPapara:
                case Constants.PaymentSystems.VevoPayHavale:
                case Constants.PaymentSystems.VevoPayMefete:
                case Constants.PaymentSystems.VevoPayKreditCard:
                case Constants.PaymentSystems.VevoPayPayfix:
                case Constants.PaymentSystems.VevoPayParazula:
                case Constants.PaymentSystems.VevoPayFups:
                case Constants.PaymentSystems.VevoPayCmt:
                case Constants.PaymentSystems.VevoPayPep:
                    response = VevoPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
				case Constants.PaymentSystems.JetonHavale:
					response = JetonHavaleHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
				case Constants.PaymentSystems.EliteBankTransfer:
				case Constants.PaymentSystems.EliteEftFast:
				case Constants.PaymentSystems.ElitePayFix:
				case Constants.PaymentSystems.ElitePapara:
				case Constants.PaymentSystems.EliteParazula:
					response = EliteHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.MaxPay:
                    response = MaxPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Chapa:
                    response = ChapaHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.SantimaPay:
                    response = SantimaPayHelpers.PayoutRequest(paymentRequest, session, log);
                    break;
				case Constants.PaymentSystems.GumballPay:
					response = GumballPayHelpers.ReturnRequest(paymentRequest, session, log);
					break;
				case Constants.PaymentSystems.Yaspa:
					//response = YaspaHelpers.PayoutRequest(paymentRequest, session, log);
					break;
				case Constants.PaymentSystems.QuikiPayCrypto:
					response = QuikiPayHelpers.CreatePayoutRequest(paymentRequest, session, log);
					break;
				case Constants.PaymentSystems.XprizoMpesa:
				case Constants.PaymentSystems.XprizoWallet:
					response = XprizoHelpers.CreatePayoutRequest(paymentRequest, session, log);
					break;
				default:
                    response.Status = PaymentRequestStates.Failed;
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            }
            if (response.Status == PaymentRequestStates.Failed)
                throw new Exception(response.Description);
            return response;
        }

        public static PaymentResponse SendPaymentDepositRequest(PaymentRequest paymentRequest, int partnerId, string goBackUrl, string errorPageUrl, SessionIdentity session, ILog log)
        {
            var paymentResponse = new PaymentResponse { Type = (int)PaymentRequestTypes.Deposit };
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            paymentRequest = UpdatePaymentRequestRegionSettings(paymentRequest, session, log);
            paymentRequest.PaymentSystemName = paymentSystem.Name;

            var verificationPlatform = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.VerificationPlatform);
            if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
            {
                switch (verificationPatformId)
                {
                    case (int)VerificationPlatforms.Insic:
                        OASISHelpers.CheckClientStatus(CacheManager.GetClientById(paymentRequest.ClientId.Value), null, session.LanguageId, session, log);
                        InsicHelpers.PaymentModalityRegistration(partnerId, paymentRequest.ClientId.Value, paymentRequest.PaymentSystemId, session, log);
                        InsicHelpers.PaymentRequest(partnerId, paymentRequest.ClientId.Value, paymentRequest.Id, paymentRequest.Type, paymentRequest.Amount, log);
                        break;
                    case (int)VerificationPlatforms.KRA:
                        KRAHelpers.SendPaymentsInfo(partnerId, paymentRequest.Amount, paymentRequest.CreationTime, log);
                        break;
                    default:
                        break;
                }
            }

            var notifyUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
            if (string.IsNullOrEmpty(cashierPageUrl))
                cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
            else
                cashierPageUrl = string.Format(cashierPageUrl, session.Domain);

            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

            paymentResponse.CancelUrl = string.Format(distributionUrlKey.StringValue, session.Domain) +
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
                case Constants.PaymentSystems.InstaVipHavale:
                    paymentResponse.Url = InstapayHelpers.CallInstaVipHavale(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaKK:
                    paymentResponse.Url = InstapayHelpers.CallInstaApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaCepbank:
                    paymentResponse.Url = InstapayHelpers.CallInstaCepbank(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InstaExpressHavale:
                    paymentResponse.Url = InstapayHelpers.CallInstaExpressHavale(paymentRequest, session, log);
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
                case Constants.PaymentSystems.QaicashBankTransferOnline:
                case Constants.PaymentSystems.QaicashDirect:
                case Constants.PaymentSystems.QaicashOnRampEWallet:
                case Constants.PaymentSystems.QaicashJPay:
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
                case Constants.PaymentSystems.PraxisFiat:
                    paymentResponse.Url = PraxisHelpers.CallPraxisApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.FinalPaySkrill:
                case Constants.PaymentSystems.FinalPayCrypto:
                    paymentResponse.Url = FinalPayHelpers.CallFinalPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PaymentIQ:
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
                case Constants.PaymentSystems.PaymentIQInternalCash:
                case Constants.PaymentSystems.PaymentIQHelp2Pay:
                case Constants.PaymentSystems.PaymentIQFlexepin:
                    paymentResponse.Url = PaymentIQHelpers.CallPaymentIQApi(paymentRequest, cashierPageUrl, session);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.PaymentAsia:
                    paymentResponse.Url = PaymentAsiaHelpers.CallPaymentAsiaApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.JazzCashierCreditCard:
                case Constants.PaymentSystems.JazzCashierCreditCard3D:
                case Constants.PaymentSystems.JazzCashierCrypto:
                case Constants.PaymentSystems.JazzCashierBitcoins:
                case Constants.PaymentSystems.JazzCashierEthereum:
                case Constants.PaymentSystems.JazzCashierLiteCoin:
                case Constants.PaymentSystems.JazzCashierBitCoinCash:
                case Constants.PaymentSystems.JazzCashierSoldana:
                case Constants.PaymentSystems.JazzCashierCardano:
                case Constants.PaymentSystems.JazzCashierDogecoin:
                case Constants.PaymentSystems.JazzCashierUSDT:
                case Constants.PaymentSystems.JazzCashierBinanceUSD:
                case Constants.PaymentSystems.JazzCashierWBTC:
                case Constants.PaymentSystems.JazzCashierUSDC:
                case Constants.PaymentSystems.JazzCashierBNB:
                case Constants.PaymentSystems.JazzCashierEUROC:
                case Constants.PaymentSystems.JazzCashierTRX:
                case Constants.PaymentSystems.JazzCashierCryptoCreditCard:
                case Constants.PaymentSystems.JazzCashierMoneyGram:
                case Constants.PaymentSystems.JazzCashierRia:
                case Constants.PaymentSystems.JazzCashierRemitly:
                case Constants.PaymentSystems.JazzCashierBoss:
                case Constants.PaymentSystems.JazzCashierZelle:
                case Constants.PaymentSystems.JazzCashierPagoefectivo:
                case Constants.PaymentSystems.JazzCashierAstropay:
                case Constants.PaymentSystems.JazzCashierBancoVenezuela:
                case Constants.PaymentSystems.JazzCashierBoleto:
                case Constants.PaymentSystems.JazzCashierJustPay:
                case Constants.PaymentSystems.JazzCashierKhipu:
                case Constants.PaymentSystems.JazzCashierTransbank:
                case Constants.PaymentSystems.JazzCashierMonnet:
                case Constants.PaymentSystems.JazzCashierPagadito:
                case Constants.PaymentSystems.JazzCashierPaycash:
                case Constants.PaymentSystems.JazzCashierMercadopago:
                case Constants.PaymentSystems.JazzCashierPaycips:
                case Constants.PaymentSystems.JazzCashierPix:
                    paymentResponse.Url = JazzCashierHelpers.CallJazzCashierApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.IPS:
                    paymentResponse.Url = IPSHelpers.CallIPSApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PayOpPayDo:
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
                case Constants.PaymentSystems.JetonCash:
                    var result = JetonHelpers.PayVoucher(paymentRequest, cashierPageUrl, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    paymentResponse.Status = result.Status;
                    paymentResponse.Description = result.Description;
                    break;
                case Constants.PaymentSystems.Flexepin:
                    var radeemationResult = FlexepinHelpers.RedeemVoucher(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    paymentResponse.Status = radeemationResult.Status;
                    paymentResponse.Description = radeemationResult.Description;
                    break;
                case Constants.PaymentSystems.FlexPay:
                    paymentResponse.Url = FlexepinHelpers.CallFlexPayApi(paymentRequest, session, log);
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
                case Constants.PaymentSystems.CorefyMaldoCrypto:
                case Constants.PaymentSystems.CorefyIPague:
                    paymentResponse.Url = CorefyHelpers.CallCorefyApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.CryptonPay:
                    paymentResponse.Url = CryptonPayHelpers.CallCryptonPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.AsPayCreditCard:
                case Constants.PaymentSystems.AsPayHavale:
                case Constants.PaymentSystems.AsPayHoppa:
                case Constants.PaymentSystems.AsPayPapara:
                    paymentResponse.Url = AsPayHelpers.CallAsPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.MaldoPayCreditCard:
                case Constants.PaymentSystems.MaldoPayHavale:
                case Constants.PaymentSystems.MaldoPayBankTransfer:
                case Constants.PaymentSystems.MaldoPayInstantlyPapara:
                case Constants.PaymentSystems.MaldoPayPapara:
                case Constants.PaymentSystems.MaldoPayParazula:
                case Constants.PaymentSystems.MaldoPayCrypto:              
                case Constants.PaymentSystems.MaldoPayMefete:
                case Constants.PaymentSystems.MaldoPayPayFix:
                case Constants.PaymentSystems.MaldoPayQR:
                    paymentResponse.Url = MaldoPayHelpers.CallMaldoPay(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.AfriPay:
                    paymentResponse.Url = AfriPayHelpers.CallAfriPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.PremierCashier:
                    paymentResponse.Url = PremierCashierHelpers.CallPremierCashierApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.NodaPay:
                    paymentResponse.Url = NodaPayHelpers.CallNodaPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.FugaPayCreditCard:
                case Constants.PaymentSystems.FugaPayBankTransfer:
                case Constants.PaymentSystems.FugaPayPapara:
                case Constants.PaymentSystems.FugaPayPayFix:
                    paymentResponse.Url = FugaPayHelpers.CallFugaPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.XcoinsPayCrypto:
                case Constants.PaymentSystems.XcoinsPayCard:
                    paymentResponse.Url = XcoinsPayHelpers.CallXcoinsPayApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.Changelly:
                    paymentResponse.Url = ChangellyHelpers.CallChangellyApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.InternationalPSP:
                    paymentResponse.Url = InternationalPSPHelpers.CallPSPApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.KralPayPapara:
                case Constants.PaymentSystems.KralPayMefete:
                case Constants.PaymentSystems.KralPayCrypto:
                case Constants.PaymentSystems.KralPayBankTransfer:
                    paymentResponse.Url = KralPayHelpers.CallKralPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.GatewayPay:
                    paymentResponse.Url = GatewayPayHelpers.CallGatewayPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.FreedomPay:
                    paymentResponse.Url = FreedomPayHelpers.CallFreedomPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.MoneyPayVisaMaster:
                case Constants.PaymentSystems.MoneyPayAmericanExpress:
                    paymentResponse.Url = MoneyPayHelpers.CallMoneyPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.NOWPay:
                    paymentResponse.Url = NOWPayHelpers.CallNOWPayApi(paymentRequest, session, log);
                    paymentResponse.CancelUrl = "https://" + session.Domain;
                    break;
                case Constants.PaymentSystems.NOWPayFiat:
                    paymentResponse.Url = NOWPayHelpers.CallNOWPayFiatApi(paymentRequest, session, log);
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
                //case Constants.PaymentSystems.CoinsPaid:
                //    paymentResponse.Url = CoinsPaidHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                //    break;
                case Constants.PaymentSystems.BRPay:
                    paymentResponse.Url = BRPayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.InterkassaVisa:
                case Constants.PaymentSystems.InterkassaMC:
                case Constants.PaymentSystems.InterkassaDeVisa:
                case Constants.PaymentSystems.InterkassaDeMC:
                case Constants.PaymentSystems.InterkassaCommunitybanking:
                case Constants.PaymentSystems.InterkassaTrBanking:
                case Constants.PaymentSystems.InterkassaKazBanking:
                case Constants.PaymentSystems.InterkassaUzBanking:
                case Constants.PaymentSystems.InterkassaPaybol:
                case Constants.PaymentSystems.InterkassaPayfix:
                case Constants.PaymentSystems.InterkassaPapara:
                case Constants.PaymentSystems.InterkassaJeton:
                case Constants.PaymentSystems.InterkassaMefete:
                case Constants.PaymentSystems.InterkassaPep:
                case Constants.PaymentSystems.InterkassaTrCard:
                case Constants.PaymentSystems.InterkassaAZNCard:
                    paymentResponse.Url = InterkassaHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log); 
                    paymentResponse.CancelUrl = "https://" + session.Domain;
					break;
                case Constants.PaymentSystems.Ngine:
                    paymentResponse.Url = NgineHelpers.CallNgineApi(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.NgineZelle:
                    paymentResponse.Description = NgineHelpers.CallNgineZelleApi(paymentRequest, session, log);
                    break;
                case Constants.PaymentSystems.IXOPayPayPal:
                case Constants.PaymentSystems.IXOPayTrustly:
                case Constants.PaymentSystems.IXOPayPaysafecard:
                case Constants.PaymentSystems.IXOPayCC:
                case Constants.PaymentSystems.IXOPaySkrill:
                case Constants.PaymentSystems.IXOPaySofort:
                case Constants.PaymentSystems.IXOPayNeteller:
                case Constants.PaymentSystems.IXOPayGiropay:
                    paymentResponse.Url = IXOPayHelpers.CallIXOPayApi(paymentRequest, goBackUrl, errorPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.OktoPay:
                    paymentResponse.Description = OktoPayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Paylado:
                    paymentResponse.Url = PayladoHelpers.PaymentRequest(paymentRequest, goBackUrl ?? cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.VevoPayPapara:
                case Constants.PaymentSystems.VevoPayHavale:
                case Constants.PaymentSystems.VevoPayMefete:
                case Constants.PaymentSystems.VevoPayKreditCard:
                case Constants.PaymentSystems.VevoPayPayfix:
                case Constants.PaymentSystems.VevoPayParazula:
                case Constants.PaymentSystems.VevoPayFups:
                case Constants.PaymentSystems.VevoPayCmt:
                case Constants.PaymentSystems.VevoPayPep:
                    paymentResponse.Url = VevoPayHelpers.CallVevoPayApi(paymentRequest, session, log);
                    break;
				case Constants.PaymentSystems.JetonHavale:
					paymentResponse.Url = JetonHavaleHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
					break;
                case Constants.PaymentSystems.FinVert:
                    paymentResponse.Url = FinVertHelpers.CallFinVertApi(paymentRequest, cashierPageUrl, session, log);
                    break;            
				case Constants.PaymentSystems.Stripe:
					paymentResponse.Url = StripeHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
					break;            
				case Constants.PaymentSystems.EliteBankTransfer:
				case Constants.PaymentSystems.EliteEftFast:
				case Constants.PaymentSystems.ElitePayFix:
				case Constants.PaymentSystems.ElitePapara:
				case Constants.PaymentSystems.EliteParazula:
					paymentResponse.Description = EliteHelpers.PaymentRequest(paymentRequest, session, log);
					break;               
                case Constants.PaymentSystems.MaxPay:
                    paymentResponse.Url = MaxPayHelpers.CallMaxPayApi(paymentRequest, cashierPageUrl, session, log);
                    break;              
                case Constants.PaymentSystems.GumballPay:
                    paymentResponse.Url = GumballPayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;          
                case Constants.PaymentSystems.SantimaPay:
                    paymentResponse.Url = SantimaPayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;         
                case Constants.PaymentSystems.Chapa:
                    paymentResponse.Url = ChapaHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;    
                case Constants.PaymentSystems.Telebirr:
                    paymentResponse.Url = TelebirrHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;  
                case Constants.PaymentSystems.Jmitsolutions:
                    paymentResponse.Url = JmitsolutionsHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
                case Constants.PaymentSystems.Yaspa:
                    paymentResponse.Url = YaspaHelpers.PaymentRequest(paymentRequest, cashierPageUrl, log);
                    break;
                case Constants.PaymentSystems.QuikiPay:
				case Constants.PaymentSystems.QuikiPayCrypto:
					paymentResponse.Url = QuikiPayHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
                    break;
				case Constants.PaymentSystems.XprizoWallet:
				case Constants.PaymentSystems.XprizoMpesa:
					paymentResponse.Description = XprizoHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
					break;
				case Constants.PaymentSystems.XprizoCard:
				case Constants.PaymentSystems.XprizoUPI:
					paymentResponse.Url = XprizoHelpers.PaymentRequest(paymentRequest, cashierPageUrl, session, log);
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
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var regionPath = CacheManager.GetRegionPathById(paymentInfo.CountryId ?? client.RegionId);
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