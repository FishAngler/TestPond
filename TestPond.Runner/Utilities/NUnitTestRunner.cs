using System;
using System.IO;
using TestPond.Runner.Models;

namespace TestPond.Runner.Utilities
{
    public static class NUnit3TestRunner
    {
        internal static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static readonly string _nunit3Path = Path.Combine(Config.CurrentDirectoryPath, "NUnit3", "nunit3-console.exe");
        private static string AddTestParamIfValueIsAvailable(string name, string value) =>
            (!string.IsNullOrEmpty(value) ? $"--testparam {name}=\"{value}\" " : "");

        static string GetConsoleRunnerArguments(TestRunParameters parms)
        {
            string where = string.Empty;
            if (!string.IsNullOrWhiteSpace(parms.NUnitWhere))
                where = $"--where \"{parms.NUnitWhere}\" ";

            string nUnitArgs = $"";

#if DEBUG || OSX
            //TODO: if we make the process include the nunit executable, this is not needed
            nUnitArgs = $"{_nunit3Path} ";
#endif

            nUnitArgs += $"--work {parms.TestResultDirectory} " + where +
                AddTestParamIfValueIsAvailable("deviceId", parms.DeviceId) +
                AddTestParamIfValueIsAvailable("deviceName", parms.DeviceName) +
                AddTestParamIfValueIsAvailable("deviceIp", parms.DeviceIPAddress) +
                $"--testparam deviceIndex={parms.DeviceIndex} " +
                $"--testparam appFilePath={Path.Combine(parms.DropDirectory, parms.AppPackageName)} " +
                $"--testparam dropDir={parms.DropDirectory} " +
                $"--testparam testResultDirectory={parms.TestResultDirectory} " +
                $"{Path.Combine(parms.DropDirectory, parms.UITestDllName)} ";

            return nUnitArgs;
        }

        public static int RunUITests(TestRunParameters parms)
        {
            Logger.Debug("Starting NUnit process:...");
            string process = _nunit3Path;
#if DEBUG || OSX
            //TODO: should we make the process be mono + nunit executable?
            process = "mono";
#endif

            string args = GetConsoleRunnerArguments(parms);
            Logger.Debug($"{process} {args}");
            var (ExitCode, Output) = ProcessRunner.Run(process, args, (s, e) =>
            {
                Logger.Info(e.Data);
            });

            return ExitCode;
        }
    }
}
