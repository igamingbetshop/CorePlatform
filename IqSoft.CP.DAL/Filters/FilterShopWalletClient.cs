using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
	public class FilterShopWalletClient : FilterBase<fnShopWalletClient>
	{
		public int? Id { get; set; }

		public string Email { get; set; }

		public string UserName { get; set; }

		public string CurrencyId { get; set; }

		public int? Gender { get; set; }

		public string Info { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string DocumentNumber { get; set; }

		public string Address { get; set; }

		public string MobileNumber { get; set; }

		public DateTime? CreatedFrom { get; set; }

		public DateTime? CreatedBefore { get; set; }

		protected override IQueryable<fnShopWalletClient> CreateQuery(IQueryable<fnShopWalletClient> objects, Func<IQueryable<fnShopWalletClient>, IOrderedQueryable<fnShopWalletClient>> orderBy = null)
		{
			if (Id.HasValue)
				objects = objects.Where(x => x.Id == Id.Value);
			if (!string.IsNullOrWhiteSpace(Email))
				objects = objects.Where(x => x.Email.Contains(Email));
			if (!string.IsNullOrWhiteSpace(UserName))
				objects = objects.Where(x => x.UserName.Contains(UserName));
			if (!string.IsNullOrWhiteSpace(CurrencyId))
				objects = objects.Where(x => x.CurrencyId == CurrencyId);
			if (Gender.HasValue)
				objects = objects.Where(x => x.Gender == Gender.Value);
			if (!string.IsNullOrEmpty(Info))
				objects = objects.Where(x => x.Info == Info);
			if (!string.IsNullOrWhiteSpace(FirstName))
				objects = objects.Where(x => x.FirstName.Contains(FirstName));
			if (!string.IsNullOrWhiteSpace(LastName))
				objects = objects.Where(x => x.LastName.Contains(LastName));
			if (!string.IsNullOrWhiteSpace(DocumentNumber))
				objects = objects.Where(x => x.DocumentNumber.Contains(DocumentNumber));
			if (!string.IsNullOrWhiteSpace(Address))
				objects = objects.Where(x => x.Address.Contains(Address));
			if (!string.IsNullOrWhiteSpace(MobileNumber))
				objects = objects.Where(x => x.MobileNumber.Contains(MobileNumber));
			if (CreatedFrom.HasValue)
				objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
			if (CreatedBefore.HasValue)
				objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);

			return base.FilteredObjects(objects, orderBy);
		}

		public IQueryable<fnShopWalletClient> FilterObjects(IQueryable<fnShopWalletClient> clients, Func<IQueryable<fnShopWalletClient>, IOrderedQueryable<fnShopWalletClient>> orderBy = null)
		{
			clients = CreateQuery(clients, orderBy);
			return clients;
		}

		public long SelectedObjectsCount(IQueryable<fnShopWalletClient> clients)
		{
			clients = CreateQuery(clients);
			return clients.Count();
		}
	}
}
