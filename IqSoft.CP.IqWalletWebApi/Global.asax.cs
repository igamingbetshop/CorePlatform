using System.Web.Http;
using System.Web;
using log4net;
using log4net.Config;

namespace IqSoft.CP.IqWalletWebApi
{
    public class WebApiApplication : HttpApplication
    {
        public static ILog DbLogger { get; private set; }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            XmlConfigurator.Configure();
            DbLogger = LogManager.GetLogger("DbLogAppender");
        }
    }
}
