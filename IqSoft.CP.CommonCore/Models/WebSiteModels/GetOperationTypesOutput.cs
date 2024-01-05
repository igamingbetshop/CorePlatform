using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetOperationTypesOutput : ApiResponseBase
    {
        public List<OperationTypeModel> OperationTypes { get; set; }
    }

    public class OperationTypeModel
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public long NameId { get; set; }
        public string Name { get; set; }
    }
}