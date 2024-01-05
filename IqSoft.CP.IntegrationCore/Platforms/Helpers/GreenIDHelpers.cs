using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Models.GreenID;
using log4net;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class GreenIDHelpers
    {
        public static bool IsDocumentVerified(int clientId, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            if (string.IsNullOrWhiteSpace(client.DocumentNumber) || !client.DocumentType.HasValue ||
                (client.DocumentType.Value != (int)KYCDocumentTypes.Passport &&
                client.DocumentType.Value != (int)KYCDocumentTypes.DriverLicense))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientIdentityNotFound);
            if (string.IsNullOrWhiteSpace(client.ZipCode))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.BuildingNumber) || string.IsNullOrWhiteSpace(client.Apartment))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);

            using (var regionBl = new RegionBll(session, log))
            {
                var regionPath = regionBl.GetRegionPath(client.RegionId);
                var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                if (country == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var town = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Town);
                if (town == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var accountId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GreenIDAccountId).StringValue;
                var apiCode = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GreenIDApiCode).StringValue;
                var verificationData = RegisterVerification(accountId, apiCode, client, country.IsoCode, CacheManager.GetRegionById(town.Id.Value, session.LanguageId)?.NickName);
                var result = SetField(accountId, apiCode, verificationData.VerificationToken, client);

                var verificationResult = GetVerificationResult(client.PartnerId, accountId, verificationData.VerificationToken);
                if (verificationResult.Error)
                    throw new System.Exception(verificationResult.ErrorMessage);
                return verificationResult.VerificationResult.Contains("VERIFIED");

            }
        }

        private static BaseOutput RegisterVerification(string accountId, string apiCode, BllClient client, string countryCode, string town)
        {
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GreenIDApiUrl).StringValue;
            var input = new
            {
                accountId,
                apiCode,
                givenNames = client.FirstName,
                surname = client.LastName,
                dob = client.BirthDate.ToString("dd/MM/yyyy"),
                country = countryCode,
                flatNumber = client.Apartment,
                postcode = client.ZipCode.Trim(),
                email = client.Email,
                state = "ACT",
                streetName = client.Apartment,
                streetNumber = client.BuildingNumber,
                streetType = "ACCS",
                suburb = town
            };

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                PostData = JsonConvert.SerializeObject(input),
                Url = $"{url}/register",
            };
            return JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }

        private static string SetField(string accountId, string apiCode, string verificationToken, BllClient client)
        {
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GreenIDApiUrl).StringValue;
            var inputString = string.Empty;
            switch (client.DocumentType)
            {
                case (int)KYCDocumentTypes.Passport:
                    inputString = JsonConvert.SerializeObject(
                        new
                        {
                            accountId,
                            apiCode,
                            greenid_passportdvs_dob = client.BirthDate.ToString("dd/MM/yyyy"),
                            greenid_passportdvs_givenname = client.FirstName,
                            greenid_passportdvs_surname = client.LastName,
                            greenid_passportdvs_number = client.DocumentNumber,
                            greenid_passportdvs_tandc = "on",
                            origin = "simpleui",
                            sourceId = "passportdvs",
                            verificationToken
                        });
                    break;
                case (int)KYCDocumentTypes.DriverLicense:
                    var docNumbers = client.DocumentNumber.Split('/');
                    inputString = JsonConvert.SerializeObject(
                        new
                        {
                            accountId,
                            apiCode,
                            greenid_actregodvs_number = docNumbers[0],
                            greenid_actregodvs_cardnumber = docNumbers[1],
                            greenid_actregodvs_dob = client.BirthDate.ToString("dd/MM/yyyy"),
                            greenid_actregodvs_givenname = client.FirstName,
                            greenid_actregodvs_surname = client.LastName,
                            greenid_actregodvs_tandc = "on",
                            origin = "simpleui",
                            sourceId = "actregodvs",
                            verificationToken
                        });
                    break;
                default:
                    break;
            }

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                PostData = inputString,
                Url = $"{url}/setfields",
            };
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        private static VerificationResultOutput GetVerificationResult(int partnerId, string accountId, string verificationToken)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.GreenIDApiUrl).StringValue;
            var apiPassword = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.GreenIDApiPassword).StringValue;
            var input = new
            {
                accountId,
                webServicePassword = apiPassword,
                verificationToken
            };

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{url}/verificationResult?{CommonFunctions.GetUriDataFromObject(input)}",
            };
            return JsonConvert.DeserializeObject<VerificationResultOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }
    }
}