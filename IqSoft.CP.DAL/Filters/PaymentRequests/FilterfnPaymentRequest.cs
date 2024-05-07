using IqSoft.CP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.PaymentRequests
{
    public class FilterfnPaymentRequest : FilterBase<fnPaymentRequest>
    {
        public int? PartnerId { get; set; }
        public long FromDate { get; set; }
        public bool? WithPendings { get; set; }
        public int? PaymentSystemId { get; set; }
        public List<long> AccountIds { get; set; }
        public long ToDate { get; set; }
        public int? Type { get; set; }
        public bool? HasNote { get; set; }
        public int? AgentId { get; set; }
        public int? BetShopId { get; set; }
        public string CashCode { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation Names { get; set; }
        public FiltersOperation FirstNames { get; set; }
        public FiltersOperation LastNames { get; set; }

        public FiltersOperation CreatorNames { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation ClientEmails { get; set; }

        public FiltersOperation UserIds { get; set; }

        public FiltersOperation CashierIds { get; set; }

        public FiltersOperation CashDeskIds { get; set; }

        public FiltersOperation PartnerPaymentSettingIds { get; set; }

        public FiltersOperation PaymentSystemIds { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation States { get; set; }
        
        public FiltersOperation Types { get; set; }

        public FiltersOperation BetShopIds { get; set; }

        public FiltersOperation BetShopNames { get; set; }

        public FiltersOperation Amounts { get; set; }
        public FiltersOperation FinalAmounts { get; set; }

        public FiltersOperation CreationTimes { get; set; }

        public FiltersOperation LastUpdateTimes { get; set; }

        public FiltersOperation ExternalTransactionIds { get; set; }

		public FiltersOperation AffiliatePlatformIds { get; set; }

		public FiltersOperation AffiliateIds { get; set; }

        public FiltersOperation ActivatedBonusTypes { get; set; }
        
        public FiltersOperation CommissionAmounts { get; set; }
        
        public FiltersOperation CardNumbers { get; set; }
        
        public FiltersOperation CountryCodes { get; set; }
        
        public FiltersOperation SegmentIds { get; set; }
        
        public FiltersOperation SegmentNames { get; set; }


        protected override IQueryable<fnPaymentRequest> CreateQuery(IQueryable<fnPaymentRequest> objects,
            Func<IQueryable<fnPaymentRequest>, IOrderedQueryable<fnPaymentRequest>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (AgentId.HasValue)
                objects = objects.Where(x => x.UserPath.Contains("/" + AgentId.Value + "/"));
            if (PaymentSystemId.HasValue)
                objects = objects.Where(x => x.PaymentSystemId == PaymentSystemId.Value);
            if (BetShopId.HasValue)
                objects = objects.Where(x => x.BetShopId == BetShopId.Value);
            if (AccountIds != null && AccountIds.Any())
                objects = objects.Where(x => x.AccountId != null && AccountIds.Contains(x.AccountId.Value));

            if (FromDate > 0)
            {
                //if (WithPendings == null || !WithPendings.Value)
                //    objects = objects.Where(x => x.Date >= FromDate);
                //else
                    objects = objects.Where(x => x.Date >= FromDate /*|| !Constants.PaymentRequestFinalStates.Contains(x.Status)*/);
            }
            if (ToDate > 0)
                objects = objects.Where(x => x.Date < ToDate);
            if (Type.HasValue && Type == (int)PaymentRequestTypes.Deposit)
                objects = objects.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit);
            else if(Type.HasValue)
            objects = objects.Where(x => x.Type == Type.Value);
            if (!string.IsNullOrEmpty(CashCode))
                objects = objects.Where(x => x.CashCode == CashCode);

            if (HasNote.HasValue && HasNote.Value)
                objects = objects.Where(x => x.ClientHasNote == true);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, CashierIds, "CashierId");
            FilterByValue(ref objects, UserIds, "Users");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, ClientEmails, "Email");
            FilterByValue(ref objects, CashDeskIds, "CashDeskId");
            FilterByValue(ref objects, PartnerPaymentSettingIds, "PartnerPaymentSettingId");
            FilterByValue(ref objects, PaymentSystemIds, "PaymentSystemId");
            FilterByValue(ref objects, Currencies, "CurrencyId");

            FilterByValue(ref objects, States, "Status");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, BetShopIds, "BetShopId");
            FilterByValue(ref objects, BetShopNames, "BetShopName");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, FinalAmounts, "FinalAmount");
            FilterByValue(ref objects, CreationTimes, "CreationTime");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            FilterByValue(ref objects, ExternalTransactionIds, "ExternalTransactionId");
            FilterByValue(ref objects, Names, "FirstName", "LastName");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, CreatorNames, "UserFirstName", "UserLastName");
			FilterByValue(ref objects, AffiliatePlatformIds, "AffiliatePlatformId");
			FilterByValue(ref objects, AffiliateIds, "AffiliateId");
			FilterByValue(ref objects, ActivatedBonusTypes, "ActivatedBonusType");
			FilterByValue(ref objects, CommissionAmounts, "CommissionAmount");
			FilterByValue(ref objects, CardNumbers, "CardNumber");
			FilterByValue(ref objects, CountryCodes, "CountryCode");
			FilterByValue(ref objects, SegmentIds, "SegmentId");
			FilterByValue(ref objects, SegmentNames, "SegmentName");

			return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnPaymentRequest> FilterObjects(IQueryable<fnPaymentRequest> paymentRequests, Func<IQueryable<fnPaymentRequest>, IOrderedQueryable<fnPaymentRequest>> orderBy = null)
        {
            paymentRequests = CreateQuery(paymentRequests, orderBy);
            return paymentRequests;
        }

        public FilterfnPaymentRequest Copy()
        {
            return new FilterfnPaymentRequest
            {
                SkipCount = base.SkipCount,
                TakeCount = base.TakeCount,
                CheckPermissionResuts = base.CheckPermissionResuts.Select(x => x.Copy()).ToList(),
                PartnerId = PartnerId,
                FromDate = FromDate,
                ToDate = ToDate,
                WithPendings = WithPendings,
                Type = Type,
                HasNote = HasNote,
                AgentId = AgentId,
                CashCode = CashCode,
                Ids = Ids == null ? new FiltersOperation() : Ids.Copy(),
                UserNames = UserNames == null ? new FiltersOperation() : UserNames.Copy(),
                Names = Names == null ? new FiltersOperation() : Names.Copy(),
                CreatorNames = CreatorNames == null ? new FiltersOperation() : CreatorNames.Copy(),
                ClientIds = ClientIds == null ? new FiltersOperation() : ClientIds.Copy(),
                UserIds = UserIds == null ? new FiltersOperation() : UserIds.Copy(),
                CashierIds = CashierIds == null ? new FiltersOperation() : CashierIds.Copy(),
                CashDeskIds = CashDeskIds == null ? new FiltersOperation() : CashDeskIds.Copy(),
                PartnerPaymentSettingIds = PartnerPaymentSettingIds == null ? new FiltersOperation() : PartnerPaymentSettingIds.Copy(),
                PaymentSystemIds = PaymentSystemIds == null ? new FiltersOperation() : PaymentSystemIds.Copy(),
                Currencies = Currencies == null ? new FiltersOperation() : Currencies.Copy(),
                States = States == null ? new FiltersOperation() : States.Copy(),
                BetShopIds = BetShopIds == null ? new FiltersOperation() : BetShopIds.Copy(),
                BetShopNames = BetShopNames == null ? new FiltersOperation() : BetShopNames.Copy(),
                Amounts = Amounts == null ? new FiltersOperation() : Amounts.Copy(),
				AffiliatePlatformIds = AffiliatePlatformIds == null ? new FiltersOperation() : AffiliatePlatformIds.Copy(),
				AffiliateIds = AffiliateIds == null ? new FiltersOperation() : AffiliateIds.Copy(),
				CreationTimes = CreationTimes == null ? new FiltersOperation() : CreationTimes.Copy(),
                LastUpdateTimes = LastUpdateTimes == null ? new FiltersOperation() : LastUpdateTimes.Copy(),
                ExternalTransactionIds = ExternalTransactionIds == null ? new FiltersOperation() : ExternalTransactionIds.Copy(),
                ClientEmails = ClientEmails == null ? new FiltersOperation() : ClientEmails.Copy()
            };
        }
    }
}
