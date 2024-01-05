using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Notification
{
    public class VerifyCodeOutput
    {
        public string EmailOrMobile { get; set; }
        public List<int> SecurityQuestions { get; set; }
    }
}
