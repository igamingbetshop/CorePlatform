//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class AdminMenu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AdminMenu()
        {
            this.UserStates = new HashSet<UserState>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string Route { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Path { get; set; }
        public string ApiRequest { get; set; }
        public string PermissionId { get; set; }
        public Nullable<int> Priority { get; set; }
    
        public virtual AdminMenu AdminMenu1 { get; set; }
        public virtual AdminMenu AdminMenu2 { get; set; }
        public virtual Permission Permission { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserState> UserStates { get; set; }
    }
}
