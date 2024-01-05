using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class GetBalanceOutput : BaseOutput
    {
        public decimal balance { get; set; }

        public string currency_code { get; set; }
    }
}