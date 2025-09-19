using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EPAD_Data.Models;
using EPAD_Common.Types;

namespace EPAD_Services.Interface
{
    public interface IGC_ParkingLotAccessedService : IBaseServices<GC_ParkingLotAccessed, EPAD_Context>
    {
        Task<DataGridClass> GetByFilter(List<short> accessType, List<int> parkingLotIndex, DateTime fromDate, DateTime? toDate, string filter,
            int page, int pageSize);
        Task<GC_ParkingLotAccessed> GetByFilterAndCompanyIndex(short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex);
        Task<GC_ParkingLotAccessed> GetByFilterCustomerAndCompanyIndex(short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex);
        Task<bool> GetByFilterAndCompanyIndexExcludeThis(short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, long pIndex, int pCompanyIndex);
        Task<List<GC_ParkingLotAccessed>> GetByFilterEmployeeAndCompanyIndex(short pAccessType, int pParkingLotIndex,
            List<string> pEmployeeATIDs, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex);
        Task<GC_ParkingLotAccessed> GetDataByIndexInt64(long pIndex);
        Task<GC_ParkingLotAccessed> GetDataByIndex(long pIndex);
        GC_ParkingLotAccessed GetParkingLotAccessed(List<GC_ParkingLotAccessed> gC_ParkingLotAccessed, short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex);
        Task<bool> AddParkingLotAccessed(ParkingLotAccessedParams param, UserInfo user);
        Task<bool> UpdateParkingLotAccessed(ParkingLotAccessedParams param, UserInfo user);
        Task<bool> DeleteParkingLotAccessed(List<long> indexes);
        Task<List<ParkingLotAccessedParams>> ImportParkingLotAccessed(List<ParkingLotAccessedParams> param, UserInfo user);
    }
}
