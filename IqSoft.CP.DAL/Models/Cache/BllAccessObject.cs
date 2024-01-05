using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllAccessObject
    {
        public int Id { get; set; }

        public int ObjectTypeId { get; set; }

        public long ObjectId { get; set; }

        public int UserId { get; set; }

        public string PermissionId { get; set; }
    }
}
