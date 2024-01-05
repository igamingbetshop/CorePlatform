using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class AccessObject : IBase
    {
        public bool ShouldSerializePermission()
        {
            return false;
        }
    }
}