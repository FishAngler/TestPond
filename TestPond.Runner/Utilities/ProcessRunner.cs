using System;
using System.Diagnostics;
using System.Text;
namespace TestPond.Runner.Utilities
{
    public static class ProcessRunner
    {
        public static (int ExitCode, string Output) Run(string fileName, string arguments, Action<object, DataReceivedEventArgs> outputDataReceived = null)
        {
            StringBuilder outputBuilder = new StringBuilder();

            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                },
            };

            p.OutputDataReceived += (s, e) =>
            {
                if (outputDataReceived != null)
                {
                    outputDataReceived.Invoke(s, e);
                }
                outputBuilder.AppendLine(e.Data);
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();
            int exitCode = p.ExitCode;
            p.Close();
            p.Dispose();

            return (ExitCode: exitCode, Output: outputBuilder.ToString());
        }
    }
}
