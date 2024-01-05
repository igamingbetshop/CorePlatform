using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SerosPay
{
    public class FieldModel
    {
		[JsonProperty(PropertyName = "entry", Order = 1)]
		public PaymentModel Entry { get; set; }

		[JsonProperty(PropertyName = "field", Order = 2)]
		public string Field { get; set; }

		[JsonProperty(PropertyName = "original", Order = 3)]
        public string Original { get; set; }
    }
}
