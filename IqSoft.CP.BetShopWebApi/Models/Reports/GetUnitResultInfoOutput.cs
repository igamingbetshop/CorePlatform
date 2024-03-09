using IqSoft.CP.BetShopWebApi.Models.Common;
namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetUnitResultInfoOutput : ApiResponseBase
    {
        public string State { get; set; }

        public string Outcome { get; set; }

        public object Selections { get; set; }
    }
}