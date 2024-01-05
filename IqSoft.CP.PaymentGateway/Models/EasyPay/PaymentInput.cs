namespace IqSoft.CP.PaymentGateway.Models.EasyPay
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("payfrex-response", Namespace = "", IsNullable = false)]
    public partial class payfrexresponse
    {
        public string message { get; set; }
        public payfrexresponseOperations operations { get; set; }
        public object optionalTransactionParams { get; set; }
        public string status { get; set; }
        public payfrexresponseWorkFlowResponse workFlowResponse { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute("operation-size")]
        public byte operationsize { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperations
    {
        public payfrexresponseOperationsOperation operation { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperationsOperation
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
      //  public string details { get; set; }
        public uint merchantTransactionId { get; set; }
        public string message { get; set; }
        public string operationType { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("entry", IsNullable = false)]
        public payfrexresponseOperationsOperationEntry[] optionalTransactionParams { get; set; }
        public uint payFrexTransactionId { get; set; }
        public string paySolTransactionId { get; set; }

        /// <remarks/>
        public payfrexresponseOperationsOperationPaymentDetails paymentDetails { get; set; }
        public string paymentMethod { get; set; }
        public string paymentSolution { get; set; }
        public string status { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("sorted-order")]
        public byte sortedorder { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperationsOperationEntry
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperationsOperationPaymentDetails
    {
        public string cardHolderName { get; set; }
        public string cardNumber { get; set; }
        public string cardNumberToken { get; set; }
        public string cardType { get; set; }
        public ushort expDate { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("entry", IsNullable = false)]
        public payfrexresponseOperationsOperationPaymentDetailsEntry[] extraDetails { get; set; }
        public string issuerBank { get; set; }
        public string issuerCountry { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperationsOperationPaymentDetailsEntry
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseWorkFlowResponse
    {
        public ushort id { get; set; }
        public string name { get; set; }
        public byte version { get; set; }
    }
}