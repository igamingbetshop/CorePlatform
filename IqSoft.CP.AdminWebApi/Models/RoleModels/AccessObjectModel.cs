namespace IqSoft.CP.AdminWebApi.Models.RoleModels
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