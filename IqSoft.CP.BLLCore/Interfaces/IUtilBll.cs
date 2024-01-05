using System.Collections.Generic;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IUtilBll : IBaseBll
    {
        Note SaveNote(Note note);

        List<fnNote> GetNotes(FilterNote filter);

        long CreateTranslation(int objectTypeId, string text);

        List<ObjectType> GetObjectTypes();
    }
}
