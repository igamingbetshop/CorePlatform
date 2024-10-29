using IqSoft.CP.Common.Enums;
using System;
using System.Collections.Generic;
namespace IqSoft.CP.Common
{
    public static class Constants
    {
        public const string CurrentVersion = "9.1.4";
        public const int MainExternalClientId = 21;
        public const int MainPartnerId = 1000;
        public const string DefaultLanguageId = "en";
        public const string DefaultCurrencyId = "USD";
        public const string DefaultIp = "127.0.0.1";
        public const int DefaultRegionId = 2;
        public readonly static DateTime DefaultDateTime = new DateTime(1920, 1, 1);
        public const int SuccessResponseCode = 0;
        public const string CardReaderClientEmail = "CR@CR.com";
        public const string ExternalClientPrefix = "external_";
        public const string FreeSpinPrefix = "FreeSpin_";

        public const int OwnVersionSecurityQuestionId = 1;
        public const int PlatformProductId = 1;

        public const int SportsbookProductId = 6;
        public readonly static List<int> ExternalSportsbookProductIds = new List<int> { 68000, 78518, 78519 };
        public const int PokerProductId = 9001;
        public const int MahjongProductId = 72001;
        public const int IqSoftSportsbookExternalId = 1000;
        public const int BetShopPaymentSystemId = 1;
        public const string SportGroupName = "Sports";
        public const string CasinoGroupName = "Casino";

        public const int ClosePeriodPeriodicy = 1;
        public const int AddMoneyToPartnerAccountPeriodicy = 1;
        public const int BetShopDailyTicketNumberResetPeriodicy = 24;
        public const int AffiliateMaximumLinksCount = 2000;

        public const decimal Delta1 = 0.1m;
        public const decimal Delta2 = 0.01m;
        public const decimal Delta3 = 0.001m;
        public const decimal Delta4 = 0.0001m;
        public const decimal Delta = 0.00001m;

        public const string UserCustomRoleFormat = "{0} custom role";

        public const int percentOfReportingBets = 2;

        public const string NameRegEx = "^[a-zA-Z0-9]+$";
        public const string PasswordRegex = "((?=^.{8,14}$)((?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])|(?=.*[a-z])(?=.*[!@#$%^&*])(?=.*[0-9])|(?=.*[A-Z])(?=.*[!@#$%^&*])(?=.*[0-9])|(?=.*[A-Z])(?=.*[a-z])(?=.*[!@#$%^&*])))(?!.*[\\s])";
        public const string CreditCardRegex = @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}$|(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}$|3[47][0-9]{13}$|(?:2131|1800|35\d{3})\d{11}";
        public readonly static List<char> CapitalLetters = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        public static class WebSiteConfiguration
        {
            public const string Translations = "Translations";
            public const string Styles = "Styles";
            public const string Registration = "Registration";
            public const string Login = "Login";
            public const string Config = "Config";
            public const string Assets = "Assets";
            public const string WebFragments = "WebFragments";
            public const string MobileFragments = "MobileFragments";
            public const string TermsConditions = "TermsConditions";
            public const string JobArea = "JobArea";
            public const string AccountTabsList = "AccountTabsList";
            public const string HeaderPanel1Menu = "HeaderPanel1Menu";
            public const string HeaderPanel2Menu = "HeaderPanel2Menu";
            public const string HeaderMenu = "HeaderMenu";
            public const string DocumentType = "DocumentType";
            public const string FooterMenu = "FooterMenu";
            public const string Root = "root";
            public const string Fonts = "Fonts";
            public const string Html = "Html";

            public const string MobileCentralMenu = "MobileCentralMenu";
            public const string MobileFooterMenu = "MobileFooterMenu";
            public const string MobileBottomMenu = "MobileBottomMenu";
            public const string MobileMenu = "MobileMenu";
            public const string MobileRightSidebar = "MobileRightSidebar";
            public const string MobileHeaderPanel = "MobileHeaderPanel";
            public const string MobileCodes = "MobileCodes";
            public const string HomeMenu = "HomeMenu";
            public const string MobileHomeMenu = "MobileHomeMenu";
            public const string FullRegister = "FullRegister";
            public const string QuickRegister = "QuickRegister";
            public const string AgentClientRegister = "AgentClientRegister";
            public const string AdminClientRegister = "AdminClientRegister";
            public const string BetshopClientRegister = "BetshopClientRegister";
            public const string ProfileDetails = "ProfileDetails";
            public const string Documentation = "Documentation";
        }
        public static class SubMenuConfiguration
        {
            public const string TermsAndConditions = "Terms_conditions";
        }
        public static class Errors
        {
            public const int EmailCantBeEmpty = 1;
            public const int EmailExists = 2;
            public const int InvalidEmail = 3;
            public const int UserNameCantBeEmpty = 4;
            public const int UserNameExists = 5;
            public const int WrongLoginParameters = 6;
            public const int EmailOrMobileMustBeFilled = 7;
            public const int UserNameCanNotContainMailSymbol = 8;
            public const int UserNameMustContainCharacter = 9;
            public const int MobileExists = 10;
            public const int InvalidMobile = 11;
            public const int DocumentNotVerified = 12;
            public const int ClientBlocked = 13;
            public const int UserBlocked = 14;
            public const int InvalidBirthDate = 15;
            public const int InvalidPassword = 16;
            public const int CanNotCancelRequestInCheckingState = 17;
            public const int PaymentRequestNotFound = 18;
            public const int AccountNotFound = 19;
            public const int CurrencyNotExists = 20;
            public const int GeneralException = 21;
            public const int ClientNotFound = 22;
            public const int WrongParentDocument = 23;
            public const int CashDeskNotFound = 24;
            public const int BetShopNotFound = 25;
            public const int PartnerPaymentSettingNotFound = 26;
            public const int UserNotFound = 27;
            public const int SessionNotFound = 28;
            public const int SessionExpired = 29;
            public const int DocumentNotFound = 30;
            public const int WrongParameters = 31;
            public const int WrongPassword = 32;
            public const int WrongSecurityQuestionAnswer = 33;
            public const int ObjectTypeNotFound = 34;
            public const int EmailNotVerified = 35;
            public const int MobileNumberNotVerified = 36;
            public const int WrongToken = 37;
            public const int TokenExpired = 38;
            public const int WrongOperationAmount = 39;
            public const int PaymentRequestNotAllowed = 40;
            public const int CanNotCancelPayedRequest = 41;
            public const int CanNotAllowRequestFromThisState = 42;
            public const int ProductNotFound = 43;
            public const int PartnerProductSettingNotFound = 44;
            public const int ClientMaxLimitExceeded = 45;
            public const int TransactionAlreadyExists = 46;
            public const int WrongClientId = 47;
            public const int ProductNotAllowedForThisPartner = 48;
            public const int ProductBlockedForThisPartner = 49;
            public const int CanNotConnectCreditAndDebit = 50;
            public const int WrongUserId = 51;
            public const int CashDeskBlocked = 52;
            public const int BetShopBlocked = 53;
            public const int PartnerProductLimitExceeded = 54;
            public const int BetShopLimitExceeded = 55;
            public const int CanNotDeleteRollbackDocument = 56;
            public const int WrongCashierId = 57;
            public const int DocumentAlreadyRollbacked = 58;
            public const int PaymentSystemDepositNotFound = 59;
            public const int PartnerKeyNotFound = 60;
            public const int PaymentSystemNotFound = 61;
            public const int PaymentRequestAlreadyExists = 62;
            public const int DontHavePermission = 63;
            public const int DontHaveAccessToThisPartner = 64;
            public const int BetShopGroupNotFound = 65;
            public const int UserIsNotCashier = 66;
            public const int WrongProductId = 67;
            public const int NotAllowed = 68;
            public const int ClientDocumentAlreadyExists = 69;
            public const int PartnerNotFound = 70;
            public const int LowBalance = 71;
            public const int WrongDocumentId = 72;
            public const int PaymentRequestAlreadyPayed = 73;
            public const int WrongHash = 74;
            public const int WinAlreadyPayed = 75;
            public const int TranslationNotFound = 76;
            public const int WrongProviderId = 77;
            public const int ParentBetShopGroupNotFound = 78;
            public const int ParentBetShopGroupBlocked = 79;
            public const int WrongPartnerId = 80;
            public const int CanNotChangeUserPassword = 81;
            public const int InvalidDataRange = 82;
            public const int CanNotChangePaymentRequestStatus = 83;
            public const int RegionNotFound = 84;
            public const int WrongApiCredentials = 85;
            public const int PartnerPaymentSettingBlocked = 86;
            public const int RoleNotFound = 87;
            public const int CanNotPayFailedRequest = 88;
            public const int RequestAlreadyPayed = 89;
            public const int DocumentRollbacked = 90;
            public const int DocumentAlreadyWinned = 91;
            public const int ClientMobileNumberAlreadyVerified = 92;
            public const int ClientEmailAlreadyVerified = 93;
            public const int CanNotFindVerificationMessage = 94;
            public const int EmailAlreadyVerified = 95;
            public const int MobileNumberAlreadyVerified = 96;
            public const int MobileNumberCantBeEmpty = 97;
            public const int WrongVerificationKey = 98;
            public const int VerificationKeyExpired = 99;
            public const int WrongPaymentRequest = 100;
            public const int AccountTypeNotFound = 101;
            public const int ParentRegionInactive = 102;
            public const int ParterNameAlreadyExists = 103;
            public const int ImpermissibleBetShop = 104;
            public const int ImpermissiblePaymentSetting = 105;
            public const int MethodNotFound = 106;
            public const int ProductAlreadyExists = 107;
            public const int CanNotPayPayedRequest = 108;
            public const int WrongCurrencyId = 109;
            public const int ControllerNotFound = 110;
            public const int ClientIdentityNotFound = 111;
            public const int BonusNotFound = 112;
            public const int PaymentRequestStateChanged = 113;
            public const int PaymentRequestAlreadyRedirected = 114;
            public const int PaymentRequestInValidAmount = 115;
            public const int PaymentRequestInValidDate = 116;
            public const int CantUpdatePaymentRequest = 117;
            public const int PageNotFound = 118;
            public const int WebSiteMenuAlreadyExists = 119;
            public const int RequestExpired = 120;
            public const int ShiftNotFound = 121;
            public const int InvalidUserName = 122;
            public const int WrongDocumentNumber = 123;
            public const int WrongCashCode = 124;
            public const int WrongConvertion = 125;
            public const int MessageNotFound = 126;
            public const int BetAfterRollback = 127;
            public const int WrongOperatorId = 128;
            public const int PromoCodeNotExists = 129;
            public const int InvalidSecretKey = 130;
            public const int PromoCodeExpired = 131;
            public const int ClientPaymentInfoAlreadyExists = 132;
            public const int TicketNotFound = 133;
            public const int WrongInputParameters = 134;
            public const int BankIsUnavailable = 135;
            public const int CantDeleteDocumentFromThisState = 136;
            public const int RoundNotFound = 137;
            public const int RestrictedDestination = 138;
            public const int CanNotPrintTicket = 139;
            public const int ActionNotFound = 140;
            public const int InvalidTwoFactorKey = 141;
            public const int TriggerSettingNotFound = 142;
            public const int TriggerGroupNotFound = 143;
            public const int ClientForceBlocked = 144;
            public const int UserForceBlocked = 145;
            public const int ClientAlreadyHasActiveBonus = 146;
            public const int MaxLimitExceeded = 147;
            public const int AcceptTermsConditions = 148;
            public const int InactivityBlock = 149;
            public const int PromoCodeAlreadyActivated = 150;
            public const int SelfExcluded = 151;
            public const int SystemExcluded = 152;
            public const int CautionSuspension = 153;
            public const int Younger = 154;
            public const int AMLProhibited = 155;
            public const int CommentTemplateNotFound = 156;
            public const int BlockedForBonuses = 157;
            public const int WrongSecurityCode = 158;
            public const int PasswordMatches = 159;
            public const int InvalidSecurityCode = 160;
            public const int NickNameExists = 161;
            public const int SecurityCodeFailed = 162;
            public const int DocumentExpired = 163;
            public const int JCJExcluded = 164;
            public const int SegmentNotFound = 165;
            public const int SegmentExist = 166;
            public const int LevelLimitExceeded = 167;
            public const int WrongAgentLevel = 168;
            public const int WrongCalculationPeriod = 169;
            public const int ClientDailyLimitExceeded = 170;
            public const int ClientWeeklyLimitExceeded = 171;
            public const int ClientMonthlyLimitExceeded = 172;
            public const int UnavailableFreespin = 173;
            public const int DemoNotSupported = 174;
            public const int PromotionNotFound = 175;
            public const int AddressCantBeEmpty = 176;
            public const int FirstNameCantBeEmpty = 177;
            public const int LastNameCantBeEmpty = 178;
            public const int ZipCodeCantBeEmpty = 179;
            public const int AffiliateNotFound = 180;
            public const int WrongUSSDPin = 181;
            public const int USSDPinMatches = 182;
            public const int PasswordContainsPersonalData = 183;
            public const int WrongFileExtension = 184;
            public const int ResponsibleGamingPartnerLimitExceeded = 185;
            public const int ClientExist = 186;
            public const int CharacterNotFound = 187;
            public const int AnnouncementNotFound = 188;
            public const int NewsNotFound = 189;
            public const int PartnerAlreadyHasActiveBonus = 190;
            public const int InvalidJson = 191;
            public const int BonusAlreadyUsed = 192;
        }
        public static class Languages
        {
            public const string English = "en";
            public const string Armenian = "hy";
            public const string Russian = "ru";
            public const string Arabic = "ar";
            public const string Turkish = "tr";
        }
        public static class PaymentSystems
        {
            public const string BetShop = "Cash";

            public const string CyberPlat = "CyberPlat";
            public const string Kassa24 = "Kassa24";
            public const string QiWi = "QiWi";
            public const string QiWiTerminal = "QiWiTerminal";
            public const string TronLink = "TronLink";
            public const string ExternalCashier = "ExternalCashier";
            public const string CustomCashier = "CustomCashier";
            public const string Skrill = "Skrill";
            public const string Neteller = "Neteller";
            public const string WebMoney = "WebMoney";            
            public const string QiWiWallet = "QiWiWallet";
            public const string Wooppay = "Wooppay";
            public const string WooppayMobile = "WooppayMobile";
            public const string PayBox = "PayBox";
            public const string PayBoxATM = "PayBoxATM";
            public const string PayBoxMobile = "PayBoxMobile";
            public const string WalletOne = "WalletOne";
            public const string CreditCards = "CreditCards";
            public const string BankTransferSwift = "BankTransferSwift";
            public const string BankTransferSepa = "BankTransferSepa";
            public const string BankWire = "BankWire";
            public const string CryptoTransfer = "CryptoTransfer";
            public const string CepBank = "CepBank";
            public const string Bitcoin = "Bitcoin";
            public const string KazPost = "Kazpochta";
            public const string Help2Pay = "Help2Pay";
            public const string OnlineBanking = "OnlineBanking";
            public const string PaySec = "PaySec";
            public const string SDPay = "SDPay";
            public const string SDPayQuickPay = "SDPayQuickPay";
            public const string SDPayP2P = "SDPayP2P";
            public const string PayTrust88 = "PayTrust88";
            public const string Piastrix = "Piastrix";
            public const string PiastrixWallet = "PiastrixWallet";
            public const string PiastrixVisaMaster = "PiastrixVisaMaster";
            public const string PiastrixQiwi = "PiastrixQiwi";
            public const string PiastrixYandex = "PiastrixYandex";
            public const string PiastrixPayeer = "PiastrixPayeer";
            public const string PiastrixPerfectMoney = "PiastrixPerfectMoney";
            public const string PiastrixBeeline = "PiastrixBeeline";
            public const string PiastrixMTS = "PiastrixMTS";
            public const string PiastrixMegafon = "PiastrixMegafon";
            public const string PiastrixTele2 = "PiastrixTele2";
            public const string PiastrixAlfaclick = "PiastrixAlfaclick";
            public const string PiastrixBTC = "PiastrixBTC";
            public const string PiastrixETH = "PiastrixETH";
            public const string PiastrixTether = "PiastrixTether";
            public const string PiastrixTerminal = "PiastrixTerminal";
            public const string PiastrixTinkoff = "PiastrixTinkoff";
            public const string CardPay = "CardPay";
            public const string CardPayCard = "CardPayCard";
            public const string CardPayBank = "CardPayBank";
            public const string CardPayQIWI = "CardPayQIWI";
            public const string CardPayYandex = "CardPayYandex";
            public const string CardPayWebMoney = "CardPayWebMoney";
            public const string CardPayBoleto = "CardPayBoleto";
            public const string CardPayLoterica = "CardPayLoterica";
            public const string CardPaySpei = "CardPaySpei";
            public const string CardPayDirectBankingEU = "CardPayDirectBankingEU";

