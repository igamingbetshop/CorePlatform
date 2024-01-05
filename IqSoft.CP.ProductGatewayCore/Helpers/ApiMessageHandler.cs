using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IqSoft.NGGP.WebApplications.ProductGateway.Helpers
{
    //public class ApiMessageHandler : DelegatingHandler
    //{
    //    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    //        CancellationToken cancellationToken)
    //    {
    //        var content = request.Content;
    //        Program.StringContent = content.ReadAsStringAsync().Result;
    //        string postData = string.Format("Type {0} Body {1} Url {2}", content.Headers.ContentType, Program.StringContent, request.RequestUri);
    //        var method = request.RequestUri.AbsolutePath;
    //        var requestLog = Helpers.WriteRequestLog(postData, method);
    //        // Call the inner handler.
    //        var response = await base.SendAsync(request, cancellationToken);
    //        content = response.Content;
    //        postData = string.Format("Type {0} Body {1}", content.Headers.ContentType, content.ReadAsStringAsync().Result);
    //        Helpers.WriteResponseLog(postData, method, requestLog);
    //        return response;
    //    }
    //}
}