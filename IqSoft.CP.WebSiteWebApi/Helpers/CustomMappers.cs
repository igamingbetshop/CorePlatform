using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.WebSiteWebApi.Models.PaymentModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.WebSiteWebApi.Helpers
{
    public static class CustomMappers
    {
        public static int GetOperationSystemType(string source)
        {
            var osType = (int)OSTypes.Windows;
            if (string.IsNullOrEmpty(source))
                return osType;
            //if (source.Contains("Linux")) //check
            //    osType = (int)OSTypes.Linux;
            if (source.Contains("iPhone"))
                osType = (int)OSTypes.IPhone;
            else if (source.Contains("iPad"))
                osType = (int)OSTypes.IPad;
            else if (source.Contains("Android"))
                osType = (int)OSTypes.Android;
            else if (source.Contains("Mac OS"))
                osType = (int)OSTypes.Mac;

            return osType;
        }

        public static ApiPartnerPaymentSystemsOutput ToApiPartnerPaymentSystemsOutput(this GetPartnerPaymentSystemsOutput input)
        {
            return new ApiPartnerPaymentSystemsOutput
            {
                ResponseCode = input.ResponseCode,
                Description = input.Description,
                ResponseObject = input.ResponseObject,
                PartnerPaymentSystems = input.PartnerPaymentSystems.Select(x => x.ToApiPartnerPaymentSystem()).ToList()
            };
        }
        public static ApiPartnerPaymentSystem ToApiPartnerPaymentSystem(this PartnerPaymentSettingModel input)
        {
            List<decimal> info = null;
            try
            {
                if (!string.IsNullOrEmpty(input.Info))
                    info = JsonConvert.DeserializeObject<List<decimal>>(input.Info);
            }
            catch { }
            return new ApiPartnerPaymentSystem
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                PaymentSystemId = input.PaymentSystemId,
                CommissionPercent = input.Commission,
                FixedFee = input.FixedFee,
                State = input.State,
                PaymentSystemType = input.PaymentSystemType,
                CurrencyId = input.CurrencyId,
                CreationTime = input.CreationTime,
                PaymentSystemName = input.PaymentSystemName,
                PaymentSystemPriority = input.PaymentSystemPriority,
                Type = input.Type,
                ContentType = input.ContentType,
                Info = info,
                Address = input.Address,
                DestinationTag = input.DestinationTag,
                MinAmount = input.MinAmount,
                MaxAmount = input.MaxAmount,
                HasBank = input.HasBank,
                ImageExtension = string.IsNullOrEmpty(input.ImageExtension) ? CP.Common.Constants.Extensions.Png : input.ImageExtension
            };
        }

        public static ApiBannerOutput ToApiBannerOutput(this BllBanner input)
        {
            var buttonTypes = Enumerable.Repeat(false, 3).ToArray();
            if (input.ButtonType.HasValue)
                buttonTypes = input.ButtonType.ToString().Select(x => x.Equals('1')).ToArray();
           
            return new ApiBannerOutput
            {
                Body = input.Body,
                Head = input.Head,
                Link = input.Link,
                Order = input.Order,
                Image = input.Image,
                ImageSizes = input.ImageSizes,
                ShowDescription = input.ShowDescription,
                ShowRegistration = buttonTypes.ElementAtOrDefault(1),
                ShowLogin = buttonTypes.ElementAtOrDefault(2),
                Visibility = input.Visibility
            };
        }

        public static ApiGeolocationData ToApiGeolocationData(this BllGeolocationData input)
        {
            return new ApiGeolocationData
            {
                Id = input.Id,
                CountryCode = input.CountryCode,
                LanguageId = input.LanguageId,
                CurrencyId = input.CurrencyId
            };
        }
    }
}
