namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterBase : ApiRequestBase
    {
        private int _takeCount = 10;

        public int TakeCount
        {
            get { return _takeCount; }
            set
            {
                if (value <= 0 || value > 200)
                    _takeCount = 10;
                else
                    _takeCount = value;
            }
        }

        public int SkipCount { get; set; }
    }
}