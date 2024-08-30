using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.Integrations.Platforms.Telegram
{
    public class ClientInfo
    {
        public long id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string username { get; set; }
        public string photo_url { get; set; }
        public string auth_date { get; set; }
        public string hash { get; set; }
    }
}
