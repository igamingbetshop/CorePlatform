namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class ObjectTypeModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool SaveChangeHistory { get; set; }

        public bool HasTranslation { get; set; }
    }
}