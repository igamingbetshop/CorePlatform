using System;
using IqSoft.NGGP.Common;
using IqSoft.NGGP.DAL;
using IqSoft.NGGP.WebApplications.ProductGateway;

namespace IqSoft.NGGP.WebApplications.ProductGateway.Helpers
{
    public class Helpers
    {
        /*public static ExternalOperation WriteRequestLog(string requestData, string method)
        {
            try
            {
                using (var baseBl = Program.BlFactory.CreateBaseBll())
                {
                    var request = new ExternalOperation
                    {
                        Method = method,
                        Type = Constants.InternalOperationType.Request,
                        Source = Constants.InternalOperationSource.FromGameProvider,
                        Body = requestData
                    };
                    var log = baseBl.SaveExternalOperation(request);
                    return log;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        public static void WriteResponseLog(string responseData, string method, ExternalOperation requestLog)
        {
            try
            {
                using (var baseBl = Program.BlFactory.CreateBaseBll())
                {

                    var request = new ExternalOperation
                    {
                        Method = method,
                        Type = Constants.InternalOperationType.Response,
                        Source = Constants.InternalOperationSource.FromGameProvider,
                        Body = responseData
                    };
                    if (requestLog != null)
                        request.ParentId = requestLog.Id;
                    baseBl.SaveExternalOperation(request);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }*/
    }
}