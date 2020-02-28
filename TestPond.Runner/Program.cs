using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using TestPond.Runner.Models;
using TestPond.Runner.Utilities;

namespace TestPond.Runner
{
    class Program
    {
        #region Argument Parameters
        [Option("-lp|--local-path", Description = "[Required] Local path to where the application package and ui test dll live.")]
        [Required]
        public string LocalPath { get; set; }

        [Option("-dp|--device-platform", Description = "[Required] The device platform to run: 'a' for Android or 'i' for iOS.")]
        [Required]
        public Platform DevicePlatform { get; set; }

        [Option("-apn|--app-package-name", Description = "[Required] The file name of the APK or the APP to run.")]
        [Required]
        public string AppPackageName { get; set; }

        [Option("-dlln|--dll-name", Description = "[Required] The file name of the UI Test dll to run.")]
        [Required]
        public string UITestDllName { get; set; }

        [Option("-nuw|--nunit-where", Description = "The Nunit where clause for test selection.")]
        public string NUnitWhere { get; set; }

        [Option("-di|--device-id", Description = "The Android Device ID to run the tests against. Required when running for Android.")]
        public string DeviceId { get; set; }

        [Option("-dn|--device-name", Description = "The iOS Device Name to run the tests against. Required when running for iOS.")]
        public string DeviceName { get; set; }

        [Option("-dix|--device-index", Description = "An arbitrary index that gets passed in to the UI Test project.")]
        public int DeviceIndex { get; set; }

        [Option("-dip|--device-ip-address", Description = "The iOS Device IP Address. If not passed in, it is retrieved using the Device Name.")]
        public string DeviceIpAddress { get; set; }

        [Option("-rdp|--result-dir-path", Description = "Specify the Result Directory Path for the for Test Artifacts like the TestResult.xml, screenshots, etc.")]
        public string ResultDirectoryPath { get; set; }
        #endregion Argument Parameters

        static IConfiguration AppSettingsConfig;

        internal static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        //The below line is needed, do not delete!
        private void OnExecute(CommandLineApplication app) => Run(app).GetAwaiter().GetResult();

        /// <summary>
        /// Entry Point of the Application!
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private async Task Run(CommandLineApplication app)
        {
            string currentDirectory = Environment.CurrentDirectory;
            Console.WriteLine($"Executing in: {currentDirectory}");

            AppSettingsConfig = new ConfigurationBuilder()
                            .SetBasePath(GetBasePath())
                            .AddJsonFile("appsettings.json", true, true)
                            .Build();

            ConfigureNLog(AppSettingsConfig);

            Logger.Debug("Executing Runner App");

            if (!CheckParameters())
                Environment.Exit((int)ExitCodes.InvalidArguments);

            var resultsDirectory = !string.IsNullOrEmpty(ResultDirectoryPath) ? ResultDirectoryPath
                : CreateResultsDirectory();

            var testRunParameters = await GetTestRunParametersAsync(resultsDirectory);

            int result = -1;
            try
            {
                result = NUnit3TestRunner.RunUITests(testRunParameters);
            }
            finally
            {
                Console.WriteLine($"Results: {resultsDirectory}"); //this should be the last thing printed!
            }

            Environment.Exit(result == 0 ? (int)ExitCodes.Success : (int)ExitCodes.UnknownError);
        }

        private bool CheckParameters()
        {
            if (DevicePlatform == Platform.Android && string.IsNullOrWhiteSpace(DeviceId))
            {
                DisplayErrorMessage("Device Id is required when running against Android");
                return false;
            }

            if (DevicePlatform == Platform.iOS && string.IsNullOrWhiteSpace(DeviceName) && string.IsNullOrWhiteSpace(DeviceIpAddress))
            {
                DisplayErrorMessage("Either a Device Name or Device IP Address is required when running against Android");
                return false;
            }

            //TODO: we should remove this and change the UITest to be zero based!
            if (DeviceIndex == 0)
                DeviceIndex = 1;
            return true;
        }

        private static string CreateResultsDirectory()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmssfff");

            var resultsDirectory = Path.Combine(Config.CurrentDirectoryPath, "Results", timestamp);
            Directory.CreateDirectory(resultsDirectory);

            return resultsDirectory;
        }

        private async Task<TestRunParameters> GetTestRunParametersAsync(string resultsDirectory)
        {
            TestRunParameters testRunParameters = new TestRunParameters
            {
                DeviceName = DeviceName ?? string.Empty,
                DeviceIndex = DeviceIndex,
                DropDirectory = LocalPath,
                TestResultDirectory = resultsDirectory,
                UITestDllName = UITestDllName,
                AppPackageName = AppPackageName,
                NUnitWhere = NUnitWhere
            };

            if (DevicePlatform == Platform.Android)
            {
                //For Android, we just take a DeviceID (we don't need to specify and IP Address
                testRunParameters.DeviceId = DeviceId;
            }
            else if (DevicePlatform == Platform.iOS)
            {
                //For IPhone we take a device name and resolve the ID and IP Address
                testRunParameters.DeviceId = iOSDeviceUtility.GetIPhoneIdByName(DeviceName);
                testRunParameters.DeviceIPAddress = await GetDeviceIPAddress(DeviceIpAddress, DeviceName);
            }

            return testRunParameters;
        }

        /// <summary>
        /// Grabs the IP Address from either the DeviceIPAddressParameter or by mapping the IP to the HostName.
        /// Returns an empty string if no IP is available. Throws an ArgumentException if the user tries to pass in a Device IP AND a DeviceName (ip address is mappeed automatically)
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetDeviceIPAddress(string deviceIpAddress, string deviceName)
        {
            return !string.IsNullOrWhiteSpace(deviceIpAddress) ? deviceIpAddress
                : await GetIPAddressByDeviceName(deviceName);
        }

        private static async Task<string> GetIPAddressByDeviceName(string deviceName)
        {
            IPHostEntry hostInfo = null;
            try
            {
                hostInfo = await Dns.GetHostEntryAsync($"{deviceName}.local");
            }
            catch(Exception ex)
            {
                Logger.Fatal(ex, $"Could not find an IP Address for Device Name: {deviceName} - Is the device connected?");
                throw;
            }

            List<IPAddress> ipVersion4List = hostInfo.AddressList.OfType<IPAddress>().ToList()
                .Where(x => x.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork)).ToList();

            var ipAddress = ipVersion4List.First().ToString();

            return ipAddress;
        }

        /// <summary>
        /// Used to display the error message to the user on the screen
        /// </summary>
        /// <param name="message"></param>
        private static void DisplayErrorMessage(string message)
        {
            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = origColor;
            Logger.Error(message);
        }

        private static void ConfigureNLog(IConfiguration appsettingsConfig)
        {
            var nLogCofigurationSection = appsettingsConfig.GetSection("NLog");
            NLog.LogManager.Configuration = new NLogLoggingConfiguration(nLogCofigurationSection);

            var currentNlogConfig = NLog.LogManager.Configuration;

            Logger.Trace("NLog Targets:");
            foreach (var target in currentNlogConfig.AllTargets)
            {
                Logger.Trace($"{target.Name}");
            }
        }

        private static string GetBasePath()
        {
            // When running a single file executable, appsettings.json will not exist in the "temporary directory"
            //created after the application was unzipped because the CSProj file excludes that file from being bundled
            // so we need to specify the base directory associated with the main exported file folder (not the temp dir)
#if DEBUG
            //Debug Directory 
            return Directory.GetCurrentDirectory();
#else
            //Root directory of the Executable
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
#endif
        }
    }
}