            public const string ApcoPay = "ApcoPay";
            public const string ApcoPayAstroPay = "ApcoPayAstroPay";
            public const string ApcoPayEcoPayz = "ApcoPayEcoPayz";
            public const string ApcoPayDirect24 = "ApcoPayDirect24";
            public const string ApcoPayVisaMaster = "ApcoPayVisaMaster";
            public const string ApcoPayNeoSurf = "ApcoPayNeoSurf";
            public const string ApcoPayMuchBetter = "ApcoPayMuchBetter";
            public const string ApcoPaySafetyPayV2 = "ApcoPaySafetyPayV2";
            public const string ApcoPayCashPayment = "ApcoPayCashPayment";
            public const string ApcoPayBoleto = "ApcoPayBoleto";
            public const string ApcoPayBankTransfer = "ApcoPayBankTransfer";
            public const string IqWallet = "IqWallet";
            public const string TotalProcessing = "TotalProcessing";
            public const string TotalProcessingVisa = "TotalProcessingVisa";
            public const string TotalProcessingMaster = "TotalProcessingMaster";
            public const string TotalProcessingMaestro = "TotalProcessingMaestro";
            public const string TotalProcessingPaysafe = "TotalProcessingPaysafe";
            public const string PayOne = "PayOne";
            public const string PerfectMoney = "PerfectMoney";
            public const string PerfectMoneyWallet = "PerfectMoneyWallet";
            public const string PerfectMoneyVoucher = "PerfectMoneyVoucher";
            public const string PerfectMoneyMobile = "PerfectMoneyMobile";
            public const string PerfectMoneyWire = "PerfectMoneyWire";
            public const string CardToCard = "CardToCard";
            public const string FreeKassa = "FreeKassa";
            public const string FreeKassaWallet = "FreeKassaWallet";
            public const string FreeKassaCard = "FreeKassaCard";
            public const string FreeKassaMasterCard = "FreeKassaMasterCard";
            public const string FreeKassaYoomoney = "FreeKassaYoomoney";
            public const string FreeKassaQIWI = "FreeKassaQIWI";
            public const string FreeKassaYandex = "FreeKassaYandex";
            public const string FreeKassaAdnvCash = "FreeKassaAdnvCash";
            public const string FreeKassaExmo = "FreeKassaExmo";
            public const string FreeKassaBitcoinCash = "FreeKassaBitcoinCash";
            public const string FreeKassaBitcoin = "FreeKassaBitcoin";
            public const string FreeKassaERC20 = "FreeKassaERC20";
            public const string FreeKassaTRC20 = "FreeKassaTRC20";
            public const string FreeKassaLitecoin = "FreeKassaLitecoin";
            public const string FreeKassaEthereum = "FreeKassaEthereum";
            public const string FreeKassaRipple = "FreeKassaRipple";
            public const string FreeKassaOOOPay = "FreeKassaOOOPay";
            public const string FreeKassaTether = "FreeKassaTether";
            public const string FreeKassaMonero = "FreeKassaMonero";
            public const string FreeKassaZCash = "FreeKassaZCash";
            public const string FreeKassaSkinPay = "FreeKassaSkinPay";
            public const string FreeKassaPayeer = "FreeKassaPayeer";
            public const string FreeKassaAlfaBank = "FreeKassaAlfaBank";
            public const string FreeKassaSberBank = "FreeKassaSberBank";
            public const string FreeKassaMTS = "FreeKassaMTS";
            public const string FreeKassaBeeline = "FreeKassaBeeline";
            public const string FreeKassaMegafon = "FreeKassaMegafon";
            public const string FreeKassaTele2 = "FreeKassaTele2";
            public const string FreeKassaPayPal = "FreeKassaPayPal";
            public const string FreeKassaDash = "FreeKassaDash";
            public const string FreeKassaSteamPay = "FreeKassaSteamPay";
            public const string FreeKassaOnlineBank = "FreeKassaOnlineBank";
            public const string FreeKassaPerfectMoney = "FreeKassaPerfectMoney";
            public const string FreeKassaWebMoney = "FreeKassaWebMoney";
            public const string FreeKassaMir = "FreeKassaMir";
            public const string ShebaTransfer = "ShebaTransfer";
            public const string LuckyPay = "LuckyPay";
            public const string ZippyCash = "ZippyCash";
            public const string ZippyCashGenerator = "ZippyCashGenerator";
            public const string ZippyWebpay = "ZippyWebpay";
            public const string ZippyWebpayV2 = "ZippyWebpayV2";
            public const string ZippyOneClick = "ZippyOneClick";
            public const string ZippyPayIn = "ZippyPayIn";
            public const string ZippyCard = "ZippyCard";
            public const string ZippyBankTransfer = "ZippyBankTransfer";

            public const string Freelanceme = "Freelanceme";
            public const string Omid = "Omid";
            public const string P2P = "P2P";
            public const string Cartipal = "Cartipal";
            public const string Cartipay = "Cartipay";
            public const string EasyPay = "EasyPay";
            public const string EasyPayCard = "EasyPayCard";
            public const string EZeeWallet = "EZeeWallet";
            public const string Ecopayz = "Ecopayz";
            public const string EcopayzVoucher = "EcopayzVoucher";

            public const string Instapay = "Instapay";
            public const string InstaMFT = "InstaMFT";
            public const string InstaPapara = "InstaPapara";
            public const string InstaPeple = "InstaPeple";
            public const string InstaKK = "InstaKK";
            public const string InstaCepbank = "InstaCepbank";
            public const string InstaExpressHavale = "InstaExpressHavale";
            public const string InstaVipHavale = "InstaVipHavale";
            public const string InstaCryptoBox = "InstaCryptoBox";

            public const string Astropay = "Astropay";
            public const string Pay4Fun = "Pay4Fun";
            public const string Qaicash = "Qaicash";
            public const string QaicashBankTransfer = "QaicashBankTransfer";
            public const string QaicashBankTransferOnline = "QaicashBankTransferOnline";
            public const string QaicashNissinPay = "QaicashNissinPay";
            public const string QaicashJPay = "QaicashJPay";
            public const string QaicashQR = "QaicashQR";
            public const string QaicashDirect = "QaicashDirect";
            public const string QaicashOnRampEWallet = "QaicashOnRampEWallet";
            public const string FinalPaySkrill = "FinalPaySkrill";
            public const string FinalPayCrypto = "FinalPayCrypto";
            public const string Capital = "Capital";
            public const string Sofort = "Sofort";
            public const string Praxis = "Praxis";
            public const string PraxisFiat = "PraxisFiat";
            public const string PraxisProcessingCC = "PraxisProcessingCC";
            public const string PraxisLivitPay = "PraxisLivitPay";
            public const string PraxisLuxonExpress = "PraxisLuxonExpress";
            public const string PraxisApplePayLunu = "PraxisApplePayLunu";
            public const string PraxisGooglePayLunu = "PraxisGooglePayLunu";
            public const string PraxisMatch2Pay = "PraxisMatch2Pay";
            public const string PraxisLuxonPay = "PraxisLuxonPay";
            public const string PraxisBancontactLunu = "PraxisBancontactLunu";
            public const string PraxisCreditCardLunu = "PraxisCreditCardLunu";
            public const string PraxisInterac = "PraxisInterac";
            public const string PaymentAsia = "PaymentAsia";
            public const string PaymentIQ = "PaymentIQ";
            public const string PaymentIQLuxon = "PaymentIQLuxon";
            public const string PaymentIQSirenPayCard = "PaymentIQSirenPayCard";
            public const string PaymentIQSirenPayWebDirect = "PaymentIQSirenPayWebDirect";
            public const string PaymentIQNeteller = "PaymentIQNeteller";
            public const string PaymentIQSkrill = "PaymentIQSkrill";
            public const string PaymentIQCryptoPay = "PaymentIQCryptoPay";
            public const string PaymentIQInterac = "PaymentIQInterac";
            public const string PaymentIQJeton = "PaymentIQJeton";
            public const string PaymentIQPaynetEasyCreditCard = "PaymentIQPaynetEasyCreditCard";
            public const string PaymentIQPaynetEasyWebRedirect = "PaymentIQPaynetEasyWebRedirect";
            public const string PaymentIQPaynetEasyBank = "PaymentIQPaynetEasyBank";
            public const string PaymentIQInternalCash = "PaymentIQInternalCash";
            public const string PaymentIQHelp2Pay = "PaymentIQHelp2Pay";
            public const string PaymentIQFlexepin = "PaymentIQFlexepin";
            public const string JazzCashierCreditCard = "JazzCashierCreditCard";
            public const string JazzCashierCreditCard3D = "JazzCashierCreditCard3D";
            public const string JazzCashierBitcoins = "JazzCashierBitcoins";
            public const string JazzCashierBitCoinCash = "JazzCashierBitCoinCash";
            public const string JazzCashierEthereum = "JazzCashierEthereum";
            public const string JazzCashierLiteCoin = "JazzCashierLiteCoin";
            public const string JazzCashierCrypto = "JazzCashierCrypto";
            public const string JazzCashierSoldana = "JazzCashierSoldana";
            public const string JazzCashierCardano = "JazzCashierCardano";
            public const string JazzCashierDogecoin = "JazzCashierDogecoin";
            public const string JazzCashierUSDT = "JazzCashierUSDT";
            public const string JazzCashierBinanceUSD = "JazzCashierBinanceUSD";
            public const string JazzCashierWBTC = "JazzCashierWBTC";
            public const string JazzCashierUSDC = "JazzCashierUSDC";
            public const string JazzCashierBNB = "JazzCashierBNB";
            public const string JazzCashierEUROC = "JazzCashierEUROC";
            public const string JazzCashierTRX = "JazzCashierTRX";
            public const string JazzCashierCryptoCreditCard = "JazzCashierCryptoCreditCard";
            public const string JazzCashierMoneyGram = "JazzCashierMoneyGram";
            public const string JazzCashierRia = "JazzCashierRia";
            public const string JazzCashierRemitly = "JazzCashierRemitly";
            public const string JazzCashierBoss = "JazzCashierBoss";
            public const string JazzCashierZelle = "JazzCashierZelle";
            public const string JazzCashierPagoefectivo = "JazzCashierPagoefectivo";
            public const string JazzCashierAstropay = "JazzCashierAstropay";
            public const string JazzCashierBancoVenezuela = "JazzCashierBancoVenezuela";
            public const string JazzCashierBoleto = "JazzCashierBoleto";
            public const string JazzCashierJustPay = "JazzCashierJustPay";
            public const string JazzCashierKhipu = "JazzCashierKhipu";
            public const string JazzCashierTransbank = "JazzCashierTransbank";
            public const string JazzCashierMonnet = "JazzCashierMonnet";
            public const string JazzCashierPagadito = "JazzCashierPagadito";
            public const string JazzCashierPaycash = "JazzCashierPaycash";
            public const string JazzCashierMercadopago = "JazzCashierMercadopago";
            public const string JazzCashierPaycips = "JazzCashierPaycips";
            public const string JazzCashierPix = "JazzCashierPix";

            public const string IPS = "IPS";
            public const string PayOp = "PayOp";
            public const string PayOpNeosurf = "PayOpNeosurf";
            public const string PayOpNeosurfAU = "PayOpNeosurfAU";
            public const string PayOpNeosurfUK = "PayOpNeosurfUK";
            public const string PayOpNeosurfNZ = "PayOpNeosurfNZ";
            public const string PayOpRevolut = "PayOpRevolut";
            public const string PayOpRevolutUK = "PayOpRevolutUK";
            public const string PayOpInstantBankTransfer = "PayOpInstantBankTransfer";
            public const string PayOpLocalBankTransfer = "PayOpLocalBankTransfer";
            public const string PayOpPIX = "PayOpPIX";
            public const string PayOpPayDo = "PayOpPayDo";
            public const string PayOpInterac = "PayOpInterac";
            public const string PayOpMonzo = "PayOpMonzo";
            public const string PayOpCashToCode = "PayOpCashToCode";
            public const string PayOpBankFI = "PayOpBankFI";
            public const string PayOpBankAT = "PayOpBankAT";
            public const string PayOpBankUK = "PayOpBankUK";
            public const string PayOpBankTransferZA = "PayOpBankTransferZA";

            public const string CashLib = "CashLib";
            public const string CryptoPay = "CryptoPay";
            public const string CryptoPayBTC = "CryptoPayBTC";
            public const string CryptoPayBCH = "CryptoPayBCH";
            public const string CryptoPayETH = "CryptoPayETH";
            public const string CryptoPayLTC = "CryptoPayLTC";
            public const string CryptoPayUSDT = "CryptoPayUSDT";
            public const string CryptoPayUSDTERC20 = "CryptoPayUSDTERC20";
            public const string CryptoPayUSDC = "CryptoPayUSDC";
            public const string CryptoPayDAI = "CryptoPayDAI";
            public const string CryptoPayXRP = "CryptoPayXRP";
            public const string CryptoPayXLM = "CryptoPayXLM";
            public const string CryptoPayADA = "CryptoPayADA";
            public const string CryptoPaySHIB = "CryptoPaySHIB";
            public const string CryptoPaySOL = "CryptoPaySOL";
            public const string Runpay = "Runpay";
            public const string DPOPay = "DPOPay";
            public const string Interac = "Interac";
            public const string OptimumWay = "OptimumWay";
            public const string Eway = "Eway";
            public const string Jeton = "Jeton";
            public const string JetonCheckout = "JetonCheckout";
            public const string JetonDirect = "JetonDirect";
            public const string JetonQR = "JetonQR";
            public const string JetonGo = "JetonGo";
            public const string JetonCash = "JetonCash";
            public const string Impaya = "Impaya";
            public const string Flexepin = "Flexepin";
            public const string FlexPay = "FlexPay";
            public const string Mifinity = "Mifinity";
            public const string CryptonPay = "CryptonPay";
            public const string CorefyCreditCard = "CorefyCreditCard";
            public const string CorefyBankTransfer = "CorefyBankTransfer";
            public const string CorefyHavale = "CorefyHavale";
            public const string CorefyPep = "CorefyPep";
            public const string CorefyPayFix = "CorefyPayFix";
            public const string CorefyMefete = "CorefyMefete";
            public const string CorefyParazula = "CorefyParazula";
            public const string CorefyPapara = "CorefyPapara";
            public const string CorefyIPague = "CorefyIPague";
            public const string CorefyMaldoCrypto = "CorefyMaldoCrypto";
            public const string CorefyMaldoCryptoBTC = "CorefyMaldoCryptoBTC";
            public const string CorefyMaldoCryptoETH = "CorefyMaldoCryptoETH";
            public const string CorefyMaldoCryptoETHBEP20 = "CorefyMaldoCryptoETHBEP20";
            public const string CorefyMaldoCryptoLTC = "CorefyMaldoCryptoLTC";
            public const string CorefyMaldoCryptoXRP = "CorefyMaldoCryptoXRP";
            public const string CorefyMaldoCryptoXLM = "CorefyMaldoCryptoXLM";
            public const string CorefyMaldoCryptoBCH = "CorefyMaldoCryptoBCH";
            public const string CorefyMaldoCryptoLINKERC20 = "CorefyMaldoCryptoLINKERC20";
            public const string CorefyMaldoCryptoUSDCERC20 = "CorefyMaldoCryptoUSDCERC20";
            public const string CorefyMaldoCryptoUSDCBEP20 = "CorefyMaldoCryptoUSDCBEP20";
            public const string CorefyMaldoCryptoUSDCTRC20 = "CorefyMaldoCryptoUSDCTRC20";
            public const string CorefyMaldoCryptoUSDTERC20 = "CorefyMaldoCryptoUSDTERC20";
            public const string CorefyMaldoCryptoUSDTBEP20 = "CorefyMaldoCryptoUSDTBEP20";
            public const string CorefyMaldoCryptoUSDTTRC20 = "CorefyMaldoCryptoUSDTTRC20";
            public const string AsPay = "AsPay";
            public const string AsPayHavale = "AsPayHavale";
            public const string AsPayCreditCard = "AsPayCreditCard";
            public const string AsPayHoppa = "AsPayHoppa";
            public const string AsPayPapara = "AsPayPapara";
            public const string MaldoPay = "MaldoPay";
            public const string MaldoPayHavale = "MaldoPayHavale";
            public const string MaldoPayBankTransfer = "MaldoPayBankTransfer";
            public const string MaldoPayInstantlyPapara = "MaldoPayInstantlyPapara";
            public const string MaldoPayPapara = "MaldoPayPapara";
            public const string MaldoPayCreditCard = "MaldoPayCreditCard";
            public const string MaldoPayMefete = "MaldoPayMefete";
            public const string MaldoPayPayFix = "MaldoPayPayFix";
            public const string MaldoPayPix = "MaldoPayPix";
            public const string MaldoPayParazula = "MaldoPayParazula";
            public const string MaldoPayQR = "MaldoPayQR";
            public const string MaldoPayCrypto = "MaldoPayCrypto";
            public const string MaldoPayCryptoBTC = "MaldoPayCryptoBTC";
            public const string MaldoPayCryptoETH = "MaldoPayCryptoETH";
            public const string MaldoPayCryptoETHBEP20 = "MaldoPayCryptoETHBEP20";
            public const string MaldoPayCryptoLTC = "MaldoPayCryptoLTC";
            public const string MaldoPayCryptoXRP = "MaldoPayCryptoXRP";
            public const string MaldoPayCryptoXLM = "MaldoPayCryptoXLM";
            public const string MaldoPayCryptoBCH = "MaldoPayCryptoBCH";
            public const string MaldoPayCryptoLINKERC20 = "MaldoPayCryptoLINKERC20";
            public const string MaldoPayCryptoUSDCERC20 = "MaldoPayCryptoUSDCERC20";
            public const string MaldoPayCryptoUSDCBEP20 = "MaldoPayCryptoUSDCBEP20";
            public const string MaldoPayCryptoUSDCTRC20 = "MaldoPayCryptoUSDCTRC20";
            public const string MaldoPayCryptoUSDTERC20 = "MaldoPayCryptoUSDTERC20";
            public const string MaldoPayCryptoUSDTBEP20 = "MaldoPayCryptoUSDTBEP20";
            public const string MaldoPayCryptoUSDTTRC20 = "MaldoPayCryptoUSDTTRC20";
            public const string AfriPay = "AfriPay";
            public const string PremierCashier = "PremierCashier";
            public const string PremierCashierWallet = "PremierCashierWallet";
            public const string PremierCashierCard = "PremierCashierCard";
            public const string PremierCashierCrypto = "PremierCashierCrypto";
            public const string PremierCashierECheck = "PremierCashierECheck";
            public const string PremierCashierMoneyTransfer = "PremierCashierMoneyTransfer";
            public const string PremierCashierManual = "PremierCashierManual";
            public const string NodaPay = "NodaPay";
            public const string FugaPayCreditCard = "FugaPayCreditCard";
            public const string FugaPayBankTransfer = "FugaPayBankTransfer";
            public const string FugaPayPapara = "FugaPayPapara";
            public const string FugaPayPayFix = "FugaPayPayFix";
            public const string XcoinsPayCrypto = "XcoinsPayCrypto";
            public const string XcoinsPayCard = "XcoinsPayCard";
            public const string Changelly = "Changelly";
            public const string ChangellyCard = "ChangellyCard";
            public const string ChangellyBankTransfer = "ChangellyBankTransfer";
            public const string InternationalPSP = "InternationalPSP";
            public const string KralPay = "KralPay";
            public const string KralPayPapara = "KralPayPapara";
            public const string KralPayMefete = "KralPayMefete";
            public const string KralPayBankTransfer = "KralPayBankTransfer";
            public const string KralPayCrypto = "KralPayCrypto";
            public const string GatewayPay = "GatewayPay";
            public const string FreedomPay = "FreedomPay";
            public const string Mpesa = "Mpesa";
            public const string MpesaB2C = "MpesaB2C";
            public const string MpesaC2B = "MpesaC2B";
            public const string MpesaPayBill = "MpesaPayBill";
            public const string PayStageCard = "PayStageCard";
            public const string PayStageWallet = "PayStageWallet";
            public const string PayStageGCash = "PayStageGCash";
            public const string PayStageGrabPay = "PayStageGrabPay";
            public const string PayStageUnionBank = "PayStageUnionBank";
            public const string PayStageBankTransfer = "PayStageBankTransfer";
            public const string PayStagePalawan = "PayStagePalawan";
            public const string PayStageCebuana = "PayStageCebuana";
            public const string PayStagePaygate = "PayStagePaygate";
            public const string PayStageInstapay = "PayStageInstapay";
            public const string PayStageQR = "PayStageQR";
            public const string PayStagePesonet = "PayStagePesonet";
            public const string Nixxe = "Nixxe";
            public const string LiberSave = "LiberSave";
            public const string LiberSaveRevolutGB = "LiberSaveRevolutGB";
            public const string LiberSaveRevolutEU = "LiberSaveRevolutEU";
            public const string LiberSaveMonzoGB = "LiberSaveMonzoGB";
            public const string LiberSavePayPalGB = "LiberSavePayPalGB";
            public const string LiberSaveSepaEU = "LiberSaveSepaEU";
            public const string WebPaysCreditCard = "WebPaysCreditCard";
            public const string WebPaysAPMs= "WebPaysAPMs";
            public const string SpayzBankCard= "SpayzBankCard";
            public const string SpayzOpenBanking= "SpayzOpenBanking";
            public const string CashBullsPix = "CashBullsPix";
            public const string CashBullsBlik = "CashBullsBlik";
            public const string CashBullsOpenBanking = "CashBullsOpenBanking";

