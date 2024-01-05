using System.Xml.Serialization;


namespace IqSoft.CP.Integration.Payments.Models.Wooppay
{
    [XmlType("request")]
    public class WooppayStartTransactionInput
    {
        [XmlElement("merchantId")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Если поле оставлено пустым, CNP сгенерирует уникальный
        /// номер (reference) и вернет его в ответном сообщении
        /// </summary>
        [XmlElement("customerReference")]
        public long TransactionId { get; set; }

        [XmlElement("orderId")]
        public long OrderId { get; set; }

        /// <summary>
        /// Например, для суммы 1000.00 нужно передавать  100000. 
        /// </summary>
        [XmlElement("totalAmount")]
        public decimal Amount { get; set; }

        [XmlElement("currencyCode")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// ru – русский  /  kz – казахский  /  en - английский
        /// </summary>
        [XmlElement("languageCode")]
        public string Language { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [XmlElement("goodsList")]
        public string goodsList { get; set; }

        [XmlElement("returnURL")]
        public string ReturnURL { get; set; }

        /// <summary>
        /// dd.mm.yyyy hh24:mi:ss 
        /// </summary>
        [XmlElement("merchantLocalDateTime")]
        public string MerchantLocalDateTime { get; set; }
    }
}
