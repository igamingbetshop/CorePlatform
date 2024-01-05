using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Betsoft
{
    public class FreeSpinInput
    {
        public int userId { get; set; }
        public int rounds { get; set; }
        public int extBonusId { get; set; }
        public List<string> games { get; set; }
        public string startTime { get; set; }
        public DateTime expirationTime { get; set; }
    }

    public class BSGSYSTEM
    {
        public BSGSYSTEMREQUEST REQUEST { get; set; }
        public string TIME { get; set; }
        public BSGSYSTEMRESPONSE RESPONSE { get; set; }
    }

    public class BSGSYSTEMREQUEST
    {
        public ushort USERID { get; set; }
        public ushort BANKID { get; set; }
        public string EXPIRATIONTIME { get; set; }
        public ushort GAMES { get; set; }
        public byte EXTBONUSID { get; set; }
        public string STARTTIME { get; set; }
        public byte ROUNDS { get; set; }
        public string HASH { get; set; }
    }

    public partial class BSGSYSTEMRESPONSE
    {
        public uint BONUSID { get; set; }
        public string RESULT { get; set; }
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
    }
}