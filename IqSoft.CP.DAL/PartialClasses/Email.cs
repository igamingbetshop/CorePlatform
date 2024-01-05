using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
	public partial class Email : IBase
	{
		long IBase.ObjectId
		{
			get { return Id; }
		}
	}
}
