using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;
using System;
using System.Web;

namespace IqSoft.CP.Integration.Extensions
{
    public static class CustomMapper
    {
        public static string EncodeDate(this DateTime date)
        {
            return HttpUtility.UrlEncode(date.ToString("yyyy-MM-ddTHH:mm:ss"));
        }

		public static Bonu ToBonus(this BonusInfo info)
		{
			return new Bonu
			{
				Id = info.Id,
				Name = info.Name,
				PartnerId = info.PartnerId,
				AccountTypeId = info.AccountTypeId,
				Status = info.Status,
				StartTime = info.StartTime,
				FinishTime = info.FinishTime,
				Period = info.Period,
				BonusType = info.BonusType,
				Info = info.Info,
				TurnoverCount = info.TurnoverCount,
				MinAmount = info.MinAmount,
				MaxAmount = info.MaxAmount,
				Sequence = info.Sequence
			};
		}

		public static BllClient ToBllClient(this Client input)
		{
            return new BllClient
            {
                Id = input.Id,
                Email = input.Email,
                IsEmailVerified = input.IsEmailVerified,
                CurrencyId = input.CurrencyId,
                UserName = input.UserName,
                PasswordHash = input.PasswordHash,
                Salt = input.Salt,
                PartnerId = input.PartnerId,
                Gender = input.Gender,
                BirthDate = input.BirthDate,
                SendMail = input.SendMail,
                SendSms = input.SendSms,
                SendPromotions = input.SendPromotions,
                State = input.State,
                CategoryId = input.CategoryId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                RegionId = input.RegionId,
                Info = input.Info,
                DocumentType = input.DocumentType,
                DocumentNumber = input.DocumentNumber,
                DocumentIssuedBy = input.DocumentIssuedBy,
                IsDocumentVerified = input.IsDocumentVerified,
                Address = input.Address,
                MobileNumber = input.MobileNumber,
                IsMobileNumberVerified = input.IsMobileNumberVerified,
                LanguageId = input.LanguageId,
                CreationTime = input.CreationTime,
                BetShopId = input.BetShopId,
                UserId = input.UserId,
                AffiliateReferralId = input.AffiliateReferralId,
                LastSessionId = input.LastSessionId
            };
		}
	}
}