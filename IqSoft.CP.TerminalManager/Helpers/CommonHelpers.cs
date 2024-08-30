using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace IqSoft.CP.TerminalManager.Helpers
{
    public static class CommonHelpers
    {
        public static PhysicalAddress? GetMacAddress()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
            }
            return null;
        }

        [SupportedOSPlatform("windows")]
        public static string GetMotherBoardID()
        {
            try
            {
                var mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                ManagementObjectCollection moc = mos.Get();
                var serial = string.Empty;

                foreach (ManagementObject mo in moc)
                {
                    serial = mo["SerialNumber"].ToString()?.Replace(" ", string.Empty);
                }
                Console.WriteLine("serial: '{0}'", serial);
                if (serial== null || string.IsNullOrEmpty(serial.Trim()))
                {
                    var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        serial = queryObj["ProcessorId"].ToString()?.Replace(" ", string.Empty); ;
                        Console.WriteLine("ProcessorId: '{0}'", queryObj["ProcessorId"]);
                    }
                }
                return serial;

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
