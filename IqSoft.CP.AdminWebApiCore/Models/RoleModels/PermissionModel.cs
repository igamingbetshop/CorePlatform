namespace IqSoft.CP.AdminWebApi.Models.RoleModels
{
    public class PermissionModel
    {
        public string Id { get; set; }
        
        public string Name { get; set; }

        public int PermissionGroupId { get; set; }

        public int ObjectTypeId { get; set; }
    }

    public class PermissionGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}