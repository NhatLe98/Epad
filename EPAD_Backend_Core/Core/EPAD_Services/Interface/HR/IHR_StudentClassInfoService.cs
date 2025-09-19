using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_StudentClassInfoService : IBaseServices<HR_StudentClassInfo, EPAD_Context>
    {
        public Task<List<HR_StudentClassInfo>> CheckExistedOrCreate(List<HR_StudentClassInfo> listClassInfo);
    }
}
