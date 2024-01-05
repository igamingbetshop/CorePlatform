using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.Notification
{
    public class UniSenderInput
    {
        [JsonProperty(PropertyName = "format")]
        public string format { get; set; }

        [JsonProperty(PropertyName = "api_key")]
        public string api_key { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "sender_name")]
        public string sender_name { get; set; }
 
        [JsonProperty(PropertyName = "sender_email")]
        public string sender_email { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string subject { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string body { get; set; }

        public string list_id { get; set; }        
    }

    public class OneSenderInput
    {
        [JsonProperty(PropertyName = "api_key")]
        public string ApiKey { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "message")]
        public Message MessageBody { get; set; }
    }
    public class Message
    {
        [JsonProperty(PropertyName = "body")]
        public Body BodyObject { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "from_email")]
        public string SenderEmail { get; set; }

        [JsonProperty(PropertyName = "from_name")]
        public string SenderName { get; set; }

        [JsonProperty(PropertyName = "template_id")]
        public string TemplateId { get; set; }

        [JsonProperty(PropertyName = "recipients")]
        public Recipient[] Recipients { get; set; }
    }

    public class Recipient
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }

    public class Body
    {
        [JsonProperty(PropertyName = "html")]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "plaintext")]
        public string Plaintext { get; set; }
    }

}