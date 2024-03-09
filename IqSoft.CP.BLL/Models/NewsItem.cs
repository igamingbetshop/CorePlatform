using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.BLL.Models
{
    public class NewsItem
    {
        public int id { get; set; }
        public string type { get; set; }
        public string image { get; set; }
        public string content { get; set; }
        public string description { get; set; }
        public string title { get; set; }
    }
}
