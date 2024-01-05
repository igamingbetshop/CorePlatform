namespace IqSoft.CP.Common.Enums
{
    public enum PaymentRequestStates
    {
        Pending = 1,
        CanceledByClient = 2,
        InProcess = 3,
        Frozen = 4,
        WaitingForKYC = 5,
        CanceledByUser = 6,
        Confirmed = 7,
        Approved = 8,
        Failed = 9,
        PayPanding = 10,
        Deleted = 11,
        ApprovedManually = 12,
        Expired = 13,
        CancelPending = 14,
        Splitted = 15
    }
}
