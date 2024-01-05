using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.UserModels
{
    public class LoginUserInput
    {
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public string LanguageId { get; set; }
        public int UserType { get; set; }
        public int? CashDeskId { get; set; }
        public string ReCaptcha { get; set; }
    }
}
