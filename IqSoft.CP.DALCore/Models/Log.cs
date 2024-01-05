namespace IqSoft.CP.DAL
{
    public partial class Log
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Caller { get; set; }
        public string Message { get; set; }
        public string FunctionName { get; set; }
        public System.DateTime CreationTime { get; set; }
    }
}