using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_RegisterCardService : IBaseServices<GC_TimeLog, EPAD_Context>
    {
        Task RegisterMonthCard(List<IC_RegisterCard> lstCard);
        Task<List<IC_RegisterCard>> GetRegisterCardInfo();
        Task IntegrateAttendanceLog();
    }
}
