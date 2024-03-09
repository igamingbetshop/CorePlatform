using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Flexepin
{
    internal class SwapedVoucher : BaseOutput
    {

        [JsonProperty(PropertyName = "source_voucher_pin")]
        public string SourceVoucherpin { get; set; }

        [JsonProperty(PropertyName = "destination_voucher_pins")]
        public VoucherPin[] DestinationVoucherPins { get; set; }
    }


    public class VoucherPin
    {
        [JsonProperty(PropertyName = "pin")]
        public string Pin { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string Serialnumber { get; set; }
    }
}
