using System;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Wooppay
{
    public class StartCashOutTransactionInput
    {
        [XmlElement("merchantId")]
        public string MerchantId { get; set; }

        [XmlAttribute("merchantKeyword")]
        public string MerchantKeyword { get; set; }

        /// <summary>
        /// UTF-8
        /// </summary>
        [XmlAttribute("terminalID")]
        public string TerminalID { get; set; }

        [XmlElement("customerReference")]
        public string TransactionId { get; set; }

        [XmlElement("tranAmount")]
        public decimal Amount { get; set; }

        [XmlElement("currencyCode")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// ru – русский  /  kz – казахский  /  en - английский
        /// </summary>
        [XmlElement("languageCode")]
        public string Language { get; set; }

        [XmlElement("description ")]
        public string Description { get; set; }

        [XmlElement("returnURL")]
        public string ReturnURL { get; set; }

        /// <summary>
        /// Формат dd.MM.yyyy HH:mm:ss (01.02.2012 12:34:58) 
        /// </summary>
        [XmlElement("merchantLocalDateTime")]
        public DateTime MerchantLocalDateTime { get; set; }

        [XmlElement("merchantAdditionalInformationList")]
        public AdditionalInformation AdditionalInformationList { get; set; }

    }

    public class AdditionalInformation
    {
        [XmlAttribute("USER_ID")]
        public string UserId { get; set; }

        [XmlAttribute("CARD_ID")]
        public string CardId { get; set; }

        [XmlAttribute("RECEIVER_USER_ID")]
        public string ReceiverUserId { get; set; }

        [XmlAttribute("RECEIVER_CARD_ID")]
        public string ReceiverCardId { get; set; }
    }
}
