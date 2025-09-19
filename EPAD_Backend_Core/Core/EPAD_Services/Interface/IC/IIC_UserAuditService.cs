using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_UserAuditService : IBaseServices<IC_UserAudit, EPAD_Context>
    {
        Task InsertAudit(List<string> employeeATIDs);
        Task DeleteAudit(List<string> employeeATIDs);
        Task InsertFaceAudit(List<string> employeeATIDs);
    }
}
