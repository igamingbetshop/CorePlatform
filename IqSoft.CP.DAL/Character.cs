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
    
    public partial class Character
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Character()
        {
            this.Character1 = new HashSet<Character>();
            this.Clients = new HashSet<Client>();
        }
    
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public long TitleTranslationId { get; set; }
        public long DescriptionTranslationId { get; set; }
        public string ImageUrl { get; set; }
        public int Status { get; set; }
        public int Order { get; set; }
        public Nullable<System.DateTime> CreationTime { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public Nullable<int> ParentId { get; set; }
        public Nullable<int> CompPoints { get; set; }
        public string BackgroundImageUrl { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Character> Character1 { get; set; }
        public virtual Character Character2 { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual Translation Translation { get; set; }
        public virtual Translation Translation1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Clients { get; set; }
    }
}
