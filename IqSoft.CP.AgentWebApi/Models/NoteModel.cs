using System;
namespace IqSoft.CP.AgentWebApi.Models
{
    public class NoteModel
    {
        public long Id { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public DateTime CreationTime { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Message { get; set; }
        public string CreatorFirstName { get; set; }
        public string CreatorLastName { get; set; }
    }
}