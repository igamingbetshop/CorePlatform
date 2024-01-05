using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ClientHistoryModel
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public DateTime ChangeDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}