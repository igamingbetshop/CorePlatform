using IqSoft.CP.AdminWebApi.Models.CommonModels;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiOpenTicketInput : ApiRequestBase
    {
        public string Subject { get; set; }

        public string Message { get; set; }

        public int PartnerId { get; set; }

        public List<int> ClientIds { get; set; }
        public List<int> SegmentIds { get; set; }
    }
}