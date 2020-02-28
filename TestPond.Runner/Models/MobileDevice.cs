namespace TestPond.Runner.Models
{
    public class MobileDevice
    {
        public string Id { get; internal set; }
        public string UniqueName { get; internal set; }
        public string ModelNumber { get; internal set; }

        public MobileDevice(string id, string uniqueName = "", string modelNumber = "")
        {
            Id = id;
            UniqueName = uniqueName;
            ModelNumber = modelNumber;
        }
    }
}
