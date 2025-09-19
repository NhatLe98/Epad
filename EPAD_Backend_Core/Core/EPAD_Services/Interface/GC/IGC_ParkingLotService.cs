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
    public interface IGC_ParkingLotService : IBaseServices<GC_ParkingLot, EPAD_Context>
    {
        Task<bool> AddParkingLot(GC_ParkingLot param, UserInfo user);
        Task<bool> UpdateParkingLot(GC_ParkingLot param, UserInfo user);
        Task<bool> DeleteParkingLots(List<int> indexes, UserInfo user);
        Task<DataGridClass> GetDataByPage(int companyIndex, int page, string filter, int pageSize);
        Task<GC_ParkingLot> GetDataByIndex(int index);
        Task<List<GC_ParkingLot>> GetDataByCompanyIndex(int companyIndex);
        Task<GC_ParkingLot> GetDataByNameAndCompanyIndex(string name, int companyIndex);
        Task<GC_ParkingLot> GetDataByCodeAndCompanyIndex(string code, int companyIndex);
        Task<string> TryDeleteParkingLot(List<int> pParkingLotIndexs, int pCompanyIndex);
    }
}
