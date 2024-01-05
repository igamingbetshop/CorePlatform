using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
	public partial class fnCharacter
	{
		public List<fnCharacter> Children { get; set; } = new List<fnCharacter>();
	}
}
