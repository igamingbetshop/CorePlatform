namespace IqSoft.CP.DAL.Models.User
{
    public class RolePermissionModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string PermissionId { get; set; }
        public bool IsForAll { get; set; }
        public PermissionModel Permission { get; set; }
    }
}
