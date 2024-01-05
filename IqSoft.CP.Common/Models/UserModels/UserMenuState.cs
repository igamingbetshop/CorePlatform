using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.UserModels
{
    public class UserMenuState
    {
        [JsonProperty(PropertyName = "colId")]
        public string ColumnId { get; set; }

        [JsonProperty(PropertyName = "hide")]
        public bool Hide { get; set; }
    }
}
