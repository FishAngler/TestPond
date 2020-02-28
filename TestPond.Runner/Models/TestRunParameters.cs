namespace TestPond.Runner.Models
{
    public class TestRunParameters
    {
        public string NUnitWhere { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIPAddress { get; set; }
        public int DeviceIndex { get; set; }
        public string DropDirectory { get; set; }
        public string AppPackageName { get; set; }
        public string UITestDllName { get; set; }
        public string TestResultDirectory { get; set; }
    }
}
