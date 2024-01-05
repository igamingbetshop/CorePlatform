namespace IqSoft.CP.Common.Enums
{
    public enum VerificationCodeTypes
    {
        MobileNumberVerification = 1,
        EmailVerification = 2,
        PasswordRecoveryByEmail = 3,
        PasswordRecoveryByMobile = 4,
        WithdrawByEmail = 5,
        WithdrawByMobile = 6,
        AddBankAccountByEmail = 7,
        AddBankAccountByMobile = 8,
        PasswordChangeByEmail = 9,
        PasswordChangeByMobile = 10,
        SecurityQuestionChangeByEmail = 11,
        SecurityQuestionChangeByMobile = 12,
        USSDPinChangeByEmail = 13,
        USSDPinChangeByMobile = 14,
        PasswordRecoveryByEmailOrMobile = 15
    }
}
