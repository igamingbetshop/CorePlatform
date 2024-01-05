using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models.Cache;
using log4net;
using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Helpers
{

    public static class AMLStatusHelpers
    {
        private enum AMLServiceTypes
        {
            DigitalCustomer = 1,
            UKSanctionsCustomService = 2
        }

        public static AMLStatus GetAMLStatus(BllClient client, int countryId, ILog log)
        {
            log.Info("Aml Status Check_" + client.Id);
            var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLServiceType);
            var result = new AMLStatus
            {
                Status = Common.Enums.AMLStatuses.NA,
                Error = "AML service not found"
            };
            if (!string.IsNullOrEmpty(partnerConfig) && int.TryParse(partnerConfig, out int partnerAMLServiceId))
            {
                switch (partnerAMLServiceId)
                {
                    case (int)AMLServiceTypes.DigitalCustomer:
                        result = DigitalCustomerHelpers.GetAMLStatus(client, countryId, log);
                        break;
                    case (int)AMLServiceTypes.UKSanctionsCustomService:
                        result = UKSanctionsCustomService.GetAMLStatus(client, log);
                        break;
                    default:
                        break;
                }
            }
            log.Info("Aml Status Response_" + JsonConvert.SerializeObject(result));
            return result;
        }
    }
}
