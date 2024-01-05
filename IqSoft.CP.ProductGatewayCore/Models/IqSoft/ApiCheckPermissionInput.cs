using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;

namespace IqSoft.CP.ProductGateway.Models.IqSoft
{
    public class ApiCheckPermissionInput : InputBase
    {
        public string Token { get; set; }

        public int UserId { get; set; }

        public string Permission { get; set; }

        public string Ip { get; set; }

        public string Country { get; set; }

        public string Source { get; set; }

        public string ActionName { get; set; }
    }
}