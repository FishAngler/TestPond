using System;
using System.IO;

namespace TestPond.Runner
{
    public static class Config
    {
        private static DirectoryInfo _currentDirectory;
        public static DirectoryInfo CurrentDirectory
        {
            get
            {
                if (_currentDirectory == null)
                {
                    //_currentDirectory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    // we can't use reflection to get the current directory, as self contained apps in .NET Core 3 use a temporary directory to extract all DLLs,
                    // Environment.CurrentDirectory seems to do the trick, but it could be set by the parent process, which could be affected when called by an
                    // external client like the Creator: https://github.com/dotnet/coreclr/issues/25623#issuecomment-523810880
                    _currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                }
                return _currentDirectory;
            }
        }

        public static string CurrentDirectoryPath => CurrentDirectory.FullName;

        public static string CreateRootFilepath(string childPath)
        {
            return Path.Combine(CurrentDirectoryPath, childPath);
        }
    }
}
