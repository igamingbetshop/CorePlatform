﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Partner
    {
        public Partner()
        {
            AffiliatePlatforms = new HashSet<AffiliatePlatform>();
            Announcements = new HashSet<Announcement>();
            Banners = new HashSet<Banner>();
            BetShopGroups = new HashSet<BetShopGroup>();
            BetShops = new HashSet<BetShop>();
            Bonus = new HashSet<Bonu>();
            CRMSettings = new HashSet<CRMSetting>();
            Categories = new HashSet<Category>();
            ClientInfoes = new HashSet<ClientInfo>();
            Clients = new HashSet<Client>();
            CommentTemplates = new HashSet<CommentTemplate>();
            ComplimentaryPointRates = new HashSet<ComplimentaryPointRate>();
            Emails = new HashSet<Email>();
            Merchants = new HashSet<Merchant>();
            MessageTemplates = new HashSet<MessageTemplate>();
            PartnerBankInfoes = new HashSet<PartnerBankInfo>();
            PartnerCountrySettings = new HashSet<PartnerCountrySetting>();
            PartnerCurrencySettings = new HashSet<PartnerCurrencySetting>();
            PartnerKeys = new HashSet<PartnerKey>();
            PartnerLanguageSettings = new HashSet<PartnerLanguageSetting>();
            PartnerPaymentSettings = new HashSet<PartnerPaymentSetting>();
            PartnerProductSettings = new HashSet<PartnerProductSetting>();
            PromoCodes = new HashSet<PromoCode>();
            Promotions = new HashSet<Promotion>();
            Roles = new HashSet<Role>();
            SecurityQuestions = new HashSet<SecurityQuestion>();
            Segments = new HashSet<Segment>();
            Tickets = new HashSet<Ticket>();
            TriggerSettings = new HashSet<TriggerSetting>();
            Users = new HashSet<User>();
            WebSiteMenus = new HashSet<WebSiteMenu>();
            Affiliates = new HashSet<Affiliate>();
            ProductCountrySettings = new HashSet<ProductCountrySetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string CurrencyId { get; set; }
        public string SiteUrl { get; set; }
        public string AdminSiteUrl { get; set; }
        public int State { get; set; }
        public long SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public TimeSpan AccountingDayStartTime { get; set; }
        public int ClientMinAge { get; set; }
        public string PasswordRegExp { get; set; }
        public int VerificationType { get; set; }
        public int EmailVerificationCodeLength { get; set; }
        public int MobileVerificationCodeLength { get; set; }
        public decimal UnusedAmountWithdrawPercent { get; set; }
        public int UserSessionExpireTime { get; set; }
        public int UnpaidWinValidPeriod { get; set; }
        public int VerificationKeyActiveMinutes { get; set; }
        public decimal AutoApproveBetShopDepositMaxAmount { get; set; }
        public decimal AutoApproveWithdrawMaxAmount { get; set; }
        public int ClientSessionExpireTime { get; set; }
        public decimal AutoConfirmWithdrawMaxAmount { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual UserSession Session { get; set; }
        public virtual ICollection<AffiliatePlatform> AffiliatePlatforms { get; set; }
        public virtual ICollection<Announcement> Announcements { get; set; }
        public virtual ICollection<Banner> Banners { get; set; }
        public virtual ICollection<BetShopGroup> BetShopGroups { get; set; }
        public virtual ICollection<BetShop> BetShops { get; set; }
        public virtual ICollection<Bonu> Bonus { get; set; }
        public virtual ICollection<CRMSetting> CRMSettings { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<ClientInfo> ClientInfoes { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
        public virtual ICollection<CommentTemplate> CommentTemplates { get; set; }
        public virtual ICollection<ComplimentaryPointRate> ComplimentaryPointRates { get; set; }
        public virtual ICollection<Email> Emails { get; set; }
        public virtual ICollection<Merchant> Merchants { get; set; }
        public virtual ICollection<MessageTemplate> MessageTemplates { get; set; }
        public virtual ICollection<PartnerBankInfo> PartnerBankInfoes { get; set; }
        public virtual ICollection<PartnerCountrySetting> PartnerCountrySettings { get; set; }
        public virtual ICollection<PartnerCurrencySetting> PartnerCurrencySettings { get; set; }
        public virtual ICollection<PartnerKey> PartnerKeys { get; set; }
        public virtual ICollection<PartnerLanguageSetting> PartnerLanguageSettings { get; set; }
        public virtual ICollection<PartnerPaymentSetting> PartnerPaymentSettings { get; set; }
        public virtual ICollection<PartnerProductSetting> PartnerProductSettings { get; set; }
        public virtual ICollection<PromoCode> PromoCodes { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<SecurityQuestion> SecurityQuestions { get; set; }
        public virtual ICollection<Segment> Segments { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<TriggerSetting> TriggerSettings { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<WebSiteMenu> WebSiteMenus { get; set; }
        public virtual ICollection<Affiliate> Affiliates { get; set; }
        public virtual ICollection<ProductCountrySetting> ProductCountrySettings { get; set; }
    }
}