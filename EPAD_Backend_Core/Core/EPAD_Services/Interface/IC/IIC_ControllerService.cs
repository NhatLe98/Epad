using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models.TimeLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_ControllerService : IBaseServices<IC_Controller, EPAD_Context>
    {
        public Task<List<IC_Controller>> GetByFilter(string pFilter, int pCompanyIndex);
        Task<bool> SetOnAndAutoOffController(RelayControllerParam param);

        public Task<DataGridClass> GetDataGrid(string pFilter, int pCompanyIndex, int pPage, int limit);

        public Task<IC_Controller> GetByIndex(int pIndex);
    }
}
