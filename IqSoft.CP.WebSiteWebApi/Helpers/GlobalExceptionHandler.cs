using System.Net;
using System.Net.Http;
using System.Text;
/*using System.Web.Http;
using System.Web.Http.Results;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json;

namespace IqSoft.CP.WebSiteWebApi.Helpers
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
		public override void Handle(ExceptionHandlerContext context)
		{
			var response = new ApiResponseBase { Description = context.Exception.Message, ResponseCode = Constants.Errors.GeneralException };
			context.Result =
				new ResponseMessageResult(new HttpResponseMessage
				{
					Content = new StringContent(JsonConvert.SerializeObject(response),
						Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson),
					StatusCode = HttpStatusCode.OK
				});
		}

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }
    }
}*/