namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiResponseBase
    {
        public int ResponseCode { get; set; }

        public string Description { get; set; }

        public object ResponseObject { get; set; }
    }
}