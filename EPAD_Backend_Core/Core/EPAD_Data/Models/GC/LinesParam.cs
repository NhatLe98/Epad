using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class LinesParam 
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool LineForCustomer { get; set; }
        public bool LineForCustomerIssuanceReturnCard { get; set; }
        public bool LineForDriver { get; set; }
        public bool LineForDriverIssuanceReturnCard { get; set; }
        public List<string> DeviceInSerial { get; set; }
        public List<string> DeviceOutSerial { get; set; }
        public List<IC_DeviceModel> DeviceIn { get; set; }
        public List<IC_DeviceModel> DeviceOut { get; set; }
        public List<int> CameraInIndex { get; set; }
        public List<int> CameraOutIndex { get; set; }
        public List<IC_Camera> CameraIn { get; set; }
        public List<IC_Camera> CameraOut { get; set; }
        public List<LineController> LineControllersIn { get; set; }
        public List<LineController> LineControllersOut { get; set; }
    }

    public class LineBasicInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }

    public class LineController
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Description { get; set; }
        public string RelayType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public int CompanyIndex { get; set; }
        public int SignalType { get; set; }
        public int? ControllerIndex { get; set; }
        public int? OpenChannel { get; set; }
        public int? CloseChannel { get; set; }
    }
}