            public const string MoneyPayVisaMaster = "MoneyPayVisaMaster";
            public const string MoneyPayAmericanExpress = "MoneyPayAmericanExpress";
            public const string NOWPay = "NOWPay";
            public const string NOWPayFiat = "NOWPayFiat";
            public const string NowPayBTC = "NowPayBTC";
            public const string NowPayBCH = "NowPayBCH";
            public const string NowPayETH = "NowPayETH";
            public const string NowPayLTC = "NowPayLTC";
            public const string NowPayUSDTTRC20 = "NowPayUSDTTRC20";
            public const string NowPayUSDTERC20 = "NowPayUSDTERC20";
            public const string NowPayUSDTBSC = "NowPayUSDTBSC";
            public const string NowPayUSDTSOL = "NowPayUSDTSOL";
            public const string NowPayUSDTPolygon = "NowPayUSDTMATIC";
            public const string NowPayUSDC = "NowPayUSDC";
            public const string NowPayUSDCPolygon = "NowPayUSDCMATIC";
            public const string NowPayDAI = "NowPayDAI";
            public const string NowPayXRP = "NowPayXRP";
            public const string NowPayXLM = "NowPayXLM";
            public const string NowPayADA = "NowPayADA";
            public const string NowPaySHIB = "NowPaySHIB";
            public const string NowPaySOL = "NowPaySOL";
            public const string NowPayBUSDBSC = "NowPayBUSDBSC";
            public const string NowPayBUSDBNB = "NowPayBUSDBNB";
            public const string NowPayBUSD = "NowPayBUSD";
            public const string NowPayPYUSD = "NowPayPYUSD";
            public const string NowPayUSDP = "NowPayUSDP";
            public const string NowPayXNO = "NowPayXNO";
            public const string NowPayXMR = "NowPayXMR";
            public const string NowPayVET = "NowPayVET";
            public const string NowPayUNI = "NowPayUNI";
            public const string NowPayTRX = "NowPayTRX";
            public const string NowPayDGB = "NowPayDGB";
            public const string NowPayDASH = "NowPayDASH";
            public const string Pix = "Pix";
            public const string Azulpay = "Azulpay";
            public const string Transact365 = "Transact365";
            public const string BRPay = "BRPay";
            public const string InterkassaVisa = "InterkassaVisa";
            public const string InterkassaMC = "InterkassaMasterCard";
            public const string InterkassaDeVisa = "InterkassaDeVisa";
            public const string InterkassaDeMC = "InterkassaDeMC";
            public const string InterkassaCommunitybanking = "InterkassaCommunitybanking";
            public const string InterkassaTrBanking = "InterkassaTrBanking";
            public const string InterkassaKazBanking = "InterkassaKazBanking";
            public const string InterkassaUzBanking = "InterkassaUzBanking";
            public const string InterkassaPaybol = "InterkassaPaybol";
            public const string InterkassaPayfix = "InterkassaPayfix";
            public const string InterkassaPapara = "InterkassaPapara";
            public const string InterkassaJeton = "InterkassaJeton";
            public const string InterkassaMefete = "InterkassaMefete";
            public const string InterkassaPep = "InterkassaPep";
            public const string InterkassaTrCard = "InterkassaTrCard";
            public const string InterkassaAZNCard = "InterkassaAZNCard";
            public const string Ngine = "Ngine";
            public const string NgineZelle = "NgineZelle";
            public const string IXOPayPayPal = "IXOPayPayPal";
            public const string IXOPayTrustly = "IXOPayTrustly";
            public const string IXOPayPaysafecard = "IXOPayPaysafecard";
            public const string IXOPayCC = "IXOPayCC";
            public const string IXOPaySkrill = "IXOPaySkrill";
            public const string IXOPayGiropay = "IXOPayGiropay";
            public const string IXOPaySofort = "IXOPaySofort";
            public const string IXOPayNeteller = "IXOPayNeteller";
            public const string OktoPay = "OktoPay";
            public const string JetonHavale = "JetonHavale";
            public const string Stripe = "Stripe";
            public const string CoinsPaid = "CoinsPaid";
            public const string CoinsPaidTRX = "CoinsPaidTRX";
            public const string CoinsPaidCPD = "CoinsPaidCPD";
            public const string CoinsPaidBTC = "CoinsPaidBTC";
            public const string CoinsPaidDOGE = "CoinsPaidDOGE";
            public const string CoinsPaidBNB = "CoinsPaidBNB";
            public const string CoinsPaidADA = "CoinsPaidADA";
            public const string CoinsPaidBCH = "CoinsPaidBCH";
            public const string CoinsPaidBSC = "CoinsPaidBSC";
            public const string CoinsPaidUSDTT = "CoinsPaidUSDTT";
            public const string CoinsPaidETH = "CoinsPaidETH";
            public const string CoinsPaidUSDC = "CoinsPaidUSDC";
            public const string CoinsPaidLTC = "CoinsPaidLTC";
            public const string CoinsPaidUSDTE = "CoinsPaidUSDTE";
            public const string CoinsPaidXRP = "CoinsPaidXRP";
            public const string CoinsPaidBNBBSC = "CoinsPaidBNBBSC";
            public const string Paylado = "Paylado";
            public const string VevoPayPapara = "VevoPayPapara";
            public const string VevoPayHavale = "VevoPayHavale";
            public const string VevoPayMefete = "VevoPayMefete";
            public const string VevoPayKreditCard = "VevoPayKredikarti";
            public const string VevoPayPayfix = "VevoPayPayfix";
            public const string VevoPayParazula = "VevoPayParazula";
            public const string VevoPayFups = "VevoPayFups";
            public const string VevoPayCmt = "VevoPayCmt";
            public const string VevoPayPep = "VevoPayPep";
            public const string FinVert = "FinVert";
            public const string EliteBankTransfer = "EliteBankTransfer";
            public const string EliteEftFast = "EliteEftFast";
            public const string ElitePayFix = "ElitePayFix";
            public const string ElitePapara = "ElitePapara";
            public const string EliteParazula = "EliteParazula";
            public const string MaxPay = "MaxPay";
            public const string GumballPay = "GumballPay";
            public const string SantimaPay = "SantimaPay";
            public const string Chapa = "Chapa";
            public const string Telebirr = "Telebirr";
            public const string JmitSolutions = "JmitSolutions";
            public const string Yaspa = "Yaspa";
            public const string QuikiPay = "QuikiPay";
            public const string QuikiPayCrypto = "QuikiPayCrypto";
            public const string XprizoCard = "XprizoCard";
            public const string XprizoWallet = "XprizoWallet";
            public const string XprizoMpesa = "XprizoMpesa";
            public const string XprizoUPI = "XprizoUPI";
            public const string Huch = "Huch";
            public const string Katarun = "Katarun";
            public const string MPayEFT = "MPayEFT";
            public const string MPayPapara = "MPayPapara";
            public const string MPayCreditCard = "MPayCreditCard";
            public const string MPayVIPCreditCard = "MPayVIPCreditCard";
            public const string MPayPayfix = "MPayPayfix";
            public const string MPayMefete = "MPayMefete";
            public const string MPayParazula = "MPayParazula";
            public const string MPayQR = "MPayQR";
            public const string MPayPopy = "MPayPopy";
            public const string MPayPayco = "MPayPayco";
            public const string MPayFastHavale = "MPayFastHavale";
            public const string WzrdPay = "WzrdPay";
            public const string WzrdPayCreditCard = "WzrdPayCreditCard";
            public const string GetaPay = "GetaPay";
            public const string Omno = "Omno";
            public const string Omer = "Omer";

        }
        public static List<string> ManualPaymentSystems { get; private set; } = new List<string>
        {
            PaymentSystems.BankTransferSepa,
            PaymentSystems.BankTransferSwift,
            PaymentSystems.BankWire
        };

        public static List<string> VoucherPaymentSystems { get; private set; } = new List<string>
        {
            PaymentSystems.PerfectMoneyVoucher,
            PaymentSystems.TronLink
        };
        public static class GameProviders
        {
            public const string Internal = "Internal";
            public const string IqSoft = "IQSoft";
            public const string TwoWinPower = "TwoWinPower";
            public const string DriveMedia = "DriveMedia";
            public const string BetGames = "BetGames";
            public const string Ezugi = "Ezugi";
            public const string TomHorn = "TomHorn";
            public const string ZeusPlay = "ZeusPlay";
            public const string Singular = "Singular";
            public const string EvenBet = "EvenBet";
            public const string EkkoSpin = "EkkoSpin";
            public const string SkyWind = "SkyWind";
            public const string ISoftBet = "ISoftBet";
            public const string SolidGaming = "SolidGaming";
            public const string YSB = "YSB";
            public const string InBet = "InBet";
            public const string SkyCity = "SkyCity";
            public const string CModule = "CModule";
            public const string SunCity = "SunCity";
            public const string ESport = "ESport";
            public const string Igrosoft = "Igrosoft";
            public const string Endorphina = "Endorphina";
            public const string Ganapati = "Ganapati";
            public const string Evolution = "Evolution";
            public const string TVBet = "TVBet";
            public const string GlobalSlots = "GlobalSlots";
            public const string OutcomeBet = "OutcomeBet";
            public const string Mascot = "Mascot";
            public const string PragmaticPlay = "PragmaticPlay";
            public const string LuckyGames = "LuckyGames";
            public const string SoftGaming = "SoftGaming";
            public const string BlueOcean = "BlueOcean";
            public const string SmartSoft = "SmartSoft";
            public const string SoftSwiss = "SoftSwiss";
            public const string Kiron = "Kiron";
            public const string BetSoft = "BetSoft";
            public const string AWC = "AWC";
            public const string Habanero = "Habanero";
            public const string Evoplay = "Evoplay";
            public const string GMW = "GMW";
            public const string BetSolutions = "BetSolutions";
            public const string GrooveGaming = "GrooveGaming";
            public const string Betsy = "Betsy";
            public const string PropsBuilder = "PropsBuilder";
            public const string Racebook = "Racebook";
            public const string EveryMatrix = "EveryMatrix";
            public const string Nucleus = "Nucleus";
            public const string Mancala = "Mancala";
            public const string VisionaryiGaming = "VisionaryiGaming";
            public const string DragonGaming = "DragonGaming";
            public const string GoldenRace = "GoldenRace";
            public const string BAS = "BAS";
            public const string DGS = "DGS";
            public const string JackpotGaming = "JackpotGaming";
            public const string IPTGaming = "IPTGaming";
            public const string TurboGames = "TurboGames";
            public const string AleaPlay = "AleaPlay";
            public const string PlaynGo = "PlaynGo";
            public const string AleaPartners = "AleaPartners";
            public const string BetMakers = "BetMakers";
            public const string RelaxGaming = "RelaxGaming";
            public const string Pixmove = "Pixmove";

            public const string NetEnt = "NetEnt";
            public const string RedTiger = "RedTiger";

            public const string Neskill = "Neskill";
            public const string MicroGaming = "MicroGaming";

            public const string Igromat = "Igromat";
            public const string Mahjong = "Mahjong";
            public const string LuckyGaming = "LuckyGaming";
            public const string Elite = "Elite";
            public const string SoftLand = "SoftLand";
            public const string BGGames = "BGGames";
            public const string TimelessTech = "TimelessTech";
            public const string BCWGames = "BCWGames";
            public const string Fiverscool = "Fiverscool";
            public const string RiseUp = "RiseUp";
            public const string LuckyStreak = "LuckyStreak";
            public const string ImperiumGames = "ImperiumGames";
            public const string Spinomenal = "Spinomenal";
        }
        public static class CashDeskStates
        {
            public const int Active = 1;
            public const int BlockedForWithdraw = 2;
            public const int BlockedForDeposit = 3;
            public const int Blocked = 4;
        }
        public static class BetShopGroupStates
        {
            public const int Active = 1;
            public const int ManuallyBlocked = 2;
            public const int AutomaticallyBlocked = 2;
        }
        public class Games
        {
            public const int Sportsbook = 6;
            public const int IqSoftSportsbook = 1000;
            public const int Keno = 1004;
            public const int BetOnPoker = 1005;
            public const int BetOnRacing = 1006;
            public const int Bingo37 = 1001;
            public const int Colors = 1002;
            public const int Bingo48 = 1003;
        }

        public static Dictionary<int, string> GamesExternalIds = new Dictionary<int, string>
        {
            {Games.Keno, "101"},
            {Games.BetOnPoker, "102"},
            {Games.BetOnRacing, "103"},
            {Games.Sportsbook, "1000"},
            {Games.Bingo37, "104"},
            {Games.Colors, "106"},
            {Games.Bingo48, "107"}
        };
        public static class HttpRequestMethods
        {
            public const string Post = "POST";
            public const string Get = "GET";
            public const string Delete = "DELETE";
            public const string Put = "PUT";
            public const string Patch = "PATCH";
        }

        public static class HttpContentTypes
        {
            public const string ApplicationJson = "application/json";
            public const string ApplicationXml = "application/xml";
            public const string ApplicationUrlEncoded = "application/x-www-form-urlencoded";
            public const string TextXml = "text/xml";
            public const string TextJson = "text/json";
            public const string TextHtml = "text/html";
            public const string TextPlain = "text/plain";
        }

        public static class CacheItems
        {
            public const string Partners = "Partners";
            public const string PartnerSetting = "PartnerSetting";
            public const string PartnerExternalToken = "PartnerExternalToken";
            public const string Products = "Products";
            public const string ProductGroups = "ProductGroups";
            public const string ClientProductCategories = "ClientProductCategories";
            public const string PartnerProductCategories = "PartnerProductCategories";
            public const string Translation = "Translation";
            public const string ProductLimits = "ProductLimits";
            public const string BetshopGroups = "BetshopGroups";

