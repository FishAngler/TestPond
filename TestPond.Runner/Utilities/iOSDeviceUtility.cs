using System;
using System.Linq;
using TestPond.Runner.Models;

namespace TestPond.Runner.Utilities
{
    public class iOSDeviceUtility
    {
        public static string[] GetPhysicalIosPhones()
        {
            string[] allDevices = GetAllIosDevices();

            var physicalIPhones = allDevices.Where(x => x.ToLower().Contains("iphone")
            && !x.ToLower().Contains("simulator")).ToArray();

            return physicalIPhones;
        }

        protected static string[] GetAllIosDevices()
        {
            // In order to get the connected device, we need to execute the command
            // xcrun instruments -s devices and grab the output
            var (ExitCode, Output) = ProcessRunner.Run("xcrun", "instruments -s devices");

            if (string.IsNullOrEmpty(Output))
                throw new Exception("No iOS Device was found! No results from command line");

            // Split the output, one for each line
            var allDevices = Output.Split('\n');
            return allDevices;
        }

        private static MobileDevice GetMobileDeviceFromXcrunDeviceInfo(string xcrunDeviceInfo)
        {
            // String comes in the form of 
            // iPhone (9.3.3) [f8233a0aac771cbb24fce52ac2cc3960fc47f83e]
            // We only need what is inside the brackets []
            var index = xcrunDeviceInfo.IndexOf('[') + 1; //find the index of [ and exclude it
            var deviceId = xcrunDeviceInfo.Substring(index, xcrunDeviceInfo.Length - index - 1); // get rid of everything except the device id

            var endOfNameIndex = xcrunDeviceInfo.IndexOf(' ') + 1;
            var name = xcrunDeviceInfo.Substring(0, endOfNameIndex - 1);

            var mobileDevice = new MobileDevice(deviceId, name);

            return mobileDevice;
        }

        public static string GetIPhoneIdByName(string deviceName)
        {
            string[] physicalIPhones = GetPhysicalIosPhones();

            var targetDeviceInfo = physicalIPhones
                .First(x => x.StartsWith(deviceName, StringComparison.CurrentCultureIgnoreCase));

            var deviceId = GetMobileDeviceFromXcrunDeviceInfo(targetDeviceInfo).Id;

            return deviceId;
        }
    }
}
