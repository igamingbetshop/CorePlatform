using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class AddClientIdentityModel
    {
        public int Id { get; set; } 

        public int ClientId { get; set; } 

        public string Name { get; set; }

        public string ImageData { get; set; }

        public string Extension { get; set; }

        public int DocumentTypeId { get; set; }

        public int State { get; set; }

        public int? UserId { get; set; }

        public DateTime? ExpirationTime { get; set; }

    }
}