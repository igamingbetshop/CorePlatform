using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models.Reports
{
    public class ApiGetShiftReportOutput : ApiResponseBase
    {
        public List<ApiShift> Shifts { get; set; }
    }
}
