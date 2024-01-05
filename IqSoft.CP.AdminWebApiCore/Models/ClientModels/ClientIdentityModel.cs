using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ClientIdentityModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public int ClientId { get; set; }

        public string ImagePath { get; set; }

        public int? UserId { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public int DocumentTypeId { get; set; }

        public int State { get; set; }

        public bool HasNote { get; set; }
    }
}