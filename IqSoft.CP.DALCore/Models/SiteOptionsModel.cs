using Newtonsoft.Json;

namespace IqSoft.CP.DAL.Models
{

    public class SiteOptionsModel
    {
        [JsonProperty(PropertyName = "PartnerId")]
        public int PartnerId { get; set; }
        [JsonProperty(PropertyName = "WebApiUrl")]
        public string WebApiUrl { get; set; }
        [JsonProperty(PropertyName = "Languages")]
        public string[] Languages { get; set; }
        [JsonProperty(PropertyName = "DefaultLanguage")]
        public string DefaultLanguage { get; set; }
        [JsonProperty(PropertyName = "Language")]
        public string Language { get; set; }
        [JsonProperty(PropertyName = "DateFormat")]
        public string DateFormat { get; set; }
        [JsonProperty(PropertyName = "MessageDateFormat")]
        public string MessageDateFormat { get; set; }
        [JsonProperty(PropertyName = "MobileDateFormat")]
        public string MobileDateFormat { get; set; }
        [JsonProperty(PropertyName = "Currencies")]
        public string[] Currencies { get; set; }
        [JsonProperty(PropertyName = "DefaultCurrency")]
        public string DefaultCurrency { get; set; }
        [JsonProperty(PropertyName = "Domain")]
        public string Domain { get; set; }
        [JsonProperty(PropertyName = "HomePage")]
        public string HomePage { get; set; }
        [JsonProperty(PropertyName = "AllowedAge")]
        public int AllowedAge { get; set; }
        [JsonProperty(PropertyName = "AgreeTermsLink")]
        public string AgreeTermsLink { get; set; }
        [JsonProperty(PropertyName = "ShowLogo")]
        public string ShowLogo { get; set; }
        [JsonProperty(PropertyName = "Registration")]
        public string Registration { get; set; }
        [JsonProperty(PropertyName = "PasswordRecovery")]
        public string PasswordRecovery { get; set; }
        [JsonProperty(PropertyName = "Deposit")]
        public string Deposit { get; set; }
        [JsonProperty(PropertyName = "Withdraw")]
        public string Withdraw { get; set; }
        [JsonProperty(PropertyName = "ShowCurrency")]
        public string ShowCurrency { get; set; }
        [JsonProperty(PropertyName = "IsShowVerifyMobileEmailPopup")]
        public string IsShowVerifyMobileEmailPopup { get; set; }
        [JsonProperty(PropertyName = "IsRegisterCommonMessageShow")]
        public string IsRegisterCommonMessageShow { get; set; }
        [JsonProperty(PropertyName = "IsPasswordRecoveryModern")]
        public string IsPasswordRecoveryModern { get; set; }
        [JsonProperty(PropertyName = "IsMobileNumberSpace")]
        public string IsMobileNumberSpace { get; set; }
        [JsonProperty(PropertyName = "IsShowOverlay")]
        public string IsShowOverlay { get; set; }
        [JsonProperty(PropertyName = "MobileCode")]
        public string MobileCode { get; set; }
        [JsonProperty(PropertyName = "IsErrorHide")]
        public string IsErrorHide { get; set; }
        [JsonProperty(PropertyName = "DemoGames")]
        public string DemoGames { get; set; }
        [JsonProperty(PropertyName = "Country")]
        public Country Country { get; set; }
        [JsonProperty(PropertyName = "QuickRegisterType")]
        public string QuickRegisterType { get; set; }
        [JsonProperty(PropertyName = "SocialLinks")]
        public string[] SocialLinks { get; set; }
        [JsonProperty(PropertyName = "ReCaptchaKey")]
        public string ReCaptchaKey { get; set; }
        [JsonProperty(PropertyName = "MenuList")]
        public MenuType[] MenuList { get; set; }
        [JsonProperty(PropertyName = "Banners")]
        public Banner[] Banners { get; set; }
        [JsonProperty(PropertyName = "ProductCategories")]
        public ProductCategory[] ProductCategories { get; set; }

    }

    public class Country
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }
    }

    public class MenuType //????
    {
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "IsEnabled")]
        public string IsEnabled { get; set; }
        [JsonProperty(PropertyName = "Items")]
        public ItemType[] Items { get; set; }
    }

    public class ItemType
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "Left")]
        public string Left { get; set; }
        [JsonProperty(PropertyName = "Titles")]
        public string Titles { get; set; }//?????????????????
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "Order")]
        public int Order { get; set; }
        [JsonProperty(PropertyName = "GroupName")]
        public string GroupName { get; set; }
        [JsonProperty(PropertyName = "link")]
        public string[] Link { get; set; }
        [JsonProperty(PropertyName = "Href")]
        public string Href { get; set; }
        [JsonProperty(PropertyName = "IsNew")]
        public string IsNew { get; set; }
        [JsonProperty(PropertyName = "isReload")]
        public string IsReload { get; set; }
        [JsonProperty(PropertyName = "IsGrouped")]
        public string IsGrouped { get; set; }
        [JsonProperty(PropertyName = "PlayButton")]
        public string PlayButton { get; set; }
        [JsonProperty(PropertyName = "Items")]
        public ItemType Items { get; set; }
        [JsonProperty(PropertyName = "DirectPlayHref")]
        public string DirectPlayHref { get; set; }
    }

    public class Banner
    {
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "Link")]
        public string Link { get; set; }
        [JsonProperty(PropertyName = "LinkType")]
        public string LinkType { get; set; }
        [JsonProperty(PropertyName = "Images")]
        public ImageType[] Images { get; set; }
        [JsonProperty(PropertyName = "ColSize")]
        public int ColSize { get; set; }
    }

    public class ImageType
    {
        [JsonProperty(PropertyName = "Image")]
        public string Image { get; set; }
        [JsonProperty(PropertyName = "Titles")]
        public string Titles { get; set; } //????????????
        [JsonProperty(PropertyName = "Descriptions")]
        public string Descriptions { get; set; } //????????????
        [JsonProperty(PropertyName = "Link")]
        public string Link { get; set; }
    }

    public class ProductCategory
    {
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "Products")]
        public Product[] Products { get; set; }
    }

    public class ProductOptions
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "mobileUrl")]
        public string MobileUrl { get; set; }
        [JsonProperty(PropertyName = "openType")]
        public string OpenType { get; set; }
    }
}