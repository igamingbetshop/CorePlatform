namespace IqSoft.CP.Common.Models.Notification
{
    public class AffiliateNotificationInput
    {
        public string secure { get; set; }

        public int goal { get; set; }

        public int status { get; set; }

        public string clickid { get; set; }

        public decimal? sum { get; set; }

        public string action_id { get; set; }
    }
}
