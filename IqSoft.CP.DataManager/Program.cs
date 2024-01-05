using log4net;
using System.ServiceProcess;

namespace IqSoft.CP.DataManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static ILog DbLogger { get; private set; }
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            DbLogger = LogManager.GetLogger("DbLogAppender");
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[]
            {
                new DataManagerService()
            };
            //(new DataManagerService()).MigrateDocuments(null);

            ServiceBase.Run(ServicesToRun);
        }
    }
}
