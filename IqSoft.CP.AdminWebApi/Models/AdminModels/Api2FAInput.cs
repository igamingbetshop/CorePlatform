using IqSoft.CP.AdminWebApi.Models.CommonModels;

namespace IqSoft.CP.AdminWebApi.Models.AdminModels
{
    public class Api2FAInput : RequestBase
    {
        public string Pin { get; set; }
    }
}