namespace EPAD_Backend_Core.Models
{
    public class DeviceForceRestartRequest
    {
        public string SerialNumber { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
    }
}
