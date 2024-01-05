using System;
using System.Runtime.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    [DataContract]
    public class BaseOutput
    {
        [DataMember(Name = "responseCode")]
        public int ResponseCode { get; set; }
    }

    [DataContract(Name = "genericResponseItem")]
    public class GenericOutput : BaseOutput
    {
        [DataMember(Name = "returnValue")]
        public string TransactionId { get; set; }

        [DataMember(Name = "returnValueLong")]
        public long ReturnValueLong { get; set; }       //Always NULL

        [DataMember(Name = "modificationDate")]
        public DateTime ModificationDate { get; set; }  //Always NULL
    }
}