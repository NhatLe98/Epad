using EPAD_Common.Repository;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Repository.Interface
{
   public interface IIC_ServiceAndDevicesRepository : IBaseRepository<IC_ServiceAndDevices, EPAD_Context>
    {
    }
}