            public const string PartnerProductSettingPages = "PartnerProductSettingPages";
            public const string PartnerProductSettings = "PartnerProductSettings";

            public const string PartnerProducts = "PartnerProducts";
            public const string fnPartnerProductSettings = "fnPartnerProductSettings";
            public const string ProductCountrySetting = "ProductCountrySetting";
            public const string Permissions = "Permissions";
            public const string UserPermissions = "UserPermissions";
            public const string AccessObjects = "AccessObjects";
            public const string GameProviders = "GameProviders";
            public const string RestrictedGameProviders = "RestrictedGameProviders";
            public const string ObjectTypes = "ObjectTypes";
            public const string CashDesks = "CashDesks";
            public const string Enumerations = "Enumerations";
            public const string Currencies = "Currencies";
            public const string Regions = "Regions";
            public const string OnlineClients = "OnlineClients";
            public const string fnProducts = "fnProducts";
            public const string fnRegions = "fnRegions";
            public const string RealTimeInfo = "RealTimeInfo";
            public const string ClientUnreadMessagesCount = "ClientUnreadMessagesCount";
            public const string Categories = "Categories";
            public const string AccountTypes = "AccountTypes";
            public const string DateDiff = "DateDiff";
            public const string fnErrorTypes = "fnErrorTypes";
            public const string ClientClassifications = "ClientClassifications";
            public const string ClientCounts = "ClientCounts";
            public const string ClientSessions = "ClientSessions";
            public const string SessionTime = "SessionTime";

            public const string ClientInactiveSessions = "ClientInactiveSessions";
            public const string ClientCategories = "ClientCategories";
            public const string Languages = "Languages";
            public const string ObjectCurrencyPriorities = "ObjectCurrencyPriorities";
            public const string Clients = "Clients";
            public const string ClientDeposit = "ClientDeposit";
            public const string ClientLastBet = "ClientLastBet";
            public const string ClientLastSportBet = "ClientLastSportBet";
            public const string ClientBalance = "ClientBalance";
            public const string ClientSettings = "ClientSettings";
            public const string ClientPaymentSetting = "ClientPaymentSetting";
            public const string AccountBalances = "AccountBalances";
            public const string AccountTypePriorities = "AccountTypePriorities";
            public const string Accounts = "Accounts";
            public const string BetShops = "BetShops";
            public const string PaymentSystems = "PaymentSystems";
            public const string Merchants = "Merchants";
            public const string ClientTicketUnreadMessagesCount = "ClientTicketUnreadMessagesCount";
            public const string ClientUnreadTicketsCount = "ClientUnreadTicketsCount";
            public const string TicketUnreadMessagesCount = "TicketUnreadMessagesCount";
            public const string LiveGamesLobbyItems = "LiveGamesLobbyItems";
            public const string BonusInfo = "BonusInfo";
            public const string AggregatedFreeSpin = "AggregatedFreeSpin";
            public const string Banners = "Banners";
            public const string Promotions = "Promotions";
            public const string News = "News";
            public const string Popups = "Popups";
            public const string ClientPopups = "ClientPopups";
            public const string Restrictions = "Restrictions";
            public const string ActiveBonusId = "ActiveBonusId";
            public const string BonusProducts = "BonusProducts";
            public const string ComplimantaryPointRates = "ComplimantaryPointRates";
            public const string Region = "Region";
            public const string RegionPath = "RegionPath";
            public const string Country = "Country";
            public const string PartnerCountrySetting = "PartnerCountrySetting";
            public const string GetWebSiteTranslations = "GetWebSiteTranslations";
            public const string User = "User";
            public const string PartnerCurrencies = "PartnerCurrencies";
            public const string SegmentSetting = "SegmentSetting";
            public const string Ticker = "Ticker";
            public const string Action = "Action";
            public const string LastIps = "LastIps";
            public const string ClientFailedLoginCount = "ClientFailedLoginCount";
            public const string VerificationCodeRequestCount = "VerificationCodeRequestCount";
            public const string UserFailedLoginCount = "UserFailedLoginCount";
            public const string UserFailedSecurityCodeCount = "UserFailedSecurityCodeCount";
            public const string CRMApiRequestsCount = "CRMApiRequestsCount";
            public const string UserSettings = "UserSettings";
            public const string UserConfiguration = "UserConfiguration";
            public const string NotAwardedCampaigns = "NotAwardedCampaigns";
            public const string TriggerSettings = "TriggerSettings";
            public const string TotalDepositAmounts = "TotalDepositAmounts";
            public const string TotalBetAmounts = "TotalBetAmounts";
            public const string TotalLossAmounts = "TotalLossAmounts";
            public const string ConfigParameters = "ConfigParameters";
            public const string WhitelistedIps = "WhitelistedIps";
            public const string JobAreas = "JobAreas";
            public const string ClientBonus = "ClientBonus";
            public const string AleaRollback = "AleaRollback";
            public const string SoftSwissRollback = "SoftSwissRollback";
            public const string EzugiRollback = "EzugiRollback";
            public const string AWCRollback = "AWCRollback";
            public const string GrooveFailedBet = "Groove";
            public const string BetsByRound = "BetsByRound";
            public const string ClientsCommissionPlan = "ClientsCommissionPlan";
            public const string ClientProductCommissionTree = "ClientProductCommissionTree";
            public const string ClientFavoriteProducts = "ClientFavoriteProducts";
            public const string SecurityQuestions = "SecurityQuestions";
            public const string MessageTemplates = "MessageTemplates";
            public const string CommentTemplates = "CommentTemplates";
            public const string GameProviderSettings = "GameProviderSettings";
            public const string AdminMenu = "AdminMenu";
            public const string ListedKeys = "ListedKeys";
            public const string Characters = "Characters";
            public const string Tournaments = "Tournaments";
            public const string TournamentLeaderboards = "TournamentLeaderboards";
            public const string ProductSessionsCount = "ProductSessionsCount";
            public const string UserNotifications = "UserNotifications";
        }

        public static class Currencies
        {
            public const string USADollar = "USD";
            public const string Euro = "EUR";
            public const string ArmenianDram = "AMD";
            public const string KazakhianTenge = "KZT";
            public const string IranianTuman = "IRT";
            public const string IranianRial = "IRR";
            public const string IndonesianRupiah = "IDR";
            public const string IndonesianRupiahKilo = "kIDR";
            public const string KoreanWon = "KRW";
            public const string UzbekistanSom = "UZS";
            public const string MexicanPeso = "MXN";
            public const string PolandZloty = "PLN";
            public const string RussianRuble = "RUB";
            public const string ChileanPeso = "CLP";
            public const string ArgentinianPeso = "ARS";
            public const string ColumbianPeso = "COP";
            public const string PeruvianSol = "PEN";
            public const string BrazilianReal = "BRL";
            public const string Renminbi = "CNY";
            public const string TurkishLira = "TRY";
            public const string LaotianKip = "LAK";
            public const string MyanmarKyat = "MMK";
            public const string CambodianRiel = "KHR";
            public const string JapaneseYen = "JPY";
            public const string CanadianDollar = "CAD";
            public const string TunisianDinar = "TND";
            public const string EthiopianBirr = "ETB";
            public const string BritainPound = "GBP";
            public const string KenyanShilling = "KES";

            public const string USDT = "USDT";
            public const string USDC = "USDC";
            public const string PYUSD = "PYUSD";
            public const string BUSD = "BUSD";
            public const string USDTTRC20 = "USDTTRC20";
            public const string USDTERC20 = "USDTERC20";
        }

        public static List<string> CryptoCurrencies = new List<string>
        {
            "BTC", "ETH", "BNB", "DOT", "DAI", "LTC", "XRP"
        };

        public static class Permissions
        {
            public const string CreateUser = "CreateUser";
            public const string ViewUser = "ViewUser";
            public const string CreateApiKey = "CreateApiKey";
            public const string ViewUserReport = "ViewUserReport";
            public const string EditCommissionPlan = "EditCommissionPlan";
            public const string ViewClient = "ViewClient";
            public const string EditClient = "EditClient";
            public const string CreateClient = "CreateClient";
            public const string ViewClientContactInfoList = "ViewClientContactInfoList";
            public const string ViewClientContactInfo = "ViewClientContactInfo";
            public const string ViewAffiliateReferral = "ViewAffiliateReferral";
            public const string ViewClientByCategory = "ViewClientByCategory";
            public const string CreateBetShop = "CreateBetShop";
            public const string ChangeBetShopLimit = "ChangeBetShopLimit";
            public const string ViewBetShop = "ViewBetShop";
            public const string CreateCashDesk = "CreateCashDesk";

            public const string ViewCashDesk = "ViewCashDesk";
            public const string CreatePartner = "CreatePartner";
            public const string ViewPartner = "ViewPartner";
            public const string EditPartnerPasswordRegEx = "EditPartnerPasswordRegEx";
            public const string ViewPartnerMessage = "ViewPartnerMessage";
            public const string EditPartnerMessage = "EditPartnerMessage";
            public const string ViewReportByUserLog = "ViewReportByUserLog";
            public const string ViewReportByTransaction = "ViewReportByTransaction";
            public const string CreateBetShopGroup = "CreateBetShopGroup";
            public const string DeleteBetShopGroup = "DeleteBetShopGroup";
            public const string ViewBetShopGroup = "ViewBetShopGroup";
            public const string CreateClientMessage = "CreateClientMessage";
            public const string ViewPartnerMessageTemplate = "ViewPartnerMessageTemplate";
            public const string EditPartnerMessageTemplate = "EditPartnerMessageTemplate";
            public const string ViewPartnerCommentTemplate = "ViewPartnerCommentTemplate";
            public const string EditPartnerCommentTemplate = "EditPartnerCommentTemplate";


            public const string ViewClientMessage = "ViewClientMessage";
            public const string ViewPartnerSetting = "ViewPartnerSetting";

            public const string ViewPartnerKey = "ViewPartnerKey";
            public const string EditPartnerKey = "EditPartnerKey";
            public const string CreateProduct = "CreateProduct";
            public const string ViewProduct = "ViewProduct";
            public const string ViewProductCategory = "ViewProductCategory";
            public const string EditProductCategory = "EditProductCategory";
            public const string CreatePartnerProductSetting = "CreatePartnerProductSetting";
            public const string EditPartnerProductSetting = "EditPartnerProductSetting";
            public const string ViewPartnerProductSetting = "ViewPartnerProductSetting";
            public const string ViewPartnerPaymentLimits = "ViewPartnerPaymentLimits";
            public const string EditPartnerPaymentLimits = "EditPartnerPaymentLimits";
            public const string EditPartnerBank = "EditPartnerBank";
            public const string ViewGameProvider = "ViewGameProvider";
            public const string EditGameProvider = "EditGameProvider";
            public const string ViewAggregators = "ViewAggregators";
            public const string MakeBetFromBetShop = "MakeBetFromBetShop";
            public const string RollBackBetShopOperationsFromProduct = "RollBackBetShopOperationsFromProduct";// RollBack BetShop Operations From Product
            public const string PayWinFromBetShop = "PayWinFromBetShop";// Pay Win From BetShop
            public const string AllowPaymentRequest = "AllowPaymentRequest";// Allow PaymentRequest
            public const string CancelPaymentRequest = "CancelPaymentRequest";
            public const string CancelPaymentRequestFromConfirmed = "CancelPaymentRequestFromConfirmed";
            public const string CancelPaymentRequestFromPayPending = "CancelPaymentRequestFromPayPending";

            public const string RollBackOperationsFromProduct = "RollBackOperationsFromProduct";// RollBack Operations From Product
            public const string CreateDepositFromPaymentSystem = "CreateDepositFromPaymentSystem";// Create Transfer From PaymentSystem               
            public const string CancelTransferFromPaymentSystem = "CancelTransferFromPaymentSystem";// Cancel Transfer From PaymentSystem
            public const string CreateDepositFromBetShop = "CreateDepositFromBetShop";// Transfer money from BetShop to client                        
            public const string PayDepositFromBetShop = "PayDepositFromBetShop";// Pay Transfer From BetShop                                           
            public const string PayWithdrawFromBetShop = "PayWithdrawFromBetShop";// Pay PaymentRequest
            public const string PayPaymentRequest = "PayPaymentRequest";// Pay PaymentRequest
            public const string PayBetShopDebt = "PayBetShopDebt";// Pay Transfer From BetShop
            public const string AddMoneyToPartnerBank = "AddMoneyToPartnerBank";// Add Money To Partner Bank
            public const string DeleteDepositFromBetShop = "DeleteDepositFromBetShop";// Delete deposit from BetShop

            public const string ViewBetShopBets = "ViewBetShopBets";
            public const string ViewBetShopBetsDashboard = "ViewBetShopBetsDashboard";
            public const string CreateBonus = "CreateBonus";
            public const string ViewJob = "ViewJob";
            public const string ChangeUserPass = "ChangeUserPassword";
            public const string ViewCashDeskTransactions = "ViewCashDeskTransactions";
            public const string CreateCorrectionForCashDesk = "CreateCorrectionForCashDesk";// Create Correction For CashDesk
            public const string ViewInternetBets = "ViewInternetBets";
            public const string CreateRole = "CreateRole";
            public const string ViewRole = "ViewRole";

            public const string CreateDebitCorrectionOnCashDesk = "CreateDebitCorrectionOnCashDesk";
            public const string CreateCreditCorrectionOnCashDesk = "CreateCreditCorrectionOnCashDesk";
            public const string CreateDebitCorrectionOnClient = "CreateDebitCorrectionOnClient";
            public const string CreateCreditCorrectionOnClient = "CreateCreditCorrectionOnClient";
            public const string ViewUserRole = "ViewUserRole";
            public const string CreateUserRole = "CreateUserRole";
            public const string ViewDepositToInternetClientsReport = "ViewDepositToInternetClientsReport";
            public const string ViewBetShopsReport = "ViewBetShopsReport";
            public const string ViewBetShopReconing = "ViewBetShopReconing";
            public const string ViewPartnerPaymentSetting = "ViewPartnerPaymentSetting";
            public const string ViewPartnerCountrySetting = "ViewPartnerCountrySetting";
            public const string EditPartnerCountrySetting = "EditPartnerCountrySetting";

            public const string CreatePartnerPaymentSetting = "CreatePartnerPaymentSetting";
            public const string DeletePartnerPaymentSetting = "DeletePartnerPaymentSetting";
            public const string ViewSegment = "ViewSegment";
            public const string EditSegment = "EditSegment";
            public const string DeleteSegment = "DeleteSegment";
            public const string ReconsiderClientSegments = "ReconsiderClientSegments";
            public const string ViewRegion = "ViewRegion";
            public const string CreateRegion = "CreateRegion";
            public const string ViewJobArea = "ViewJobArea";
            public const string EditJobArea = "EditJobArea";
            public const string ViewEnumerations = "ViewEnumerations";
            public const string CreateCurrency = "CreateCurrency";
            public const string CreateNote = "CreateNote";
            public const string DeleteNote = "EditNote";
            public const string ViewNote = "ViewNote";
            public const string CreatePartnerCurrencySetting = "CreatePartnerCurrencySetting";
            public const string CreatePartnerLanguageSetting = "CreatePartnerLanguageSetting";

            public const string ExportInternetBet = "ExportInternetBet";
            public const string ExportProduct = "ExportProduct";
            public const string ExportBetShops = "ExportBetShops";
            public const string ExportProviders = "ExportProviders";
            public const string ExportBetShopReconings = "ExportBetShopReconings";
            public const string ExportInternetBetsByClient = "ExportInternetBetsByClient";
            public const string ExortReportByClientsDetails = "ExortReportByClientsDetails";
            public const string ExportBetShopBets = "ExportBetShopBets";
            public const string ExportByBetShopPayments = "ExportByBetShopPayments";
            public const string ExportByUserLogs = "ExportByUserLogs";
            public const string ExportClientCorrections = "ExportClientCorrections";
            public const string ExportClientAccounts = "ExportClientAccounts";
            public const string ExportPlayersDashboard = "ExportPlayersDashboard";
            public const string ExportClients = "ExportClients";
            public const string ExportClientMessages = "ExportClientMessages";
            public const string ExportPartners = "ExportPartners";
            public const string ExportPartnersModel = "ExportPartnersModel";
            public const string ExportUsersModel = "ExportUsersModel";
            public const string ExportAdminShift = "ExportAdminShift";
            public const string ExportDeposit = "ExportDeposit";
            public const string ExportWithdrawal = "ExportWithdrawal";
            public const string ExportClientSessions = "ExportClientSessions";
            public const string ExportClientIdentities = "ExportClientIdentities";
            public const string ExportPartnerProductSetting = "ExportPartnerProductSetting";

