using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnPartnerBankInfo
    {
        [NotMapped]
        public List<ClientPaymentInfo> ClientPaymentInfos { get; set; }
    }
}
