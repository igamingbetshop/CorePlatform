using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterRole : FilterBase<Role>
    {
        public int? Id { get; set; }
        public int? PartnerId { get; set; }
        public bool? IsStatic { get; set; }
        public bool? IsAdmin { get; set; }
        public string Name { get; set; }
        public List<string> PermissionIds { get; set; }

        protected override IQueryable<Role> CreateQuery(IQueryable<Role> objects, Func<IQueryable<Role>, IOrderedQueryable<Role>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (IsStatic.HasValue)
                objects = objects.Where(x => x.IsStatic == IsStatic.Value);
            if (IsAdmin.HasValue)
                objects = objects.Where(x => x.IsAdmin == IsAdmin.Value);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name.Contains(Name));
            if (PermissionIds != null && PermissionIds.Count != 0)
                objects = objects.Where(x => PermissionIds.All(z => x.RolePermissions.Any(y => y.PermissionId == z)));
            if(PartnerId != null)
                objects = objects.Where(x => x.PartnerId == null || x.PartnerId == PartnerId.Value);

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Role> FilterObjects(IQueryable<Role> roles, Func<IQueryable<Role>, IOrderedQueryable<Role>> orderBy = null)
        {
            roles = CreateQuery(roles, orderBy);
            return roles;
        }

        public long SelectedObjectsCount(IQueryable<Role> roles)
        {
            roles = CreateQuery(roles);
            return roles.Count();
        }
    }
}