            public const string ViewTotalsOnlineClients = "ViewTotalsOnlineClients";
            public const string ViewDepositsTotals = "ViewDepositsTotals";
            public const string ViewWithdrawalsTotals = "ViewWithdrawalsTotals";
            public const string ViewResetPass = "ViewResetPassword";
            public const string ViewDashboard = "ViewDashboard";
            public const string ViewPaymentRequests = "ViewPaymentRequests";
            public const string ViewPaymentSystems = "ViewPaymentSystems";
            public const string EditPaymentSystem = "EditPaymentSystem";
            public const string ViewRealTime = "ViewRealTime";
            public const string ViewPlayerDashboard = "ViewPlayerDashboard";
            public const string ViewBonuses = "ViewBonuses";
            public const string EditBonuses = "EditBonuses";
            public const string ViewComplimentaryRates = "ViewComplimentaryRates";
            public const string EditComplimentaryRates = "EditComplimentaryRates";
            public const string EditJackpot = "EditJackpot";
            public const string ViewJackpot = "ViewJackpot";
            public const string ViewReportByClientsDetails = "ViewReportByClientsDetails";
            public const string ViewCurrency = "ViewCurrency";
            public const string ViewTranslation = "ViewTranslation";
            public const string ViewReportByProvider = "ViewReportByProvider";
            public const string ViewReportByProduct = "ViewReportByProduct";
            public const string ViewReportByPartner = "ViewReportByPartner";
            public const string ViewAdminShift = "ViewAdminShift";
            public const string ViewLanguage = "ViewLanguage";
            public const string ViewClientIdentity = "ViewClientIdentity";
            public const string CreateClientIdentity = "CreateClientIdentity";
            public const string InProcessPaymentRequest = "InProcessPaymentRequest";
            public const string FrozenPaymentRequest = "FrozenPaymentRequest";
            public const string KYCPaymentRequest = "KYCPaymentRequest";
            public const string EditClientPass = "EditClientPassword";
            public const string EditClientPinCode = "EditClientPinCode";
            public const string ViewReportByCorrection = "ViewReportByCorrection";
            public const string ViewReportByObjectChangeHistory = "ViewReportByObjectChangeHistory";
            public const string ExportObjectChangeHistory = "ExportObjectChangeHistory";
            public const string ViewReportByLog = "ViewReportByLog";
            public const string ViewReportByJobLog = "ViewReportByJobLog";

            public const string ViewObjectTypes = "ViewObjectTypes";
            public const string SaveBanner = "SaveBanner";
            public const string ViewBanner = "ViewBanner";
            public const string ViewPromotion = "ViewPromotion";
            public const string ViewNews = "ViewNews";
            public const string ViewPopup = "ViewPopup";
            public const string ViewPopupStatistics = "ViewPopupStatistics";
            public const string EditPromotions = "EditPromotions";
            public const string RemovePromotions = "RemovePromotions";
            public const string RemoveNews = "RemoveNews";
            public const string ViewWebSiteMenu = "ViewWebSiteMenu";
            public const string ViewInterfaceTranslations = "ViewInterfaceTranslations";
            public const string ViewDocumentation = "ViewDocumentation";
            public const string EditWebSiteMenu = "EditWebSiteMenu";
            public const string EditWebSiteMenuTranslationEntry = "EditWebSiteMenuTranslationEntry";
            public const string EditInterfaceTranslation = "EditInterfaceTranslation";

            public const string CreateDebitCorrectionOnUser = "CreateDebitCorrectionOnUser";
            public const string CreateCreditCorrectionOnUser = "CreateCreditCorrectionOnUser";
            public const string CreateDebitCorrectionOnAffiliate = "CreateDebitCorrectionOnAffiliate";
            public const string CreateCreditCorrectionOnAffiliate = "CreateCreditCorrectionOnAffiliate";

            public const string EditCloudflare = "EditCloudflare";
            public const string EditCRMSetting = "EditCRMSetting";
            public const string ViewCRMSetting = "ViewCRMSetting";
            public const string EditAnnouncement = "EditAnnouncement";
            public const string ViewAnnouncement = "ViewAnnouncement";

            public const string EditTranslationEntry = "EditTranslationEntry";
            public const string EditStyles = "EditStyles";
            public const string EditConfig = "EditConfig";
            public const string EditNews = "EditNews";
            public const string EditPopup = "EditPopup";
            public const string EditPartnerAccounts = "EditPartnerAccounts";
            public const string ExportReportByPartners = "ExportReportByPartners";
            public const string ViewAffiliatePlatforms = "ViewAffiliatePlatforms";
            public const string ViewNotificationServices = "ViewNotificationServices";
            public const string ViewAffiliate = "ViewAffiliate";
            public const string ViewAffiliates = "ViewAffiliates";
            public const string UpdateAffiliate = "UpdateAffiliate";
            public const string GetAffiliateReport = "GetAffiliateReport";

            public const string ViewPaymentStatuses = "ViewPaymentStatuses";
            public const string EditSelfExcusion = "EditSelfExcusion";
            public const string ViewClientLimit = "ViewClientLimit";
            public const string EditClientLimit = "EditClientLimit";
            public const string EditClientSelfLimit = "EditClientSelfLimit";

            public const string SendEmailToPlayer = "SendEmailToPlayer";
            public const string SendEmailToPlayers = "SendEmailToPlayers";

            public const string ViewPaymentSetting = "ViewPaymentSetting";
            public const string EditPaymentSetting = "EditPaymentSetting";
            public const string AddPaymentSetting = "AddPaymentSetting";
            public const string RemovePaymentSetting = "RemovePaymentSetting";

            public const string ViewPaymentInfo = "ViewPaymentInfo";
            public const string EditPaymentInfo = "EditPaymentInfo";

            public const string ViewSetting = "ViewSetting";
            public const string EditSetting = "EditSetting";
            public const string ViewTodaysClients = "ViewTodaysClients";
            public const string ViewCharacter = "ViewCharacter";
            public const string EditCharacter = "EditCharacter";
            public const string DeleteCharacter = "DeleteCharacter";
            public const string ViewUserSetting = "ViewUserSetting";
            public const string EditUserSetting = "EditUserSetting";

            public const string ViewReportByAgents = "ViewReportByAgents";
            public const string ViewProductCommissionPercent = "ViewProductCommissionPercent";
        }

        public static class Jobs
        {
            public const int CloseAccountPeriod = 1;
            public const int AddMoneyToPartnerAccount = 2;
            public const int CalculateCompPoints = 3;
            public const int CalculateJackpots = 4;
            public const int ExpireUserSessions = 5;
            public const int BroadcastBets = 6;
            public const int ResetBetShopDailyTicketNumber = 7;
            public const int ExpireClientSessions = 9;
            public const int ExpireClientVerificationKeys = 10;

            public const int CalculateCashBackBonuses = 11;
            public const int CloseClientPeriod = 12;
            public const int GiveAffiliateBonus = 13;
            public const int DeletePaymentExpiredActiveRequests = 14;
            public const int UpdateCurrenciesRate = 15;
            public const int SendActiveMails = 16;
            public const int UpdateClientWageringBonus = 17;
            public const int FinalizeWageringBonus = 18;
            public const int SendActiveMerchantRequests = 19;
            public const int ApproveIqWalletConfirmedRequests = 20;

            public const int SendAffiliateReport = 21;
            public const int CalculateAgentsGGRProfit = 22;
            public const int CalculateAgentsTurnoverProfit = 23;
            public const int TriggerCRM = 24;
            public const int CheckClientBlockedSessions = 25;
            public const int CheckWithdrawRequestsStatuses = 26;
            public const int DeactivateExiredKYC = 27;
            public const int TriggerBonus = 28;
            public const int CheckUserBlockedSessions = 29;
            public const int CheckInactiveClients = 30;

            public const int NotifyIdentityExpiration = 31;
            public const int InactivateImpossibleBonuses = 32;
            public const int UpdateJackpotFeed = 34;
            public const int ReconsiderDynamicSegments = 35;
            public const int CheckInactiveUsers = 36;
            public const int SendPartnerDailyReport = 37;
            public const int SendPartnerWeeklyReport = 38;
            public const int SendPartnerMonthlyReport = 39;
            public const int AwardCashBackBonuses = 40;

            public const int FairSegmentTriggers = 41;
            public const int GiveFreeSpin = 42;
            public const int GiveJackpotWin = 43;
            public const int GiveCompPoints = 44;
            public const int CheckDepositRequestsStatuses = 45;
            public const int CheckForceBlockedClients = 47;
            public const int GiveFixedFeeCommission = 48;
            public const int FulfillDepositAction = 49;
            public const int ApplyClientRestriction = 50;

            public const int SettleBets = 51;
            public const int RestrictUnverifiedClients = 52;
            public const int GiveAffiliateCommission = 53;
            public const int ExpireClientVerificationStatus = 54;
            public const int CheckDuplicateClients = 55;
            public const int CloseTournaments = 56;
            public const int NotifyAffiliator = 57;
        }

        public static class EnumerationTypes
        {
            public const string ObjectTypes = "ObjectTypes";
            public const string SessionStates = "SessionStates";
            public const string Gender = "Gender";
            public const string ClientStates = "ClientStates";
            public const string UserStates = "UserStates";
            public const string ClientInfoTypes = "ClientInfoTypes";
            public const string ClientInfoStates = "ClientInfoStates";
            public const string PartnerClientVerificationTypes = "PartnerClientVerificationTypes";
            public const string DocumentStates = "DocumentStates";
            public const string TransactionTypes = "TransactionTypes";
            public const string PaymentRequestStates = "PaymentRequestStates";
            public const string OperationTypes = "OperationTypes";
            public const string AccountTypeKinds = "AccountTypeKinds";
            public const string ChangeClientFieldActions = "ChangeClientFieldActions";
            public const string ClientMessageTypes = "ClientMessageTypes";
            public const string LimitTypes = "LimitTypes";
            public const string GameProviders = "GameProviders";
            public const string InternalOperationSource = "InternalOperationSource";
            public const string PartnerProductSettingStates = "PartnerProductSettingStates";
            public const string CashDeskStates = "CashDeskStates";
            public const string BetShopStates = "BetShopStates";
            public const string PartnerStates = "PartnerStates";
            public const string BetShopTypes = "BetShopTypes";
            public const string BetShopGroupStates = "BetShopGroupStates";
            public const string GameProviderType = "GameProviderType";
            public const string PaymentSystemDepositStates = "PaymentSystemDepositStates";
            public const string PaymentRequestSendedStates = "PaymentRequestSendedStates";
            public const string UserTypes = "UserTypes";
            public const string PartnerPaymentSettingStates = "PartnerPaymentSettingStates";
            public const string Jobs = "Jobs";
            public const string JobStates = "JobStates";
            public const string PaymentSystemTypes = "PaymentSystemTypes";
            public const string RegionTypes = "RegionTypes";
            public const string NoteStates = "NoteStates";
            public const string FilterOperations = "FilterOperations";
            public const string ProductStates = "ProductStates";
            public const string DeviceTypes = "DeviceTypes";
            public const string ClientDocumentTypes = "ClientDocumentTypes";
            public const string KYCDocumentTypes = "KYCDocumentTypes";
            public const string KYCDocumentStates = "KYCDocumentStates";
            public const string ClientLogActions = "ClientLogActions";
            public const string CreditDocumentTypes = "CreditDocumentTypes";
            public const string WebSiteMenuType = "WebSiteMenuType";
            public const string WebSiteMenuItemType = "WebSiteMenuItemType";
            public const string ClientPaymentInfoTypes = "ClientPaymentInfoTypes";
            public const string PaymentRequestTypes = "PaymentRequestTypes";
            public const string TicketTypes = "TicketTypes";
            public const string MessageTicketState = "MessageTicketState";
            public const string ProductGroupTypes = "ProductGroupTypes";
            public const string BannerTypes = "BannerTypes";
            public const string BonusStates = "BonusStates";
            public const string CRMSettingTypes = "CRMSettingTypes";
            public const string AgentLevels = "AgentLevels";
            public const string EnvironmentTypes = "EnvironmentTypes";
            public const string AnnouncementTypes = "AnnouncementTypes";
            public const string ActionGroups = "ActionGroups";
            public const string CasinoLayoutTypes = "CasinoLayoutTypes";
            public const string TriggerTypes = "TriggerTypes";
            public const string LogoutTypes = "LogoutTypes";
            public const string BonusTypes = "BonusTypes";
            public const string ExportReportByPartner = "ExportReportByPartner";
            public const string MessageTemplateStates = "MessageTemplateStates";
            public const string CommunicationTypes = "CommunicationTypes";
            public const string ClientPaymentInfoStates = "ClientPaymentInfoStates";
        }

        public static class PartnerKeys
        {
            public const string SmsSender = "SmsSender";
            public const string SmsGwUrl = "SmsGwUrl";
            public const string SmsGwLogin = "SmsGwLogin";
            public const string SmsGwPass = "SmsGwPassword";
            public const string SmsTimeBounds = "SmsTimeBounds";
            public const string SmtpServer = "SmtpServer";
            public const string SmtpPort = "SmtpPort";
            public const string NotificationMail = "NotificationMail";
            public const string NotificationMailPass = "NotificationMailPassword";
            public const string CaptchaUrl = "CaptchaUrl";
            public const string CaptchaSecretKey = "CaptchaSecretKey";
            public const string CaptchaEnabled = "CaptchaEnabled";
            public const string AdminCaptchaEnabled = "AdminCaptchaEnabled";
            public const string LastLoginInfo = "LastLoginInfo";
            public const string GamingComplianceInfo = "GamingComplianceInfo";
            public const string TermsConditionVersion = "TermsConditionVersion";
            public const string DocumentExpirationPeriod = "DocumentExpirationPeriod";
            public const string RegistrationKYCTypes = "RegistrationKYCTypes";
            public const string PartnerKYCTypes = "PartnerKYCTypes";
            public const string WhitelistedCountries = "WhitelistedCountries";
            public const string BlockedCountries = "BlockedCountries";
            public const string WhitelistedIps = "WhitelistedIps";
            public const string BlockedIps = "BlockedIps";
            public const string RegistrationLimitPerDay = "RegistrationLimitPerDay";
            public const string SelfExclusionPeriod = "SelfExclusionPeriod";
            public const string PaymentDetailsValidation = "PaymentDetailsValidation";
            public const string AllowedFaildLoginCount = "AllowedFaildLoginCount";
            public const string ExternalDocumentVerification = "ExternalDocumentVerification";
            public const string JCJVerification = "JCJVerification";
            public const string AMLVerification = "AMLVerification";
            public const string ResponsibleGamingLimits = "ResponsibleGamingLimits";
            public const string LimitFutureUpdateInDays = "LimitFutureUpdateInDays";
            public const string VerificationPlatform = "VerificationPlatform";
            public const string OASISProviderId = "OASISProviderId";
            public const string GreenIdVerification = "GreenIdVerification";
            public const string AllowDigitalUsername = "AllowDigitalUsername";
            public const string AllowCancelConfirmedWithdraw = "AllowCancelConfirmedWithdraw";
            public const string WinDisplayType = "WinDisplayType";
            public const string SendRegistrationSMS = "SendRegistrationSMS";
            public const string SendRegistrationEmail = "SendRegistrationEmail";
            public const string SendWaitingKYCDocumentEmail = "SendWaitingKYCDocumentEmail";
            public const string SendWaitingKYCDocumentSMS = "SendWaitingKYCDocumentSMS";
            public const string SendDepositEmailNotification = "SendDepositEmailNotification";
            public const string SendWithdrawEmailNotification = "SendWithdrawEmailNotification";
            public const string ClientOperationTypes = "ClientOperationTypes";
            public const string NotificationService = "NotificationService";
            public const string EmailNotificationService = "EmailNotificationService";
            public const string RecoveryBySmsLimit = "RecoveryBySmsLimit";
            public const string VerificationBySmsLimit = "VerificationBySmsLimit";
            public const string VerificationByEmailLimit = "VerificationByEmailLimit";
            public const string AccountDetailsVerificationBySmsLimit = "AccountDetailsVerificationBySmsLimit";
            public const string MaxPendingWithdrawalsCount = "MaxPendingWithdrawalsCount";
            public const string WithdrawMaxCountPerDayPerCustomer = "WithdrawMaxCountPerDayPerCustomer";
            public const string CashWithdrawMaxCountPerDayPerCustomer = "CashWithdrawMaxCountPerDayPerCustomer";
            public const string UserPasswordExpiryPeriod = "UserPasswordExpiryPeriod";
            public const string ClientPasswordExpiryPeriod = "ClientPasswordExpiryPeriod";
            public const string BlockForInactivity = "BlockForInactivity";
            public const string RestrictUnverifiedClients = "RestrictUnverifiedClients";
            public const string BlockUserForInactivity = "BlockUserForInactivity";
            public const string ForceBlockPeriodInMinutes = "ForceBlockPeriodInMinutes";
            public const string ProvidersByRating = "ProvidersByRating";
            public const string VerificationKeyNumberOnly = "VerificationKeyNumberOnly";
            public const string PartnerCountryId = "PartnerCountryId";
            public const string DistributionUrl = "DistributionUrl";
            public const string ExternalPlatform = "ExternalPlatform";
            public const string PaymentGateway = "PaymentGateway";
            public const string ProductGateway = "ProductGateway";
            public const string AccountDetailsMobileVerification = "AccountDetailsMobileVerification";
            public const string CasinoPageUrl = "CasinoPageUrl";
            public const string LiveCasinoPageUrl = "LiveCasinoPageUrl";
            public const string CashierPageUrl = "CashierPageUrl";
            public const string PaymentErrorPageUrl = "PaymentErrorPageUrl";
            public const string PaymentSuccessPageUrl = "PaymentSuccessPageUrl";
            public const string PaymentStatusPageUrl = "PaymentStatusPageUrl";
            public const string ExternalPlatformUrl = "ExternalPlatformUrl";
            public const string TicketingSystemApiUrl = "TicketingSystemApiUrl";
            public const string TicketingSystemOpenUrl = "TicketingSystemOpenUrl";
            public const string TicketingSystemApiToken = "TicketingSystemApiToken";

