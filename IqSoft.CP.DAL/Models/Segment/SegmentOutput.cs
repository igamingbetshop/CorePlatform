using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Models.Segment
{
    public class SegmentOutput
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public int Mode { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public SegmentSettingModel SegementSetting { get; set; }
        public bool? IsKYCVerified { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsMobileNumberVerified { get; set; }
        public int? Gender { get; set; }
        public bool? IsTermsConditionAccepted { get; set; } // ???

        public string ClientStatus { get; set; }
        public Condition ClientStatusObject
        {
            set
            {
                ClientStatusSet = !string.IsNullOrEmpty(ClientStatus) ? new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId = (int)FilterOperations.InSet, StringValue = ClientStatus } }
                } : null;
            }
        }
        public Condition ClientStatusSet { get; set; }
                
        public string ClientId { get; set; }
        public Condition ClientIdObject {            
            set
            {
                ClientIdSet =!string.IsNullOrEmpty(ClientId) ? new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId = (int)FilterOperations.InSet, StringValue = ClientId } }
                } : null;
            }        
        }
        public Condition ClientIdSet { get; set; }

        public string Email { get; set; }
        public Condition EmailObject
        {
            set
            {
                EmailSet = !string.IsNullOrEmpty(Email) ? new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId =  (int)FilterOperations.InSet, StringValue = Email } }
                } : null;
            }
        }
        public Condition EmailSet{ get; set; }
        
        public string FirstName { get; set; }
        public Condition FirstNameObject
        {
            set
            {
                FirstNameSet =!string.IsNullOrEmpty(FirstName) ? new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId =  (int)FilterOperations.InSet, StringValue = FirstName } }
                } : null;
            }
        }
        public Condition FirstNameSet{ get; set; }
        
        public string LastName { get; set; }
        public Condition LastNameObject
        {
            set
            {
                LastNameSet = !string.IsNullOrEmpty(LastName) ? new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId =  (int)FilterOperations.InSet, StringValue = LastName } }
                } : null;
            }
        }
        public Condition LastNameSet{ get; set; }
       
        public string Region { get; set; }
        public Condition RegionObject
        {
            set
            {
                RegionSet = !string.IsNullOrEmpty(Region) ? new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId =  (int)FilterOperations.InSet, StringValue = Region } }
                } : null;
            }
        }
        public Condition RegionSet { get; set; }
        
        public string MobileCode { get; set; }
        public Condition MobileCodeObject
        {
            set
            {
                MobileCodeSet = !string.IsNullOrEmpty(MobileCode) ?  new Condition
                {
                    ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId =  (int)FilterOperations.InSet, StringValue = MobileCode } }
                } : null;
            }
        }
        public Condition MobileCodeSet { get; set; }

        public string SessionPeriod { get; set; }
        public Condition SessionPeriodObject { get; set; }

        public string SignUpPeriod { get; set; }
        public Condition SignUpPeriodObject { get; set; }
        //public string Profit { get; set; }
        //public Condition ProfitObject { get; set; }
        //public string Bonus { get; set; }
        //public Condition BonusObject
        //{
        //    set
        //    {
        //        BonusSet = !string.IsNullOrEmpty(Bonus) ? new Condition
        //        {
        //            ConditionItems = new List<ConditionItem> { new ConditionItem { OperationTypeId =  (int)FilterOperations.InSet, StringValue = Bonus } }
        //        } : null;
        //    }
        //}
        //public Condition BonusSet { get; set; }
        public string SuccessDepositPaymentSystem { get; set; }
        public Condition SuccessDepositPaymentSystemObject
        {
            set
            {
                SuccessDepositPaymentSystemList = !string.IsNullOrEmpty(SuccessDepositPaymentSystem) ?
                                                   SuccessDepositPaymentSystem.Split(',').Select(Int32.Parse).ToList() : null;
            }
        }

        public List<int> SuccessDepositPaymentSystemList { get; set; }

        public string SuccessWithdrawalPaymentSystem { get; set; }
        public Condition SuccessWithdrawalPaymentSystemObject
        {
            set
            {
                SuccessWithdrawalPaymentSystemList = !string.IsNullOrEmpty(SuccessWithdrawalPaymentSystem) ?
                                                                 SuccessWithdrawalPaymentSystem.Split(',').Select(Int32.Parse).ToList() : null;
            }
        }
        public List<int> SuccessWithdrawalPaymentSystemList { get; set; }

        public string AffiliateId { get; set; }
        public Condition AffiliateIdObject { get; set; }
        public string AgentId { get; set; }
        public Condition AgentIdObject { get; set; }

        public string TotalBetsCount { get; set; }
        public Condition TotalBetsCountObject { get; set; }
        public string SportBetsCount { get; set; }
        public Condition SportBetsCountObject { get; set; }
        public string CasinoBetsCount { get; set; }
        public Condition CasinoBetsCountObject { get; set; }
        public string TotalBetsAmount { get; set; }
        public Condition TotalBetsAmountObject { get; set; }
        public string TotalDepositsCount { get; set; }
        public Condition TotalDepositsCountObject { get; set; }
        public string TotalDepositsAmount { get; set; }
        public Condition TotalDepositsAmountObject { get; set; }
        public string TotalWithdrawalsCount { get; set; }
        public Condition TotalWithdrawalsCountObject { get; set; }
        public string TotalWithdrawalsAmount { get; set; }
        public Condition TotalWithdrawalsAmountObject { get; set; }
        public string ComplimentaryPoint { get; set; }
        public Condition ComplimentaryPointObject { get; set; }
    }
}
