using IqSoft.CP.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL
{
    public partial class fnAffiliate : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }
    }
}