            public const string LastCompPointBetId = "LastCompPointBetId";
            public const string LastJackpotBetId = "LastJackpotBetId";
            public const string LastBroadcastedBet = "LastBroadcastedBet";
            public const string LastProcessedBetDocumentId = "LastProcessedBetDocumentId";
            public const string LastConsideredBetId = "LastConsideredBetId";
            public const string ControlSystemUrl = "ControlSystemUrl";
            public const string CashCenterApiUrl = "CashCenterApiUrl";
            public const string CashCenterAppApiUrl = "CashCenterAppApiUrl";
            public const string CashCenterClientId = "CashCenterClientId";
            public const string CashCenterClientSecret = "CashCenterClientSecret";
            public const string CashCenterCodeVerifier = "CashCenterCodeVerifier";
            public const string CashCenterAppClientId = "CashCenterAppClientId";
            public const string CashCenterAppClientSecret = "CashCenterAppClientSecret";
            public const string AffiliateUrl = "AffiliateUrl";
            public const string AffiliateName = "AffiliateName";
            public const string AffiliateSecure = "AffiliateSecure";
            public const string AffiliateUsername = "AffiliateUsername";
            public const string AffiliatePassword = "AffiliatePassword";
            public const string AffiliateBrandId = "BrandId";
            public const string AffiliateFtpUrl = "FtpUrl";
            public const string AffiliateFtpUsername = "FtpUsername";
            public const string AffiliateFtpPassword = "FtpPassword";
            public const string AffiliateApiKey = "ApiKey";
            public const string AffiliateSendPostback = "SendPostback";
            public const string ReportFtpUrl = "ReportFtpUrl";
            public const string ReportFtpUsername = "ReportFtpUsername";
            public const string ReportFtpPassword = "ReportFtpPassword";
            public const string PartnerCommissionType = "PartnerCommissionType";
            public const string IsUserNameGeneratable = "IsUserNameGeneratable";
            public const string RefreshTokenState = "RefreshTokenState";
            public const string ResetPasswordOnFirstLogin = "ResetPasswordOnFirstLogin";
            public const string AgentAccountLimits = "AgentAccountLimits";
            public const string SubAccountLimits = "SubAccountLimits";
            public const string BulkAccountsLimits = "BulkAccountsLimits";
            public const string RequireDocumentForWithdrawal = "RequireDocumentForWithdrawal";
            public const string RequireDocumentForDeposit = "RequireDocumentForDeposit";
            public const string ActiveBonusMaxCount = "ActiveBonusMaxCount";
            public const string UserPasswordRegex = "UserPasswordRegex";
            public const string AgentPasswordRegex = "AgentPasswordRegex";
            public const string DistributionAddress = "DistributionAddress";
            public const string PartnerEmails = "PartnerEmails";
            public const string AMLServiceType = "AMLServiceType";
            public const string CheckComplimentaryPoints = "CheckComplimentaryPoints";
            public const string WebflowAccessToken = "WebflowAccessToken";
            public const string FtpServer = "FtpServer";
            public const string FtpUserName = "FtpUserName";
            public const string FtpPassword = "FtpPassword";
            public const string ApiCredentials = "ApiCredentials";
            public const string ResourcesUrl = "ResourcesUrl";
            public const string IgnoreSessionDomain = "IgnoreSessionDomain";
            public const string BetCommissionPercent = "BetCommissionPercent";
            public const string CRMPlarforms = "CRMPlarforms";
            public const string StatementFTPServer = "StatementFTPServer";
            public const string StatementFTPUsername = "StatementFTPUsername";
            public const string StatementFTPPassword = "StatementFTPPassword";
            public const string StatementPath = "StatementPath";
            public const string AllowMultiCurrencyAccounts = "AllowMultiCurrencyAccounts";

            #region Services

            public const string GreenIDAccountId = "GreenIDAccountId";
            public const string GreenIDApiCode = "GreenIDApiCode";
            public const string GreenIDApiPassword = "GreenIDApiPassword";
            public const string GreenIDApiUrl = "GreenIDApiUrl";

            public const string MGTComplianceApiUrl = "MGTComplianceApiUrl";
            public const string MGTComplianceService = "MGTComplianceService";
            public const string MGTComplianceClient = "MGTComplianceClient";
            public const string MGTComplianceApiSecret = "MGTComplianceApiSecret";

            public const string CloudflareApiUrl = "CloudflareApiUrl";
            public const string CloudflareEmail = "CloudflareEmail";
            public const string CloudflareApiKey = "CloudflareApiKey";
            public const string CloudflareZoneId = "CloudflareZoneId";

            public const string DigitalCustomerAMLUrl = "DigitalCustomerAMLUrl";
            public const string DigitalCustomerJCJUrl = "DigitalCustomerJCJUrl";
            public const string InsicApiUrl = "InsicApiUrl";
            public const string InsicApiMode = "InsicApiMode";
            public const string InsicPartnerKey = "InsicPartnerKey";
            public const string InsicUsername = "InsicUsername";
            public const string InsicPassword = "InsicPassword";
            public const string OASISApiUrl = "OASISApiUrl";
            public const string OASISAuthToken = "OASISAuthToken";
            public const string OASISIdentity = "OASISIdentity";
            public const string OASISPassword = "OASISPassword";
            public const string ExternalApiUrl = "ExternalApiUrl";
            public const string KRAApiUrl = "KRAApiUrl";
            public const string KRAApiUsername = "KRAApiUsername";
            public const string KRAApiPassword = "KRAApiPassword";
            public const string KRATaxFee = "KRATaxFee";
            public const string KRAWithHoldingTaxFee = "KRAWithHoldingTaxFee";
            public const string KRADailyExciseTax = "KRADailyExciseTax";
            public const string KRADailyWithHoldingTax = "KRADailyWithHoldingTax";

            public const string UKSanctionsServiceUrl = "UKSanctionsServiceUrl";

            public const string UniSenderApi = "UniSenderApi";
            public const string UniSenderApiUrl = "UniSenderApiUrl";
            public const string UniSenderApiCampaignId = "UniSenderApiCampaignId";
            public const string MailChimpUrl = "MailChimpUrl";
            public const string MailChimpApiKey = "MailChimpApiKey";
            public const string CustomerApiKey = "CustomerApiKey";
            public const string CustomerSender = "CustomerSender";
            public const string CustomerApiUrl = "CustomerApiUrl";
            public const string SMSToApiKey = "SMSToApiKey";
            public const string SMSToApiUrl = "SMSToApiUrl";
            public const string SMSToSender = "SMSToSender";
            public const string MailgunUsername = "MailgunUsername";
            public const string MailgunPassword = "MailgunPassword";
            public const string MailgunDomain = "MailgunDomain";
            public const string MailgunUrl = "MailgunUrl";

            public const string XeId = "XeId";
            public const string XeKey = "XeKey";
            public const string XeApiUrl = "XeApiUrl";

            public const string TelegramBotToken = "TelegramBotToken";
            public const string GeezSmsUrl = "GeezSmsUrl";
            public const string GeezSmsToken = "GeezSmsToken";

            public const string CustomerIoApiKey = "CustomerIoApiKey";
            public const string CustomerIoUrl = "CustomerIoUrl";

            public const string SmarticoUrl = "SmarticoUrl";
            public const string SmarticoToken = "SmarticoToken";
            public const string SmarticoBrandId = "SmarticoBrandId";






            #endregion

            #region Products

            public const string EvolutionAuthToken = "EvolutionAuthToken";
            public const string PartnerWebSiteWebApiSecretKey = "PartnerWebSiteWebApiSecretKey";
            public const string PartnerMicroGamingLogin = "PartnerMicroGamingLogin";
            public const string PartnerMicroGamingPass = "PartnerMicroGamingPassword";
            public const string TwoWinPowerSalt = "TwoWinPowerSalt";
            public const string TomHornSecretKey = "TomHornSecretKey";
            public const string TomHorn3SecretKey = "TomHorn3SecretKey";
            public const string TomHornOperatorId = "TomHornOperatorId";
            public const string TomHorn3OperatorId = "TomHorn3OperatorId";
            public const string TomHornApiUrl = "TomHornApiUrl";
            public const string DriveMediaSecretKey = "DriveMediaSecretKey";
            public const string BetGamesSecretKey = "BetGamesSecretKey";
            public const string BetGamesPartnerCode = "BetGamesPartnerCode";
            public const string BetGamesPartnerServer = "BetGamesPartnerServer";
            public const string PayBoxSecretKey = "PayBoxSecretKey";
            public const string ZeusPlayDataSignatureKey = "ZeusPlayDataSignatureKey";
            public const string ZeusPlayCasinoUrl = "ZeusPlayCasinoUrl";
            public const string SingularSecretKey = "SingularSecretKey";
            public const string EkkoSpinSecretKey = "EkkoSpinSecretKey";
            public const string EvenBetUrl = "EvenBetUrl";
            public const string EvenBetCasinoId = "EvenBetCasinoId";
            public const string EvenBetSecureKey = "EvenBetSecureKey";
            public const string IqSoftKeyToEvenBet = "IqSoftKeyToEvenBet";
            public const string SportsbookUrl = "SportsbookUrl";

            public const string SolidGamingUserName = "SolidGamingUserName";
            public const string SolidGamingPwd = "SolidGamingPwd";
            public const string SolidGamingBrandCode = "SolidGamingBrandCode";

            public const string ISoftBetSecretKey = "ISoftBetSecretKey";

            public const string InBetSecretKey = "InBetSecretKey";
            public const string InBetCustomerId = "InBetCustomerId";

            public const string TwoWinPowerSecretKey = "TwoWinPowerSecretKey";
            public const string TwoWinPowerId = "TwoWinPowerId";

            public const string CModuleSecretKey = "CModuleSecretKey";
            public const string CModulePartnerId = "CModulePartnerId";

            public const string ESportOperatorId = "ESportOperatorId";
            public const string ESportSecureKey = "ESportSecureKey";

            public const string IgrosoftSalt = "IgrosoftSalt";
            public const string IgrosoftMerchantIId = "IgrosoftMerchantIId";

            public const string EndorphinaSalt = "EndorphinaSalt";
            public const string EndorphinaMerchantId = "EndorphinaMerchantId";

            public const string GanapatiOperatorId = "GanapatiOperatorId";
            public const string GanapatiSecretKey = "GanapatiSecretKey";

            public const string EvolutionHostName = "EvolutionHostName";
            public const string EvolutionCasinoKey = "EvolutionCasinoKey";
            public const string EvolutionApiToken = "EvolutionApiToken";
            public const string EvolutionGameHistoryApiToken = "EvolutionGameHistoryApiToken";
            public const string EvolutionBrandId = "EvolutionBrandId";

            public const string EzugiApiId = "EzugiApiId";
            public const string EzugiApiName = "EzugiApiName";
            public const string EzugiApiUrl = "EzugiApiUrl";
            public const string EzugiAccess = "EzugiAccess";
            public const string EzugiOperatorId = "EzugiOperatorId";
            public const string EzugiSecretKey = "EzugiSecretKey";

            public const string YSBPrefix = "YSBPrefix";
            public const string YSBVendor = "YSBVendor";
            public const string YSBBaseUrl = "YSBBaseUrl";
            public const string YSBSecretKey = "YSBSecretKey";

            public const string SkyWindMerchantId = "SkyWindMerchantId";
            public const string SkyWindMerchantPwd = "SkyWindMerchantPwd";
            public const string SkyWindSecretKey = "SkyWindSecretKey";
            public const string SkyWindUserName = "SkyWindUserName";
            public const string SkyWindPwd = "SkyWindPwd";

            public const string SkyCityApKey = "SkyCityApKey";
            public const string SkyCityId = "SkyCityId";
            public const string SkyCitySecureKey = "SkyCitySecureKey";
            public const string SkyCityGameLauncherUrl = "SkyCityGameLauncherUrl";

            public const string SunCityOperatorID = "SunCityOperatorID";
            public const string SunCitySecureKey = "SunCitySecureKey";
            public const string SunCityApiUrl = "SunCityApiUrl";

            public const string TVBetSecureKey = "TVBetSecureKey";
            public const string TVBetPartnerId = "TVBetPartnerId";
            public const string TVBetIframe = "TVBetIframe";

            public const string OutcomeBetSessionUrl = "OutcomeBetSessionUrl";

            public const string PragmaticPlaySecureKey = "PragmaticPlaySecureKey";
            public const string PragmaticPlaySecureLogin = "PragmaticPlaySecureLogin";
            public const string PragmaticPlayCasinoDomain = "PragmaticPlayCasinoDomain";
            public const string PragmaticPlayCasinoApiUrl = "PragmaticPlayCasinoApiUrl";

            public const string IqSoftBrandId = "IqSoftBrandId";

            public const string SoftGamingSecureKey = "SoftGamingSecureKey";
            public const string SoftGamingApiKey = "SoftGamingApiKey";
            public const string SoftGamingApiPwd = "SoftGamingApiPwd";

            public const string BlueOceanApiKey = "BlueOceanApiKey";
            public const string BlueOceanApiPwd = "BlueOceanApiPwd";
            public const string BlueOceanSalt = "BlueOceanSalt";

            public const string SmartSoftKey = "SmartSoftKey";
            public const string SmartSoftPortalName = "SmartSoftPortalName";

            public const string SoftSwissCasinoId = "SoftSwissCasinoId";
            public const string SoftSwissAuthToken = "SoftSwissAuthToken";
            public const string SoftSwissResourcesUrl = "SoftSwissResourcesUrl";

            public const string KironOperatorId = "KironOperatorId";
            public const string KironBetShopOperatorId = "KironBetShopOperatorId";

            public const string BetSoftBankId = "BetSoftBankId";
            public const string BetSoftPassKey = "BetSoftPassKey";

            public const string AWCSecureKey = "AWCSecureKey";
            public const string AWCAgentId = "AWCAgentId";

            public const string HabaneroBrandId = "HabaneroBrandId";
            public const string HabaneroApiKey = "HabaneroApiKey";

            public const string EvoplayApiKey = "EvoplayApiKey";
            public const string EvoplayProjectId = "EvoplayProjectId";

            public const string GMWCasinoKey = "GMWCasinoKey";

            public const string BetSolutionsMerchantId = "BetSolutionsMerchantId";
            public const string BetSolutionsSecureKey = "BetSolutionsSecureKey";
            public const string BetSolutionsApiUrl = "BetSolutionsApiUrl";

            public const string GrooveCasinoId = "GrooveCasinoId";
            public const string GrooveLicense = "GrooveLicense";
            public const string GrooveAPIUrl = "GrooveAPIUrl";
            public const string GrooveEmail = "GrooveEmail";
            public const string GroovePassword = "GroovePassword";

            public const string BetsyPartnerId = "BetsyPartnerId";
            public const string BetsyApiKey = "BetsyApiKey";

            public const string PropsBuilderApiKey = "PropsBuilderApiKey";
            public const string PropsBuilderCasinoId = "PropsBuilderCasinoId";

            public const string RacebookPrivateKey = "RacebookPrivateKey";
            public const string RacebookSiteId = "RacebookSiteId";

            public const string EveryMatrixOperatorId = "EveryMatrixOperatorId";
            public const string EveryMatrixLogin = "EveryMatrixLogin";
            public const string EveryMatrixPassword = "EveryMatrixPassword";
            public const string EveryMatrixGamesUrl = "EveryMatrixGamesUrl";
            public const string EveryMatrixSportLaunchUrl = "EveryMatrixSportLaunchUrl";
            public const string EveryMatrixAdapterUrl = "EveryMatrixAdapterUrl";
            public const string EveryMatrixBonusAPIUrl = "EveryMatrixBonusAPIUrl";
            public const string EveryMatrixBonusAppUrl = "EveryMatrixBonusAppUrl";
            public const string EveryMatrixVendorApiUrl = "EveryMatrixVendorApiUrl";
            public const string EveryMatrixAdapterApiKey = "EveryMatrixAdapterApiKey";
            public const string EveryMatrixBonusApiUsername = "EveryMatrixBonusApiUsername";
            public const string EveryMatrixBonusApiPassword = "EveryMatrixBonusApiPassword";
            public const string EveryMatrixFSBonusApiUsername = "EveryMatrixFSBonusApiUsername";
            public const string EveryMatrixFSBonusApiPassword = "EveryMatrixFSBonusApiPassword";

            public const string NucleusBankId = "NucleusBankId";
            public const string NucleusApiKey = "NucleusApiKey";
            public const string NucleusApiUrl = "NucleusApiUrl";

            public const string MancalaCasinoId = "MancalaCasinoId";
            public const string MancalaApiKey = "MancalaApiKey";

            public const string VisionarySiteId = "VisionarySiteId";
            public const string VisionarySecretKey = "VisionarySecretKey";

            public const string DragonApiUrl = "DragonApiUrl";
            public const string DragonApiKey = "DragonApiKey";

