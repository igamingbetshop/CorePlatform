namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiWebSiteMenuItem
    {
        public int? Id { get; set; }

        public int MenuId { get; set; }

        public string Icon { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string StyleType { get; set; }

        public string Href { get; set; }

        public bool OpenInRouting { get; set; }

        public bool Orientation { get; set; }

        public int Order { get; set; }

        public string Image { get; set; }
    }
}