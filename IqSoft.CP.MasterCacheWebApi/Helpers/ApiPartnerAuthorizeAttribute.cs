using System;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class ApiPartnerAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return true;
        }
    }
}