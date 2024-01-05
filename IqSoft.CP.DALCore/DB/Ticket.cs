﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Ticket
    {
        public Ticket()
        {
            TicketMessages = new HashSet<TicketMessage>();
        }

        public long Id { get; set; }
        public int Status { get; set; }
        public string Subject { get; set; }
        public int Type { get; set; }
        public DateTime CreationTime { get; set; }
        public int PartnerId { get; set; }
        public int? ClientId { get; set; }
        public long LastMessageDate { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int? ClientUnreadMessagesCount { get; set; }
        public int? UserUnreadMessagesCount { get; set; }
        public int? LastMessageUserId { get; set; }

        public virtual Client Client { get; set; }
        public virtual User LastMessageUser { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual ICollection<TicketMessage> TicketMessages { get; set; }
    }
}