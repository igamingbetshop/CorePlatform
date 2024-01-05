using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
	public partial class Banner
	{
		[NotMapped]
		public string ImageSize { get; set; }
		public Banner Copy()
		{
			return new Banner
			{
				Id = Id,
				PartnerId = PartnerId,
				Type = Type,
				NickName = NickName,
				Head = Head,
				HeadTranslationId = HeadTranslationId,
				Body = Body,
				BodyTranslationId = BodyTranslationId,
				Link = Link,
				ShowDescription = ShowDescription,
				Order = Order,
				IsEnabled = IsEnabled,
				Image = Image,
				StartDate = StartDate,
				EndDate = EndDate,
                Visibility = Visibility,
				ButtonType = ButtonType
			};
		}
	};
}
