using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
	public class FilterEmail : FilterBase<Email>
	{ 
	 public FiltersOperation Ids { get; set; }
	public FiltersOperation PartnerIds { get; set; }
	public FiltersOperation Subjects { get; set; }
	public FiltersOperation Statuses { get; set; }
	public FiltersOperation Receiver { get; set; }
	public DateTime? CreatedFrom { get; set; }
	public DateTime? CreatedBefore { get; set; }
	public int? ObjectId { get; set; }
	public int? ObjectTypeId { get; set; }

	protected override IQueryable<Email> CreateQuery(IQueryable<Email> objects, Func<IQueryable<Email>, IOrderedQueryable<Email>> orderBy = null)
	{
		if (CreatedFrom.HasValue)
			objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
		if (CreatedBefore.HasValue)
			objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);
		if (ObjectId.HasValue)
			objects = objects.Where(x => x.ObjectId == ObjectId.Value);
		if (ObjectTypeId.HasValue)
			objects = objects.Where(x => x.ObjectTypeId == ObjectTypeId.Value);

		FilterByValue(ref objects, Ids, "Id");
		FilterByValue(ref objects, PartnerIds, "PartnerId");
		FilterByValue(ref objects, Subjects, "Subject");
		FilterByValue(ref objects, Statuses, "Status");
		FilterByValue(ref objects, Receiver, "Receiver");

		return base.FilteredObjects(objects, orderBy);
	}

	public IQueryable<Email> FilterObjects(IQueryable<Email> clientMessages, Func<IQueryable<Email>, IOrderedQueryable<Email>> orderBy = null)
	{
		return CreateQuery(clientMessages, orderBy);
	}

	public long SelectedObjectsCount(IQueryable<Email> clientMessages)
	{
		clientMessages = CreateQuery(clientMessages);
		return clientMessages.Count();
	}
}
}