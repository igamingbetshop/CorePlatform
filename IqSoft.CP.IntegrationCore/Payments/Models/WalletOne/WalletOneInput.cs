namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOneInput
    {
        /// <summary>
        /// Позволяет показывать пользователю способы оплаты, 
        /// соответствующие его стране нахождения.
        /// 0 — отображаются способы без привязки к стране пользователя;
        /// 1 — страна пользователя и отображение способов определяется по IP.
        /// </summary>
        public int WMI_AUTO_LOCATION { get; set; }

        /// <summary>
        /// ru-RU — русский
        /// en-US — английский
        /// </summary>
        public string WMI_CULTURE_ID { get; set; }

        public string WMI_CURRENCY_ID { get; set; }

        public string WMI_CUSTOMER_FIRSTNAME { get; set; }

        public string WMI_CUSTOMER_LASTNAME { get; set; }

        public string WMI_DESCRIPTION { get; set; }

        /// <summary>
        /// ISO 8601
        /// </summary>
        public string WMI_EXPIRED_DATE { get; set; }

        public string WMI_FAIL_URL { get; set; }

        public string WMI_MERCHANT_ID { get; set; }

        public string WMI_PAYMENT_AMOUNT { get; set; }

        public long WMI_PAYMENT_NO { get; set; }

        /// <summary>
        /// Логин плательщика по умолчанию. 
        /// Значение данного параметра будет автоматически
        /// подставляться в поле логина при авторизации.
        /// </summary>
        public string WMI_RECIPIENT_LOGIN { get; set; }

        public string WMI_SIGNATURE { get; set; }

        public string WMI_SUCCESS_URL { get; set; }

    }
}
