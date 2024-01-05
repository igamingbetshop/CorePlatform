using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterClientSession : FilterBase<ClientSession>
    {
        public long? Id { get; set; }

        public int? ClientId { get; set; }

        public string LanguageId { get; set; }

        public string Ip { get; set; }

        public string Token { get; set; }

        public int? ProductId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? State { get; set; }

        protected override IQueryable<ClientSession> CreateQuery(IQueryable<ClientSession> objects, Func<IQueryable<ClientSession>, IOrderedQueryable<ClientSession>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId.Value);
            if (ProductId.HasValue)
                objects = objects.Where(x => x.ProductId == ProductId.Value);
            if (StartTime.HasValue)
                objects = objects.Where(x => x.StartTime == StartTime.Value);
            if (LastUpdateTime.HasValue)
                objects = objects.Where(x => x.LastUpdateTime == LastUpdateTime.Value);
            if (EndTime.HasValue)
                objects = objects.Where(x => x.EndTime == EndTime.Value);
            if (State.HasValue)
                objects = objects.Where(x => x.State == State.Value);
            if (!string.IsNullOrWhiteSpace(LanguageId))
                objects = objects.Where(x => x.LanguageId.Contains(LanguageId));
            if (!string.IsNullOrWhiteSpace(Ip))
                objects = objects.Where(x => x.Ip.Contains(Ip));
            if (!string.IsNullOrWhiteSpace(Token))
                objects = objects.Where(x => x.Token.Contains(Token));

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<ClientSession> FilterObjects(IQueryable<ClientSession> clientSessions, Func<IQueryable<ClientSession>, IOrderedQueryable<ClientSession>> orderBy = null)
        {
            clientSessions = CreateQuery(clientSessions, orderBy);
            return clientSessions;
        }

        public long SelectedObjectsCount(IQueryable<ClientSession> clientSessions)
        {
            clientSessions = CreateQuery(clientSessions);
            return clientSessions.Count();
        }
    }
}