            public const string GoldenRaceApiKey = "GoldenRaceApiKey";
            public const string GoldenRacePublicKey = "GoldenRacePublicKey";
            public const string GoldenRaceSiteId = "GoldenRaceSiteId";
            public const string GoldenRaceHostName = "GoldenRaceHostName";
            public const string GoldenRaceApiUrl = "GoldenRaceApiUrl";

            public const string ITPGamingPartnerId = "ITPGamingPartnerId";
            public const string ITPGamingAPIKey = "ITPGamingAPIKey";

            public const string TurboGamesClientId = "TurboGamesClientId";
            public const string TurboGamesApiKey = "TurboGamesApiKey";

            public const string BASApiKey = "BASApiKey";
            public const string BASLotteryLaunchUrl = "BASLotteryLaunchUrl";
            public const string BASDGSLaunchUrl = "BASDGSLaunchUrl";
            public const string WinSystemsTicketUrl = "WinSystemsTicketUrl";

            public const string AleaPlayCasinoId = "AleaPlayCasinoId";
            public const string AleaPlaySecretKey = "AleaPlaySecretKey";
            public const string AleaPlayEnvironment = "AleaPlayEnvironment";
            public const string AleaPlayGamesUrl = "AleaPlayGamesUrl";
            public const string AleaPlayFSApiUrl = "AleaPlayFSApiUrl";

            public const string PlaynGoAccessToken = "PlaynGoAccessToken";
            public const string PlaynGoGMTIsStaging = "PlaynGoGMTIsStaging";
            public const string PlaynGoGMTApiUsername = "PlaynGoGMTApiUsername";
            public const string PlaynGoGMTApiPassword = "PlaynGoGMTApiPassword";
            public const string PlaynGoApiUrl = "PlaynGoApiUrl";
            public const string PlaynGoApiUsername = "PlaynGoApiUsername";
            public const string PlaynGoApiPassword = "PlaynGoApiPassword";
            public const string PlaynGoApiPId = "PlaynGoApiPId";

            public const string AleaPartnersName = "AleaPartnersName";
            public const string AleaPartnersInstance = "AleaPartnersInstance";

            public const string BetMakersApiKey = "BetMakersApiKey";

            public const string RelaxGamingPartnerId = "RelaxGamingPartnerId";
            public const string RelaxGamingPartnerName = "RelaxGamingPartnerName";
            public const string RelaxGamingApiUrl = "RelaxGamingApiUrl";
            public const string RelaxGamingApiUsername = "RelaxGamingApiUsername";
            public const string RelaxGamingApiPassword = "RelaxGamingApiPassword";

            public const string PixmoveApiKey = "PixmoveApiKey";
            public const string PixmovePartnerId = "PixmovePartnerId";


            public const string IqSoftApiUrl = "IqSoftApiUrl";
            public const string IqSoftFreeSpinApiUrl = "IqSoftFreeSpinApiUrl";
            public const string IqSoftFreeSpinApiKey = "IqSoftFreeSpinApiKey";
            public const string IqSoftApiKey = "IqSoftApiKey";
            public const string IqSoftApiUserId = "IqSoftApiUserId";
            public const string IqSoftApiPartnerId = "IqSoftApiPartnerId";
            public const string IqSoftApiResourcesUrl = "IqSoftApiResourcesUrl";

            public const string MahjongSecretKey = "MahjongSecretKey";
            public const string MahjongPartnerId = "MahjongPartnerId";

            public const string LuckyGamingAESKey = "LuckyGamingAESKey";
            public const string LuckyGamingMD5Key = "LuckyGamingMD5Key";
            public const string LuckyGamingAgentID = "LuckyGamingAgentID";
            public const string LuckyGamingGamePlatformID = "LuckyGamingGamePlatformID";
            public const string LuckyGamingGameID = "LuckyGamingGameID";

            public const string EliteSkinCode = "EliteSkinCode";
            public const string EliteAuthorizationKey = "EliteAuthorizationKey";
            public const string EliteApiUrl = "EliteApiUrl";

            public const string BGGamesApiUrl = "BGGamesApiUrl";
            public const string BGGamesApiKey = "BGGamesApiKey";
            public const string BGGamesSignature = "BGGamesSignature";

            public const string TimelessTechOperatorID = "TimelessTechOperatorID";
            public const string TimelessTechSecretkey = "TimelessTechSecretkey";
            public const string TimelessTechUrl = "TimelessTechUrl";
            public const string TimelessTechLaunchUrl = "TimelessTechLaunchUrl";

            public const string FiverscoolAgentCode = "FiverscoolAgentCode";
            public const string FiverscoolAgentToken = "FiverscoolAgentToken";
            public const string FiverscoolAgentSecret = "FiverscoolAgentSecret";

            public const string RiseUpUrl = "RiseUpUrl";
            public const string RiseUpOperatorId = "RiseUpOperatorId";
            public const string RiseUpApiKey = "RiseUpApiKey";
            public const string RiseUp = "RiseUp";

            public const string LuckyStreakOperatorId = "LuckyStreakOperatorId";
            public const string LuckyStreakOperatorName = "LuckyStreakOperatorName";
            public const string LuckyStreakOperatorClientName = "LuckyStreakOperatorClientName";
            public const string LuckyStreakOperatorClientSecret = "LuckyStreakOperatorClientSecret";
            public const string LuckyStreakTokenUrl = "LuckyStreakTokenUrl";
            public const string LuckyStreakGetGamesUrl = "LuckyStreakGetGamesUrl";
            public const string LuckyStreakHmacKey = "LuckyStreakHmacKey";

            public const string JackpotGamingApiToken = "JackpotGamingApiToken";

            public const string SmarticoLabelKey = "SmarticoLabelKey";
            public const string SmarticoBrandKey = "SmarticoBrandKey";
            public const string SmarticoSaltKey = "SmarticoSaltKey";
            public const string SmarticoWidgetURL = "SmarticoWidgetURL";

            public const string ImperiumGamesHallId = "ImperiumGamesHallId";
            public const string ImperiumGamesHallKey = "ImperiumGamesHallKey";
            #endregion

            #region PaymentSystems

            public const string BankTransferUrl = "BankTransferUrl";
            public const string SkrillDepositUrl = "SkrillDepositUrl";
            public const string SkrillSecurKey = "SkrillSecurKey";
            public const string SkrillWithdrawUrl = "SkrillWithdrawUrl";
            public const string WooppayUrl = "WooppayUrl";
            public const string WooppayMerchantId = "WooppayMerchantId";
            public const string WooppayMerchantKeyword = "WooppayMerchantKeyword";
            public const string WooppayCardId = "WooppayCardId";
            public const string WooppayUserId = "WooppayUserId";
            public const string WalletOneDepositUrl = "WalletOneDepositUrl";
            public const string WalletOneWithdrawUrl = "WalletOneWithdrawUrl";
            public const string WalletOneSecretKey = "WalletOneSecretKey";
            public const string WalletOneWithdrawKey = "WalletOneWithdrawKey";
            public const string WalletOneToken = "WalletOneToken";
            public const string QiwiWalletDepositUrl = "QiwiWalletDepositUrl";
            public const string QiwiWalletActionUrl = "QiwiWalletActionUrl";
            public const string QiwiWalletProviderId = "QiwiWalletProviderId";
            public const string PayBoxDepositUrl = "PayBoxDepositUrl";
            public const string PayBoxWithdrawUrl = "PayBoxWithdrawUrl";
            public const string PayBoxAddWithdrawResultEndPoint = "PayBoxAddWithdrawResultEndPoint";
            public const string PayBoxPayWithdrawResultEndPoint = "PayBoxPayWithdrawResultEndPoint";
            public const string PayboxWithdrawId = "PayboxWithdrawId";
            public const string KazPostToken = "KazPostToken";
            public const string KazPostUrl = "KazPostUrl";
            public const string Help2PayApiUrl = "Help2PayApiUrl";
            public const string Help2PayWithdrawBankCode = "Help2PayWithdrawBankCode";
            public const string PaySecUrl = "PaySecUrl";
            public const string SDPayMobileUrl = "SDPayMobileUrl";
            public const string SDPayDesktopUrl = "SDPayDesktopUrl";
            public const string SDPayKey1 = "SDPayKey1";
            public const string SDPayKey2 = "SDPayKey2";
            public const string SDPayMD5Key = "SDPayMD5Key";
            public const string SDPayWithdrawUrl = "SDPayWithdrawUrl";
            public const string SDPayWithdrawKey1 = "SDPayWithdrawKey1";
            public const string SDPayWithdrawKey2 = "SDPayWithdrawKey2";
            public const string PayTrust88Url = "PayTrust88Url";
            public const string PayTrust88WithdrawBankCode = "PayTrust88WithdrawBankCode";
            public const string PiastrixDepositUrl = "PiastrixDepositUrl";
            public const string PiastrixWithdrawalUrl = "PiastrixWithdrawalUrl";
            public const string CardPayUrl = "CardPayUrl";
            public const string EasyPayUrl = "EasyPayUrl";
            public const string CardPaySecretKey = "CardPaySecretKey";
            public const string ApcoPayDepositUrl = "ApcoPayDepositUrl";
            public const string ApcoPaySecretWord = "ApcoPaySecretWord";
            public const string IqWalletUrl = "IqWalletUrl";
            public const string TotalProcessingUrl = "TotalProcessingUrl";
            public const string NetellerApiUrl = "NetellerApiUrl";
            public const string PayOneApiUrl = "PayOneApiUrl";
            public const string PayOneNotifyUrl = "PayOneNotifyUrl";
            public const string PerfectMoneyPayoutApiUrl = "PerfectMoneyPayoutApiUrl";
            public const string CardToCardApiUrl = "CardToCardApiUrl";
            public const string FreeKassaApiUrl = "FreeKassaApiUrl";
            public const string FreeKassaWithdrawApiUrl = "FreeKassaWithdrawApiUrl";
            public const string ZippyApiUrl = "ZippyApiUrl";
            public const string ZippyOneClickApiUrl = "ZippyOneClickApiUrl";
            public const string ZippyPayInApiUrl = "ZippyPayInApiUrl";
            public const string ZippyToken = "ZippyToken";
            public const string FreelancemeUrl = "FreelancemeUrl";
            public const string OmerApiUrl = "OmerApiUrl";
            public const string OmidApiUrl = "OmidApiUrl";
            public const string OmidNotifyUrl = "OmidNotifyUrl";
            public const string P2PApiUrl = "P2PApiUrl";
            public const string CartipalApiUrl = "CartipalApiUrl";
            public const string TronLinkUrl = "TronLinkUrl";
            public const string EZeeWalletUrl = "EZeeWalletUrl";
            // public const string InstaMFTUrl = "InstaMFTUrl";                     
            public const string InstapayUrl = "InstapayUrl";
            public const string InstaKKUrl = "InstaKKUrl";
            public const string InstaCepbankUrl = "InstaCepbankUrl";
            // public const string InstaVipHavaleUrl = "InstaVipHavaleUrl";
            public const string InstaExpressHavaleUrl = "InstaExpressHavaleUrl";
            public const string EcopayzApiUrl = "EcopayzApiUrl";
            public const string AstropayApiUrl = "AstropayApiUrl";
            public const string QaicashApiUrl = "QaicashApiUrl";
            public const string FinalPayApiUrl = "FinalPayApiUrl";
            public const string Pay4FunApiUrl = "Pay4FunApiUrl";
            public const string CapitalApiUrl = "CapitalApiUrl";
            public const string PraxisApiUrl = "PraxisApiUrl";
            public const string PraxisApplicationKey = "PraxisApplicationKey";
            public const string PaymentAsiaApiUrl = "PaymentAsiaApiUrl";
            public const string PaymentAsiaPayoutApiUrl = "PaymentAsiaPayoutApiUrl";
            public const string PaymentIQEnvironment = "PaymentIQEnvironment";
            public const string PaymentIQPayoutApiUrl = "PaymentIQPayoutApiUrl";
            public const string PaymentIQAuthToken = "PaymentIQAuthToken";
            public const string PaymentIQBoKey = "PaymentIQBoKey";
            public const string JazzCashierApiUrl = "JazzCashierApiUrl";
            public const string IPSApiUrl = "IPSApiUrl";
            public const string IPSApiId = "IPSApiId";
            public const string PayOpApiUrl = "PayOpApiUrl";
            public const string PayOpCheckoutUrl = "PayOpCheckoutUrl";
            public const string CashLibApiUrl = "CashLibApiUrl";
            public const string CryptoPayApiUrl = "CryptoPayApiUrl";
            public const string RunpayApiUrl = "RunpayApiUrl";
            public const string RunpayPayoutApiUrl = "RunpayPayoutApiUrl";
            public const string DPOPayApiUrl = "DPOPayApiUrl";
            public const string InteracApiUrl = "InteracApiUrl";
            public const string InteracSandbox = "InteracSandbox";
            public const string OptimumWayApiUrl = "OptimumWayApiUrl";
            public const string EwayApiUrl = "EwayApiUrl";
            public const string JetonApiUrl = "JetonApiUrl";
            public const string JetonCashApiUrl = "JetonCashApiUrl";
            public const string JetonCashoutApiUrl = "JetonCashoutApiUrl";
            public const string ImpayaApiUrl = "ImpayaApiUrl";
            public const string FlexepinApiUrl = "FlexepinApiUrl";
            public const string FlexPayApiUrl = "FlexPayApiUrl";
            public const string MifinityApiUrl = "MifinityApiUrl";
            public const string MifinityEnvironment = "MifinityEnvironment";
            public const string CorefyApiUrl = "CorefyApiUrl";
            public const string CorefyIsTest = "CorefyIsTest";
            public const string CryptonPayApiUrl = "CryptonPayApiUrl";
            public const string AsPayApiUrl = "AsPayApiUrl";
            public const string MaldoPayApiUrl = "MaldoPayApiUrl";
            public const string AfriPayApiUrl = "AfriPayApiUrl";
            public const string PremierCashierApiUrl = "PremierCashierApiUrl";
            public const string NodaPayApiUrl = "NodaPayApiUrl";
            public const string FugaPayApiUrl = "FugaPayApiUrl";
            public const string FugaPayPayoutApiUrl = "FugaPayPayoutApiUrl";
            public const string XcoinsPayApiUrl = "XcoinsPayApiUrl";
            public const string XcoinsPayApiPublicKey = "XcoinsPayApiPublicKey";
            public const string XcoinsPayApiPrivateKey = "XcoinsPayApiPrivateKey";
            public const string XcoinsPayRedirectUrl = "XcoinsPayRedirectUrl";
            public const string InternationalPSPApiUrl = "InternationalPSPApiUrl";
            public const string KralPayApiUrl = "KralPayApiUrl";
            public const string KralPayPayoutApiUrl = "KralPayPayoutApiUrl";
            public const string GatewayPayApiUrl = "GatewayPayApiUrl";
            public const string GatewayPayRedirectUrl = "GatewayPayRedirectUrl";
            public const string FreedomPayApiUrl = "FreedomPayApiUrl";
            public const string MpesaApiUrl = "MpesaApiUrl";
            public const string MpesaDirectApiUrl = "MpesaDirectApiUrl";
            public const string MpesaKey= "MpesaKey";
            public const string PayStageApiUrl = "PayStageApiUrl";
            public const string NixxeApiUrl = "NixxeApiUrl";
            public const string NixxeApiMode = "NixxeApiMode";
            public const string LiberSaveApiUrl = "LiberSaveApiUrl";
            public const string ExternalCashierApiUrl = "ExternalCashierApiUrl";
            public const string ExternalCashierRedirectUrl = "ExternalCashierRedirectUrl";
            public const string SpayzApiUrl = "SpayzApiUrl";
            public const string SpayzPublicKey = "SpayzPublicKey";
            public const string CashBullsApiUrl = "CashBullsApiUrl";

            public const string LuckyPayWithdrawUrl = "LuckyPayWithdrawUrl";
            public const string LuckyPayDepositUrl = "LuckyPayDepositUrl";


            public const string MoneyPayApiUrl = "MoneyPayApiUrl";
            public const string MoneyPayQueryUrl = "MoneyPayQueryUrl";
            public const string NOWPayApiUrl = "NOWPayApiUrl";

            public const string PixUrl = "PixUrl";
            public const string PixKey = "PixKey";
            public const string PixSecretKey = "PixSecretKey";
            public const string PixPrefix = "PixPrefix";

            public const string AzulpayUrl = "AzulpayUrl";
            public const string AzulpayApiKey = "AzulpayApiKey";

            public const string Transact365Url = "Transact365Url";

            public const string CoinsPaidTerminalUrl = "CoinsPaidTerminalUrl";
            public const string CoinsPaidUrl = "CoinsPaidUrl";
            public const string CoinsPaidId = "CoinsPaidId";
            public const string CoinsPaidConvertCurrency = "CoinsPaidConvertCurrency";

            public const string BRPayUrl = "BRPayUrl";
            public const string BRPayPaymentSystem = "BRPayPaymentSystem";
            public const string BRPayOutletId = "BRPayOutletId";

            public const string InterkassaCkeckoutId = "InterkassaCkeckoutId";
            public const string InterkassaUrl = "InterkassaUrl";
            public const string InterkassaVisaPaymentMethod = "InterkassaVisaPaymentMethod";
            public const string InterkassaMasterCardPaymentMethod = "InterkassaMasterCardPaymentMethod";
            public const string InterkassaAccountId = "InterkassaAccountId";
            public const string InterkassaPurseIds = "InterkassaPurseIds";

