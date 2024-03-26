namespace IqSoft.CP.Common.Enums
{
    public enum AccountTypes
    {
        ClientUnusedBalance = 1,
        ClientUsedBalance = 2,
        ClientBooking = 3,
        BetShopBalance = 4,
        PartnerPaymentSettingBalance = 5,
        PartnerBalance = 6,
        ExternalClientsAccount = 8,
        PartnerBank = 9,
        ClientBetShopBalance = 10,
        ProductDebtToPartner = 11,
        ClientBonusBalance = 12,
        CashDeskBalance = 13,
        UserBalance = 14,
        AffiliateManagerBalance = 15,
        ClientCompBalance = 16, //CanBeDecreased
        ClientCoinBalance = 17, //AlwaysIncreases
        BonusWin = 18,
        TerminalBalance = 19
    }
}