using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.AdminModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.BLL.Services
{
    public class UtilBll : PermissionBll, IUtilBll
    {
        #region Constructors

        public UtilBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public UtilBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Note SaveNote(Note note)
        {
            CheckPermission(Constants.Permissions.CreateNote);
            var currentTime = GetServerDate();
            var dbNote = Db.Notes.FirstOrDefault(x => x.Id == note.Id);

            if (dbNote == null)
            {
                dbNote = new Note { CreationTime = currentTime };
                Db.Notes.Add(dbNote);
            }
            
            Db.Entry(dbNote).CurrentValues.SetValues(note);
            dbNote.LastUpdateTime = currentTime;
            dbNote.SessionId = SessionId;
            SaveChanges();
            return dbNote;
        }

        public List<fnNote> GetNotes(FilterNote filter)
        {
            Func<IQueryable<fnNote>, IOrderedQueryable<fnNote>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnNote>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnNote>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = note => note.OrderByDescending(x => x.CreationTime);
            }

            return filter.FilterObjects(Db.fn_Note(), orderBy).ToList();
        }

        public PagedModel<TranslationModel> GetfnTranslationEntriesPagedModel(FilterfnObjectTranslationEntry filter)
        {
            if (string.IsNullOrEmpty(filter.SearchText))
                filter.SearchText = string.Empty;
            else
                filter.SearchText = '%' + filter.SearchText + '%';


            var translations = new List<fnObjectTranslationEntry>();
            foreach (var l in filter.SelectedLanguages)
            {
                var items = filter.FilterObjects(Db.fn_ObjectTranslationEntry(filter.ObjectTypeId, filter.SearchText, l), x => x.OrderByDescending(y => y.TranslationId)).ToList();
                translations.AddRange(items);
            }
            var count = filter.SelectedObjectsCount(Db.fn_ObjectTranslationEntry(filter.ObjectTypeId, filter.SearchText, Constants.DefaultLanguageId));
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                var ids = translations.Select(x => x.TranslationId).Distinct().ToList();
                translations = new List<fnObjectTranslationEntry>();

                foreach (var l in filter.SelectedLanguages)
                {
                    var items = Db.fn_ObjectTranslationEntry(filter.ObjectTypeId, string.Empty, l).Where(x => ids.Contains(x.TranslationId)).ToList();
                    translations.AddRange(items);
                }
                count = filter.TakeCount;
            }

            var mapedTranslations = new PagedModel<TranslationModel>();
            mapedTranslations.Count = count;
            mapedTranslations.Entities = (from translation in translations
            group translation by translation.TranslationId into y
            select new TranslationModel
            {
                TranslationId = y.Key ?? 0,
                ObjectTypeId = filter.ObjectTypeId,
                NickName = y.First().NickName,
                TranslationEntries = y.Select(x => new Common.Models.AdminModels.TranslationEntry
                {
                    LanguageId = x.LanguageId,
                    Text = x.Text
                }).OrderBy(x => x.LanguageId).ToList()
            }).ToList();

            return mapedTranslations;
        }

        public long CreateTranslation(int objectTypeId, string text)
        {
            var currentDate = GetServerDate();
            var translation = new Translation
            {
                ObjectTypeId = objectTypeId,
                SessionId = SessionId,
                CreationTime = currentDate,
                LastUpdateTime = currentDate
            };
            Db.Translations.Add(translation);
            Db.SaveChanges();

            var translationEntry = new DAL.TranslationEntry
            {
                TranslationId = translation.Id,
                LanguageId = LanguageId,
                Text = text,
                SessionId = SessionId,
                CreationTime = currentDate,
                LastUpdateTime = currentDate
            };
            Db.TranslationEntries.Add(translationEntry);
            Db.SaveChanges();

            var result = translation.Id;
            return result;
        }

        public List<ObjectType> GetObjectTypes()
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewObjectTypes
            });
            return Db.ObjectTypes.Where(x => x.HasTranslation).ToList();
        }
    }
}
