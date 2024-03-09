using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.Products
{
    public class ProductSettingInfo
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public decimal? Rating { get; set; }
        public int? OpenMode { get; set; }
        public int SubproviderId { get; set; }
        public List<int> CategoryIds { get; set; }
        public bool? HasDemo { get; set; }
        public bool ProductHasDemo { get; set; }
        public string ImageUrl { get; set; }
    }
}
