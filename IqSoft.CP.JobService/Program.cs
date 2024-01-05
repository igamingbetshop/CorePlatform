using System.ServiceProcess;
using log4net;

namespace IqSoft.CP.WindowsServices.JobService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>        
        public static ILog DbLogger { get; private set; }

        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            DbLogger = LogManager.GetLogger("DbLogAppender");
            ServiceBase.Run(new ServiceBase[] { new JobService() });
        }
    }
}
