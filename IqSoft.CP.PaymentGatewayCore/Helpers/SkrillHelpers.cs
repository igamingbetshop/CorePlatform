namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class SkrillHelpers
    {
        public static class StatusCodes
        {
            public const int Confirmed = 2;
            public const int PayPanding = 0;
            public const int CanceledByClient = -1;
            public const int Failed = -2;

        }
    }
}