namespace IqSoft.NGGP.WebApplications.AdminWebApi.Models.UserModels
{
    public class AccessObjectModel
    {
        public int Id { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public int UserId { get; set; }
        public string PermissionId { get; set; }
    }
}