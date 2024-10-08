using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.Notification
{
    public class VerifyCodeOutput
    {
        public int ClientId { get; set; }
        public string EmailOrMobile { get; set; }
        public List<int> SecurityQuestions { get; set; }
    }
}
