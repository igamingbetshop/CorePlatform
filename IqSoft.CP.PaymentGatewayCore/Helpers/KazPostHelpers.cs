namespace IqSoft.CP.PaymentGateway.Helpers
{
    public class KazPostHelpers
    {
        public enum Statuses
        {
            Waiting = 1,
            Expired = 2,
            Canceled = 3,
            Paid=4,
            Failed=5,
            Confirmed =6
        }
    }
}