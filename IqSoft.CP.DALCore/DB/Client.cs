﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Client
    {
        public Client()
        {
            AgentCommissions = new HashSet<AgentCommission>();
            Bets = new HashSet<Bet>();
            ClientBonus = new HashSet<ClientBonu>();
            ClientBonusTriggers = new HashSet<ClientBonusTrigger>();
            ClientClassifications = new HashSet<ClientClassification>();
            ClientClosedPeriods = new HashSet<ClientClosedPeriod>();
            ClientFavoriteProducts = new HashSet<ClientFavoriteProduct>();
            ClientIdentities = new HashSet<ClientIdentity>();
            ClientInfoes = new HashSet<ClientInfo>();
            ClientLogs = new HashSet<ClientLog>();
            ClientMessageStates = new HashSet<ClientMessageState>();
            ClientMessages = new HashSet<ClientMessage>();
            ClientPaymentInfoes = new HashSet<ClientPaymentInfo>();
            ClientPaymentSettings = new HashSet<ClientPaymentSetting>();
            ClientSessions = new HashSet<ClientSession>();
            ClientSettings = new HashSet<ClientSetting>();
            Documents = new HashSet<Document>();
            JobTriggers = new HashSet<JobTrigger>();
            PaymentRequests = new HashSet<PaymentRequest>();
            SecurityQuestions = new HashSet<SecurityQuestion>();
            TicketMessageStates = new HashSet<TicketMessageState>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public int Salt { get; set; }
        public int PartnerId { get; set; }
        public int? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public bool CallToPhone { get; set; }
        public bool SendPromotions { get; set; }
        public int State { get; set; }
        public int CategoryId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RegionId { get; set; }
        public string Info { get; set; }
        public string ZipCode { get; set; }
        public string RegistrationIp { get; set; }
        public int? DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public bool HasNote { get; set; }
        public string LanguageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime? FirstDepositDate { get; set; }
        public DateTime? LastDepositDate { get; set; }
        public decimal? LastDepositAmount { get; set; }
        public int? BetShopId { get; set; }
        public int? UserId { get; set; }
        public int? AffiliateReferralId { get; set; }
        public long? LastSessionId { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public string BuildingNumber { get; set; }
        public string Apartment { get; set; }
        public string NickName { get; set; }
        public string City { get; set; }

        public virtual AffiliateReferral AffiliateReferral { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual JobArea JobAreaNavigation { get; set; }
        public virtual Language Language { get; set; }
        public virtual ClientSession LastSession { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual Region Region { get; set; }
        public virtual User User { get; set; }
        public virtual PaymentLimit PaymentLimit { get; set; }
        public virtual ICollection<AgentCommission> AgentCommissions { get; set; }
        public virtual ICollection<Bet> Bets { get; set; }
        public virtual ICollection<ClientBonu> ClientBonus { get; set; }
        public virtual ICollection<ClientBonusTrigger> ClientBonusTriggers { get; set; }
        public virtual ICollection<ClientClassification> ClientClassifications { get; set; }
        public virtual ICollection<ClientClosedPeriod> ClientClosedPeriods { get; set; }
        public virtual ICollection<ClientFavoriteProduct> ClientFavoriteProducts { get; set; }
        public virtual ICollection<ClientIdentity> ClientIdentities { get; set; }
        public virtual ICollection<ClientInfo> ClientInfoes { get; set; }
        public virtual ICollection<ClientLog> ClientLogs { get; set; }
        public virtual ICollection<ClientMessageState> ClientMessageStates { get; set; }
        public virtual ICollection<ClientMessage> ClientMessages { get; set; }
        public virtual ICollection<ClientPaymentInfo> ClientPaymentInfoes { get; set; }
        public virtual ICollection<ClientPaymentSetting> ClientPaymentSettings { get; set; }
        public virtual ICollection<ClientSession> ClientSessions { get; set; }
        public virtual ICollection<ClientSetting> ClientSettings { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<JobTrigger> JobTriggers { get; set; }
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; }
        public virtual ICollection<SecurityQuestion> SecurityQuestions { get; set; }
        public virtual ICollection<TicketMessageState> TicketMessageStates { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<ClientSecurityAnswer> ClientSecurityAnswers { get; set; }
    }
}