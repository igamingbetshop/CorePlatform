using IqSoft.CP.AdminWebApi.Models.CommonModels;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterBase : ApiRequestBase
    {
        private int _takeCount = 100;

        public int TakeCount
        {
            get { return _takeCount; }
            set
            {
                if (value == -1)
                    _takeCount = 0;
                else if (value <= 0 || value > 10000)
                    _takeCount = 100;
                else
                    _takeCount = value;
            }
        }

        public int SkipCount { get; set; }

        public bool? OrderBy { get; set; }

        public string FieldNameToOrderBy { get; set; }
    }
}