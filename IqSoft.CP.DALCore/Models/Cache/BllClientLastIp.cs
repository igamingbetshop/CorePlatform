using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClientLastIp
    {
        public int ClientId {get;set;}
        public string Ip {get;set;}
    }
}
