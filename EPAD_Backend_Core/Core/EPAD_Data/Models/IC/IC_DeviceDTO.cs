using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_DeviceDTO : IC_Device
    {
    }

    public class IC_DeviceModel : IC_Device
    { 
        public string Status { get; set; }
        public string Name { get; set; }
    }
}
