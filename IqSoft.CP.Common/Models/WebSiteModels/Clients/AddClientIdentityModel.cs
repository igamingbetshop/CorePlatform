namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class AddClientIdentityModel
    {
        public int Id { get; set; } 

        public int ClientId { get; set; } 

        public string Name { get; set; }

        public string ImageData { get; set; }

        public string Extension { get; set; }

        public int DocumentTypeId { get; set; }
    }
}