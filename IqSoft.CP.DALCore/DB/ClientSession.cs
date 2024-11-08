﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientSession
    {
        public ClientSession()
        {
            ClientLogs = new HashSet<ClientLog>();
            Clients = new HashSet<Client>();
            InverseParent = new HashSet<ClientSession>();
        }

        public long Id { get; set; }
        public int ClientId { get; set; }
        public string LanguageId { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public string Token { get; set; }
        public int ProductId { get; set; }
        public int DeviceType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int State { get; set; }
        public string CurrentPage { get; set; }
        public long? ParentId { get; set; }
        public string ExternalToken { get; set; }
        public string Source { get; set; }
        public int? LogoutType { get; set; }

        public virtual Client Client { get; set; }
        public virtual Language Language { get; set; }
        public virtual ClientSession Parent { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<ClientLog> ClientLogs { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
        public virtual ICollection<ClientSession> InverseParent { get; set; }
    }
}