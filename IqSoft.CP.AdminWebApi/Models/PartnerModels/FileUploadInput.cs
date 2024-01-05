namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class FileUploadInput
    {
        public int PartnerId { get; set; }
        public int EnvironmentTypeId { get; set; }

        public string ImageData { get; set; }
        public string Image { get; set; }
        public string ImageType { get; set; }

    }
}