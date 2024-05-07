//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Client
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Client()
        {
            this.AgentCommissions = new HashSet<AgentCommission>();
            this.AgentProfits = new HashSet<AgentProfit>();
            this.ClientBonus = new HashSet<ClientBonu>();
            this.ClientBonusTriggers = new HashSet<ClientBonusTrigger>();
            this.ClientClassifications = new HashSet<ClientClassification>();
            this.ClientClosedPeriods = new HashSet<ClientClosedPeriod>();
            this.ClientFavoriteProducts = new HashSet<ClientFavoriteProduct>();
            this.ClientIdentities = new HashSet<ClientIdentity>();
            this.ClientLogs = new HashSet<ClientLog>();
            this.ClientMessageStates = new HashSet<ClientMessageState>();
            this.ClientPaymentInfoes = new HashSet<ClientPaymentInfo>();
            this.ClientSecurityAnswers = new HashSet<ClientSecurityAnswer>();
            this.ClientSessions = new HashSet<ClientSession>();
            this.ClientSettings = new HashSet<ClientSetting>();
            this.Documents = new HashSet<Document>();
            this.JobTriggers = new HashSet<JobTrigger>();
            this.PaymentLimits = new HashSet<PaymentLimit>();
            this.PaymentRequests = new HashSet<PaymentRequest>();
            this.Tickets = new HashSet<Ticket>();
            this.TicketMessageStates = new HashSet<TicketMessageState>();
            this.ClientPaymentSettings = new HashSet<ClientPaymentSetting>();
        }
    
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public int Salt { get; set; }
        public int PartnerId { get; set; }
        public Nullable<int> Gender { get; set; }
        public System.DateTime BirthDate { get; set; }
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
        public Nullable<int> DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public bool HasNote { get; set; }
        public string LanguageId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public Nullable<System.DateTime> FirstDepositDate { get; set; }
        public Nullable<System.DateTime> LastDepositDate { get; set; }
        public Nullable<decimal> LastDepositAmount { get; set; }
        public Nullable<int> BetShopId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> AffiliateReferralId { get; set; }
        public Nullable<long> LastSessionId { get; set; }
        public Nullable<int> Citizenship { get; set; }
        public Nullable<int> JobArea { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public string BuildingNumber { get; set; }
        public string Apartment { get; set; }
        public string NickName { get; set; }
        public string City { get; set; }
        public string USSDPin { get; set; }
        public Nullable<int> Title { get; set; }
        public Nullable<bool> IsTwoFactorEnabled { get; set; }
        public string QRCode { get; set; }
        public Nullable<int> CountryId { get; set; }
        public Nullable<int> CharacterId { get; set; }
    
        public virtual AffiliateReferral AffiliateReferral { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AgentCommission> AgentCommissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AgentProfit> AgentProfits { get; set; }
        public virtual Character Character { get; set; }
        public virtual ClientSession ClientSession { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual JobArea JobArea1 { get; set; }
        public virtual Language Language { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual Region Region { get; set; }
        public virtual Region Region1 { get; set; }
        public virtual User User { get; set; }
        public virtual ClientBankInfo ClientBankInfo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientBonu> ClientBonus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientBonusTrigger> ClientBonusTriggers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientClassification> ClientClassifications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientClosedPeriod> ClientClosedPeriods { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientFavoriteProduct> ClientFavoriteProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientIdentity> ClientIdentities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientLog> ClientLogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientMessageState> ClientMessageStates { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientPaymentInfo> ClientPaymentInfoes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSecurityAnswer> ClientSecurityAnswers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSession> ClientSessions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSetting> ClientSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Document> Documents { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JobTrigger> JobTriggers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentLimit> PaymentLimits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ticket> Tickets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketMessageState> TicketMessageStates { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientPaymentSetting> ClientPaymentSettings { get; set; }
    }
}
