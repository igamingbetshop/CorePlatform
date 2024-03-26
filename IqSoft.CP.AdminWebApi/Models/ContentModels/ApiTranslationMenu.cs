namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiMenuTranslation
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public int MenuItemId { get; set; }
        public int Order { get; set; }
        public int InterfaceType { get; set; }
    }
}