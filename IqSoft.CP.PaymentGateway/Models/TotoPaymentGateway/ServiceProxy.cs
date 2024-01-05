using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using IqSoft.NGGP.WebApplications.PaymentGateway.Helpers;

namespace TotoGaming.Casino.Api.Logic.PaymentGateway
{
    [DebuggerStepThrough()]
    [GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
    [DataContract(Name="PaymentResult", Namespace="http://schemas.datacontract.org/2004/07/TotoGaming.Casino.Api.Logic.PaymentGateway")]
    public class PaymentResult : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }

        [DataMember()]
        public TotoPaymentGatewayHelpers.ErrorCode Code { get; set; }

        [DataMember()]
        public PaymentRequestInfo RequestInfo { get; set; }

        [DataMember()]
        public string Description { get; set; }
    }
    
    [DebuggerStepThrough()]
    [GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
    [DataContract(Name="DepositInfo", Namespace="http://schemas.datacontract.org/2004/07/TotoGaming.Casino.Api.Logic.PaymentGateway")]
    public class PaymentRequestInfo : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }
        
        [DataMember()]
        public Amount Amount { get; set; }
        
        [DataMember()]
        public int Id { get; set; }

        [DataMember()]
        public int Status { get; set; }
        
        [DataMember()]
        public DateTime? Time { get; set; }

        [DataMember()]
        public string ExternalId { get; set; }
        
        [DataMember()]
        public string Info { get; set; }
    }
    
    [DebuggerStepThrough()]
    [GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
    [DataContract(Name="Amount", Namespace="http://schemas.datacontract.org/2004/07/TotoGaming.Casino.Api.Logic.PaymentGateway")]
    public class Amount : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }

        [DataMember()]
        public TotoPaymentGatewayHelpers.Currency Currency { get; set; }
        
        [DataMember()]
        public double Value { get; set; }
    }
    
    [DebuggerStepThrough()]
    [GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
    [DataContract(Name="ReceiptInfo", Namespace="http://schemas.datacontract.org/2004/07/TotoGaming.Casino.Api.Logic.PaymentGateway")]
    public class ReceiptInfo : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }
        
        [DataMember()]
        public KeyValuePair<string, string>[] AdditionalInfo { get; set; }
        
        [DataMember()]
        public Amount Amount { get; set; }
        
        [DataMember()]
        public string Id { get; set; }
        
        [DataMember()]
        public TotoPaymentGatewayHelpers.PaymentSystem System { get; set; }
        
        [DataMember()]
        public DateTime Time { get; set; }
    }
}
