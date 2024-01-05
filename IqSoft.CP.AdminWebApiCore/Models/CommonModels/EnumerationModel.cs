namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class EnumerationModel <T>
    {
        public T Id { get; set; }

        public string NickName { get; set; }

        public string Name { get; set; }

        public string Info { get; set; }
    }
}