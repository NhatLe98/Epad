using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_LinesService : IBaseServices<GC_Lines, EPAD_Context>
    {
        Task<bool> AddLine(LinesParam param, UserInfo user);
        Task<bool> UpdateLine(LinesParam param, UserInfo user);
        Task<bool> DeleteLines(List<int> indexes, UserInfo user);
        Task<GC_Lines> GetLineBySerialNumber(string serialNumber);
        Task<DataGridClass> GetAllData(int companyIndex);
        Task<List<GC_Lines>> GetDataByCompanyIndex(int companyIndex);
        Task<GC_Lines> GetDataByIndex(int index);
        Task<GC_Lines> GetDataByNameAndCompanyIndex(string name, int companyIndex);
        Task<bool> CheckLineUsing(List<int> pLineIndexs, int pCompanyIndex);
        Task<string> TryDeleteLine(List<int> pLineIndexs, int pCompanyIndex);
    }
}
