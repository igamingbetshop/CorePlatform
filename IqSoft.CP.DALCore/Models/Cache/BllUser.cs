using System;
namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllUser
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Gender { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string MobileNumber { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public string Email { get; set; }
        public int Type { get; set; }
        public int? Level { get; set; }
        public string SecurityCode { get; set; }
        public DateTime CreationTime { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Path { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
    }
}