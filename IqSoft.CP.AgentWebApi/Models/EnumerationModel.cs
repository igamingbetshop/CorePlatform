namespace IqSoft.CP.AgentWebApi.Models
{
    public class EnumerationModel <T>
    {
        public T Id { get; set; }

        public string NickName { get; set; }

        public string Name { get; set; }

        public string Info { get; set; }
    }
}