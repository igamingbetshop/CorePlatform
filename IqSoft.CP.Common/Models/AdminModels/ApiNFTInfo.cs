using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiNFTInfo
    {
        public int ClientId { get; set; }
        public string ContractAddress { get; set; } 
        public string Id { get; set; }
        public int Family { get; set; }
        public DateTime StakedUntil { get; set; }
        public DateTime PledgedUntil { get; set; }
    }
}
