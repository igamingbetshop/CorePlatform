namespace IqSoft.CP.Integration.Payments.Models.EasyPay
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("payfrex-response", Namespace = "", IsNullable = false)]
    public partial class payfrexresponse
    {

        private string messageField;

        private payfrexresponseOperations operationsField;

        private object optionalTransactionParamsField;

        private string statusField;

        private payfrexresponseWorkFlowResponse workFlowResponseField;

        private byte operationsizeField;

        /// <remarks/>
        public string message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        public payfrexresponseOperations operations
        {
            get
            {
                return this.operationsField;
            }
            set
            {
                this.operationsField = value;
            }
        }

        /// <remarks/>
        public object optionalTransactionParams
        {
            get
            {
                return this.optionalTransactionParamsField;
            }
            set
            {
                this.optionalTransactionParamsField = value;
            }
        }

        /// <remarks/>
        public string status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public payfrexresponseWorkFlowResponse workFlowResponse
        {
            get
            {
                return this.workFlowResponseField;
            }
            set
            {
                this.workFlowResponseField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("operation-size")]
        public byte operationsize
        {
            get
            {
                return this.operationsizeField;
            }
            set
            {
                this.operationsizeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperations
    {

        private payfrexresponseOperationsOperation operationField;

        /// <remarks/>
        public payfrexresponseOperationsOperation operation
        {
            get
            {
                return this.operationField;
            }
            set
            {
                this.operationField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseOperationsOperation
    {

        private decimal amountField;

        private string currencyField;

        private uint merchantTransactionIdField;

        private string messageField;

        private string operationTypeField;

        private uint payFrexTransactionIdField;

        private string paymentSolutionField;

        private string statusField;

        private byte sortedorderField;

        /// <remarks/>
        public decimal amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        public string currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        public uint merchantTransactionId
        {
            get
            {
                return this.merchantTransactionIdField;
            }
            set
            {
                this.merchantTransactionIdField = value;
            }
        }

        /// <remarks/>
        public string message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        public string operationType
        {
            get
            {
                return this.operationTypeField;
            }
            set
            {
                this.operationTypeField = value;
            }
        }

        /// <remarks/>
        public uint payFrexTransactionId
        {
            get
            {
                return this.payFrexTransactionIdField;
            }
            set
            {
                this.payFrexTransactionIdField = value;
            }
        }

        /// <remarks/>
        public string paymentSolution
        {
            get
            {
                return this.paymentSolutionField;
            }
            set
            {
                this.paymentSolutionField = value;
            }
        }

        /// <remarks/>
        public string status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("sorted-order")]
        public byte sortedorder
        {
            get
            {
                return this.sortedorderField;
            }
            set
            {
                this.sortedorderField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class payfrexresponseWorkFlowResponse
    {

        private ushort idField;

        private string nameField;

        private byte versionField;

        /// <remarks/>
        public ushort id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public byte version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }
}