            public const string NgineUrl = "NgineUrl";
            public const string NgineApiUrl = "NgineApiUrl";

            public const string IXOPayApiUrl = "IXOPayApiUrl";

            public const string OktoPayUrl = "OktoPayUrl";

            public const string PayladoUrl = "PayladoUrl";

            public const string SoftLandIdentityUrl = "SoftLandIdentityUrl";
            public const string SoftLandIdentityId = "SoftLandIdentityId";
            public const string SoftLandSiteApiKey = "SoftLandSiteApiKey";
            public const string SoftLandClientSecret = "SoftLandClientSecret";

            public const string JetonHavaleUrl = "JetonHavaleUrl";

            public const string FinVertUrl = "FinVertUrl";

            public const string StripeUrl = "StripeUrl";

            public const string EliteUrl = "EliteUrl";
            public const string MaxPayApiUrl = "MaxPayApiUrl";

            public const string GumballPayUrl = "GumballPayUrl";

            public const string SantimaPayUrl = "SantimaPayUrl";

            public const string ChapaUrl = "ChapaUrl";

            public const string TelebirrUrl = "TelebirrUrl";
            public const string TelebirrPublicKey = "TelebirrPublicKey";

            public const string JmitSolutionsUrl = "JmitSolutionsUrl";

            public const string YaspaUrl = "YaspaUrl";
            public const string YaspaPublicKey = "YaspaPublicKey";
            public const string YaspaPrivateKey = "YaspaPrivateKey";

            public const string QuikiPayUrl = "QuikiPayUrl";

            public const string XprizoUrl = "XprizoUrl";

            public const string HuchAuthenticationUrl = "HuchAuthenticationUrl";
            public const string HuchUrl = "HuchUrl";

            public const string KatarunUrl = "KatarunUrl";
            public const string KatarunSecretKey = "KatarunSecretKey";
            public const string OmnoUrl = "OmnoUrl";
            public const string OmnohAuthenticationUrl = "OmnoAuthenticationUrl";

            public const string MPayUrl = "MPayUrl";

            public const string WzrdPayUrl = "WzrdPayUrl";
            public const string WzrdPayIsTest = "WzrdPayIsTest";

            public const string GetaPayUrl = "GetaPayUrl";
            

            //ForGoogle Autorize
            public const string GoogleClientSecret = "GoogleClientSecret";
            public const string GoogleClientId = "GoogleClientId";
            public const string FacebookAuthEndpoint = "FacebookAuthEndpoint";
            public const string FacebookTokenEndpoint = "FacebookTokenEndpoint";
            public const string GoogleAuthEndpoint = "GoogleAuthEndpoint";
            public const string GoogleTokenEndpoint = "GoogleTokenEndpoint";
            public const string SocialMediaredirectUrl = "SocialMediaredirectUrl";
            #endregion

            #region Platforms

            public const string TelegramToken = "TelegramToken";

            #endregion

            #region Config

            public const string RestrictEmailChanges = "RestrictEmailChanges";
            public const string RestrictMobileNumberChanges = "RestrictMobileNumberChanges";
            public const string RestrictNameChanges = "RestrictNameChanges";
            public const string RestrictBirthdayChanges = "RestrictBirthdayChanges";
            public const string VerificationCodeForWithdraw = "VerificationCodeForWithdraw";
            public const string ExternalAccountsPlatformTypes = "ExternalAccountsPlatformTypes";
            public const string ProhibitPasswordContainingPersonalData = "ProhibitPasswordContainingPersonalData";
            public const string ConnectingIPHeader = "ConnectingIPHeader";
            public const string IPCountryHeader = "IPCountryHeader";
            public const string FirstLastBirthUnique = "FirstLastBirthUnique";
            public const string CharactersEnabled = "CharactersEnabled";
            public const string HasCustomerIo = "HasCustomerIo";
            public const string VerificationValidPeriodInMonth = "VerificationValidPeriodInMonth";
            public const string PaymentSystemAccountTypes = "PaymentSystemAccountTypes";
            public const string DisallowMemberCorrection = "DisallowMemberCorrection";
            public const string HideClientContactInfo = "HideClientContactInfo";

            #endregion
        }

        public static class RequestMethods
        {
            public const string GetOnlineClients = "GetOnlineClients";
            public const string GetDeposits = "GetDeposits";
            public const string GetWithdrawals = "GetWithdrawals";
            public const string GetBetsInfo = "GetBetsInfo";
            public const string GetPlayersInfo = "GetPlayersInfo";
            public const string GetProviderBets = "GetProviderBets";
            public const string GetClientsInfoList = "GetClientsInfoList";
            public const string ExportClientsInfoList = "ExportClientsInfoList";
            public const string GetPaymentsInfo = "GetPaymentsInfo";
            public const string GetClientsInfo = "GetClientsInfo";
            public const string GetTopRegistrationCountries = "GetTopRegistrationCountries";
            public const string GetTopVisitorCountries = "GetTopVisitorCountries";
            public const string GetTopTurnoverClients = "GetTopTurnoverClients";
            public const string GetTopProfitableClients = "GetTopProfitableClients";
            public const string GetTopDamagingClients = "GetTopDamagingClients";
            public const string GetTopDepositMethods = "GetTopDepositMethods";
            public const string GetTopWithdrawMethods = "GetTopWithdrawMethods";
            public const string GetTopBonusReceivers = "GetTopBonusReceivers";
            public const string GetTopGrantedBonuses = "GetTopGrantedBonuses";
            public const string GetTopProfitableAgents = "GetTopProfitableAgents";
            public const string GetTopDamagingAgents = "GetTopDamagingAgents";
            public const string GetTopProfitablePartners = "GetTopProfitablePartners";
            public const string GetTopDamagingPartners = "GetTopDamagingPartners";
            public const string GetTopActiveClients = "GetTopActiveClients";
        }


        public static class ClientSettings
        {
            public const string UnusedAmountWithdrawPercent = "UnusedAmountWithdrawPercent";
            public const string CasinoLayout = "CasinoLayout";
            public const string IsAffiliateManager = "IsAffiliateManager";
            public const string AllowDoubleCommission = "AllowDoubleCommission";
            public const string AllowOutright = "AllowOutright";
            public const string AllowAutoPT = "AllowAutoPT";
            public const string ParentState = "ParentState";
            public const string MaxCredit = "MaxCredit";
            public const string SessionLimit = "SessionLimit";
            public const string SessionLimitDaily = "SessionLimitDaily";
            public const string SessionLimitWeekly = "SessionLimitWeekly";
            public const string SessionLimitMonthly = "SessionLimitMonthly";
            public const string SystemSessionLimit = "SystemSessionLimit";
            public const string PasswordChangedDate = "PasswordChangedDate";
            public const string TermsConditionsAcceptanceVersion = "TermsConditionsAcceptanceVersion";
            public const string SelfExcluded = "SelfExcluded";
            public const string SystemExcluded = "SystemExcluded";
            public const string AMLProhibited = "AMLProhibited";
            public const string AMLVerified = "AMLVerified";
            public const string AMLPercent = "AMLPercent";
            public const string JCJProhibited = "JCJProhibited";
            public const string DocumentVerified = "DocumentVerified";
            public const string CautionSuspension = "CautionSuspension";
            public const string BlockedForInactivity = "BlockedForInactivity";
            public const string BlockedForBonuses = "BlockedForBonuses";
            public const string PaymentAddress = "PaymentAddress";
            public const string ReferralType = "ReferralType";
            public const string UnderMonitoring = "UnderMonitoring";
            public const string Duplicated = "Duplicated";
            public const string PayoutLimit = "PayoutLimit";
            public const string ExternalStatus = "ExternalStatus";
            public const string VerificationServiceName = "VerificationServiceName";
            public const string AffiliateCommissionGranted = "AffiliateCommissionGranted";
            public const string LimitConfirmed = "LimitConfirmed";
            public const string PinCode = "PinCode";
            public const string NFTInfo = "NFTInfo";
        }
        public static class UserConfigurations
        {
            public const string CorrectonMaxAmount = "CorrectonMaxAmount";
            public const string DepositSubscription = "DepositSubscription";
            public const string WithdrawSubscription = "WithdrawSubscription";
            public const string RegistrationSubscription = "RegistrationSubscription";
            public const string BonusSubscription = "BonusSubscription";
            public const string KYCSubscription = "KYCSubscription";
        }

        public static class SegmentSettings
        {
            public const string SocialLink = "SocialLink";
            public const string AlternativeDomain = "AlternativeDomain";
            public const string ApiUrl = "ApiUrl";
            public const string ApiKey = "ApiKey";
            public const string DepositMinAmount = "DepositMinAmount";
            public const string IsDefault = "IsDefault";
            public const string Priority = "Priority";
            public const string DepositMaxAmount = "DepositMaxAmount";
            public const string WithdrawMinAmount = "WithdrawMinAmount";
            public const string WithdrawMaxAmount = "WithdrawMaxAmount";
            public const string DomainTextTranslationKey = "DomainTextTranslationKey";
        }

        public static class ProductDescriptions
        {
            public const string Sportsbook = "Sportsbook";
            public const string VirtualGames = "VirtualGames";
        };
        
        public static List<string> UserConfigurationTypes = new List<string>
        {
            UserConfigurations.CorrectonMaxAmount,
            UserConfigurations.DepositSubscription,
            UserConfigurations.WithdrawSubscription,
            UserConfigurations.RegistrationSubscription,
            UserConfigurations.BonusSubscription,
            UserConfigurations.KYCSubscription
        };

        public static List<int> ClaimingBonusTypes = new List<int>
        {
            (int)BonusTypes.CampaignCash,
            (int)BonusTypes.CampaignFreeBet,
            (int)BonusTypes.CampaignWagerCasino,
            (int)BonusTypes.CampaignWagerSport,
            (int)BonusTypes.CampaignFreeSpin,
            (int)BonusTypes.SpinWheel
        };

        public static List<int> PaymentRequestFinalStates { get; private set; } = new List<int>
        {
            (int)PaymentRequestStates.CanceledByClient,
            (int)PaymentRequestStates.CanceledByUser,
            (int)PaymentRequestStates.Approved,
            (int)PaymentRequestStates.ApprovedManually,
            (int)PaymentRequestStates.Failed,
            (int)PaymentRequestStates.Deleted,
            (int)PaymentRequestStates.Splitted
        };

        public static List<int> ClientBetTypes { get; private set; } = new List<int>
                    {
                        (int)BetDocumentStates.Uncalculated,
                        (int)BetDocumentStates.Won,
                        (int)BetDocumentStates.Lost,
                        (int)BetDocumentStates.Deleted,
                        (int)BetDocumentStates.Returned,
                        (int)BetDocumentStates.Cashouted
                    };

        public static List<int> ClientInfoEmailTypes { get; private set; } = new List<int>
        {
            (int)ClientInfoTypes.EmailVerificationKey,
            (int)ClientInfoTypes.ResetPasswordEmail,
            (int)ClientInfoTypes.QuickEmailRegistration,
            (int)ClientInfoTypes.ApproveWithdrawEmail,
            (int)ClientInfoTypes.RejectWithdrawEmail,
            (int)ClientInfoTypes.PasswordRecoveryEmailKey,
            (int)ClientInfoTypes.AffiliateClientInvitationEmail,
            (int)ClientInfoTypes.NewIpLoginEmail,
            (int)ClientInfoTypes.PasswordRecoveryEmailSubject,
            (int)ClientInfoTypes.EmailVerificationSubject,
            (int)ClientInfoTypes.EmailToPartner,
            (int)ClientInfoTypes.MissedDepositEmail,
            (int)ClientInfoTypes.BirthdayEmail,
            (int)ClientInfoTypes.QuickEmailRegistrationSubject,
            (int)ClientInfoTypes.IdentityCloseToExpire,
            (int)ClientInfoTypes.SelfExclusionFinished,
            (int)ClientInfoTypes.ClientLimit,
            (int)ClientInfoTypes.DepositEmail,
            (int)ClientInfoTypes.ClientInactivityEmail,
            (int)ClientInfoTypes.IdentityExpired,
            (int)ClientInfoTypes.PaymentInfoVerificationEmail,
            (int)ClientInfoTypes.SystemExclusionApplied,
            (int)ClientInfoTypes.SuccessRegistrationEmail,
            (int)ClientInfoTypes.WaitingKYCDocumentEmail,
            (int)ClientInfoTypes.PendingKYCVerificationEmail,
            (int)ClientInfoTypes.FailedDepositEmail,
            (int)ClientInfoTypes.ConfirmWithdrawEmail,
            (int)ClientInfoTypes.WaitingForKYCWithdrawEmail,
            (int)ClientInfoTypes.FrozenWithdrawEmail,
            (int)ClientInfoTypes.InProcessWithdrawEmail,
            (int)ClientInfoTypes.PendingWithdrawEmail,
            (int)ClientInfoTypes.AffiliateConfirmationEmail,
            (int)ClientInfoTypes.KYCVerificationEmail
        };

        public static List<int> ClientInfoSmsTypes { get; private set; } = new List<int>
        {
            (int)ClientInfoTypes.MobileVerificationKey,
            (int)ClientInfoTypes.ResetPasswordSMS,
            (int)ClientInfoTypes.QuickSmsRegistration,
            (int)ClientInfoTypes.AccountDetailsMobileKey,
            (int)ClientInfoTypes.PasswordRecoveryMobileKey,
            (int)ClientInfoTypes.AffiliateClientInvitationSMS,
            (int)ClientInfoTypes.ConfirmWithdrawSMS,
            (int)ClientInfoTypes.SuccessRegistrationSMS,
            (int)ClientInfoTypes.WaitingKYCDocumentSMS,
            (int)ClientInfoTypes.PendingKYCVerificationSMS
        };

        public static List<int> ClientInfoSecuredTypes { get; private set; } = new List<int>
        {
            //(int)ClientInfoTypes.ResetPasswordSMS,
            //(int)ClientInfoTypes.ResetPasswordEmail
        };

        public static Dictionary<int, string> PartnerKeyByClientInfoType { get; private set; } = new Dictionary<int, string>
        {
            { (int)ClientInfoTypes.AccountDetailsMobileKey, PartnerKeys.AccountDetailsVerificationBySmsLimit },
            { (int)ClientInfoTypes.MobileVerificationKey, PartnerKeys.VerificationBySmsLimit },
            { (int)ClientInfoTypes.EmailVerificationKey, PartnerKeys.VerificationByEmailLimit },
            { (int)ClientInfoTypes.PasswordRecoveryMobileKey, PartnerKeys.RecoveryBySmsLimit },
            { (int)ClientInfoTypes.PasswordRecoveryEmailKey, PartnerKeys.VerificationByEmailLimit },
            { (int)ClientInfoTypes.ResetPasswordSMS, PartnerKeys.RecoveryBySmsLimit },
            { (int)ClientInfoTypes.WithdrawVerificationEmail, PartnerKeys.VerificationByEmailLimit },
            { (int)ClientInfoTypes.WithdrawVerificationSMS, PartnerKeys.RecoveryBySmsLimit },
        };

        public static List<int> AutoclaimingTriggers = new List<int>
        {
            (int)TriggerTypes.SignIn,
            (int)TriggerTypes.SignUp,
            (int)TriggerTypes.PromotionalCode,
            (int)TriggerTypes.SignupCode,
            (int)TriggerTypes.NthDeposit,
            (int)TriggerTypes.AnyDeposit
        };

        public static List<int> BetTriggers = new List<int>
        {
            (int)TriggerTypes.BetPlacement,
            (int)TriggerTypes.BetSettlement,
            (int)TriggerTypes.BetPlacementAndSettlement,
            (int)TriggerTypes.CrossProductBetPlacement
        };

        public static List<string> FormPayments = new List<string>
        {
            PaymentSystems.BankTransferSwift,
            PaymentSystems.BankTransferSepa,
            PaymentSystems.BankWire,
            PaymentSystems.CepBank,
            PaymentSystems.ShebaTransfer
        };

        public static List<string> ReportingAffiliates = new List<string>
        {
            AffiliatePlatforms.MyAffiliates,
            AffiliatePlatforms.VipAffiliate,
            AffiliatePlatforms.Netrefer,
            AffiliatePlatforms.Intelitics,
            AffiliatePlatforms.DIM,
            AffiliatePlatforms.Affilka,
            AffiliatePlatforms.IncomeAccess,
            AffiliatePlatforms.Scaleo,
            AffiliatePlatforms.Trackier,
            AffiliatePlatforms.Scaleo, 
            AffiliatePlatforms.Trackier, 
            AffiliatePlatforms.Wynta
        };

        public enum ProiductCategories
        {
            LiveGames = 14,
            SkillGames = 15,
            TableGames = 17,
            Sportsbook = 18,
            Slots = 19,
            Lottery = 22,
            VirtualGames = 23,
            CrashGames = 58
        }

        public enum RegisterTypes
        {
            Full = 1,
            Mobile = 2,
            Email = 3,
            Agent = 4,
            Admin = 5,
            Betshop = 6
        }

        public enum SystemModuleTypes
        {
            ManagementSystem = 1,
            AgentSystem = 2,
            WebSite = 3,
            BetShop = 4,
            AffilliateSystem = 5,
        }
    }
}