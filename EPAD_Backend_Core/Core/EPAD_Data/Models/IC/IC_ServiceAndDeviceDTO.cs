namespace EPAD_Data.Models
{
    public class IC_ServiceAndDeviceDTO : Entities.IC_ServiceAndDevices
    {
        public string ServiceType { get; set; }
        public string AliasName { get; set; }
        public string IPAddress { get; set; }
    }
}
