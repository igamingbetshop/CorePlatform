using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.ProductGateway.Models.MGTCompliance
{
    public class VerifyResult
    {
        [JsonProperty(PropertyName = "currency")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public Fastident fastident { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public Verifeye verifeye { get; set; }
    }
    public class Fastident
    {
        [JsonProperty(PropertyName = "currency")]
        public string status { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string documentResult { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public int faceMatchScore { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string documentType { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string documentNumber { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string documentCountry { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public bool documentIsVerified { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string documentIssuedBy { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string documentExpiration { get; set; }
        
        [JsonProperty(PropertyName = "currency")]
        public string firstName { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string lastName { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string birthdate { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string serviceName { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string service { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public DateTime time { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public Matching matching { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string[] files { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public Attatchments attatchments { get; set; }
    }

    public class Matching
    {
        [JsonProperty(PropertyName = "currency")]
        public string result { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public Verification verification { get; set; }
    }

    public class Verification
    {
        //[JsonProperty(PropertyName = "currency")]
        //public Firstname firstName { get; set; }

        //[JsonProperty(PropertyName = "currency")]
        //public Lastname lastName { get; set; }

        //[JsonProperty(PropertyName = "currency")]
        //public Birthdate birthdate { get; set; }
    }

    public class PersonalField
    {
        [JsonProperty(PropertyName = "currency")]
        public string verified { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string claimed { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string evaluation { get; set; }
    }

    public class Attatchments
    {
        [JsonProperty(PropertyName = "currency")]
        public string id_front { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string id_back { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string selfie { get; set; }
    }

    public class Verifeye
    {
        public string customerUuid { get; set; }
        public string clientUuid { get; set; }
        public string externalID { get; set; }
        public string isVerified { get; set; }
        public string stage { get; set; }
        public IP_Information IP_Information { get; set; }
        public Document_Information Document_Information { get; set; }
        public Face_Information Face_Information { get; set; }
        public Administration Administration { get; set; }
        public Additionalfields AdditionalFields { get; set; }
    }

    public class IP_Information
    {
        public string ip_address { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string continent { get; set; }
        public string continent_code { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public string postal_code { get; set; }
        public string timezone { get; set; }
        public object owner { get; set; }
        public float longitude { get; set; }
        public float latitude { get; set; }
        public string currency { get; set; }
        public string[] languages { get; set; }
    }

    public class Document_Information
    {
        public string name { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string maidenName { get; set; }
        public string salutation { get; set; }
        public string title { get; set; }
        public string dateOfExpiration { get; set; }
        public string cityOfBirth { get; set; }
        public string issuedBy { get; set; }
        public string dateOfIssue { get; set; }
        public string issuingAuthority { get; set; }
        public string dob { get; set; }
        public string documentId { get; set; }
        public string documentType { get; set; }
        public string documentCountry { get; set; }
        public string age { get; set; }
        public bool underage { get; set; }
        public string isDocumentVerified { get; set; }
    }

    public class Face_Information
    {
        public string faceMatchScore { get; set; }
        public string isFaceVerified { get; set; }
    }

    public class Administration
    {
        public DateTime time { get; set; }
        public string type { get; set; }
    }

    public class Additionalfields
    {
        public string cancelReason { get; set; }
    }

}