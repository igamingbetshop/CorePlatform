using System.Collections.Generic;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.BLL.Interfaces
{
	using IqSoft.CP.DAL;
	using System;

	public interface IPartnerBll : IBaseBll
    {
        Partner GetPartnerById(int id);

        PagedModel<Partner> GetPartnersPagedModel(FilterPartner filter);

        List<Partner> GetPartners(FilterPartner filter, bool checkPermissions = true);

        Partner SavePartner(Partner partner);

        List<PartnerKey> GetPartnerKeys(int partnerId);

        PartnerKey SavePartnerKey(PartnerKey partnerKey);

        bool IsPartnerIdExists(int partnerId);

        List<Partner> ExportPartners(FilterPartner filter, bool checkPermissions = true);

        List<Partner> ExportPartnersModel(FilterPartner filter);

		bool ChangePartnerAccountBalance(int? partnerId, DateTime endTime);
    }
}
