namespace IqSoft.CP.PaymentGateway.Models.WalletOne
{
    public class WalletOneRequestStatus
    {
        public string WMI_MERCHANT_ID { get; set; }

        public string WMI_PAYMENT_AMOUNT { get; set; }

        public string WMI_COMMISSION_AMOUNT { get; set; }

        public string WMI_CURRENCY_ID { get; set; }

        public string WMI_TO_USER_ID { get; set; }

        public long WMI_PAYMENT_NO { get; set; }

        public long WMI_ORDER_ID { get; set; }

        public string WMI_DESCRIPTION { get; set; }

        public string WMI_CREATE_DATE { get; set; }

        public string WMI_UPDATE_DATE { get; set; }

        public string WMI_SUCCESS_URL { get; set; }

        public string WMI_FAIL_URL { get; set; }

        public string WMI_EXPIRED_DATE { get; set; }

        public string WMI_ORDER_STATE { get; set; }

        public string WMI_SIGNATURE { get; set; }

        public int WMI_AUTO_ACCEPT { get; set; }

        public int WMI_AUTO_LOCATION { get; set; }

        public string WMI_CUSTOMER_FIRSTNAME { get; set; }

        public string WMI_CUSTOMER_LASTNAME { get; set; }

        public string WMI_LAST_NOTIFY_DATE { get; set; }

        public int WMI_NOTIFY_COUNT { get; set; }

        public string WMI_PAYMENT_TYPE { get; set; }

        public string WMI_EXTERNAL_ACCOUNT_ID { get; set; }

        public string WMI_TEST_MODE_INVOICE { get; set; }

        public string WMI_INVOICE_OPERATIONS { get; set; }
    }
}