using System.Collections.Generic;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IProductBll : IBaseBll
    {
        Product SaveProduct(fnProduct product, string comment, FtpModel model);

        fnProduct GetfnProductById(int id, bool checkPermission, string languageId = null);

        PagedModel<fnProduct> GetFnProducts(FilterfnProduct filter, bool checkPermission);

        List<Product> GetProducts(FilterProduct filter, bool checkPermission);

        List<PartnerProductSetting> GetPartnerProductSettings(FilterPartnerProductSetting filter);

        List<GameProvider> GetGameProviders(FilterGameProvider filter, bool checkPermission);

        List<fnProduct> ExportFnProducts(FilterfnProduct filter);
    }
}
