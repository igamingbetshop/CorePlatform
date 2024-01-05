using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Currency : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return 1; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Currency; }
        }

        public bool ShouldSerializeBetShops()
        {
            return false;
        }

        public bool ShouldSerializeObjectCurrencyPriorities()
        {
            return false;
        }

        public bool ShouldSerializeCurrencyRates()
        {
            return false;
        }

        public bool ShouldSerializePartnerPaymentSettings()
        {
            return false;
        }

        public bool ShouldSerializePartners()
        {
            return false;
        }

        public bool ShouldSerializeAccounts()
        {
            return false;
        }

        public bool ShouldSerializeUsers()
        {
            return false;
        }

        public bool ShouldSerializeUserSession()
        {
            return false;
        }

        public bool ShouldSerializePaymentRequests()
        {
            return false;
        }

        public bool ShouldSerializeBetShopReconings()
        {
            return false;
        }

        public bool ShouldSerializeClients()
        {
            return false;
        }
    }
}