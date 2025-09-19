using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class ParkingLotParams
    {

    }

    public class ParkingLotAccessedRequest
    {
        public List<short> accessType { get; set; }
        public List<int> parkingLotIndex { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public string filter { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }

    public class ParkingLotAccessedParam
    {
        public short AccessType { get; set; }
        public int ParkingLotIndex { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ParkingLotAccessedParams : GC_ParkingLotAccessed
    { 
        public List<string> EmployeeATIDs { get; set; }
        public string FullName { get; set; }
        public string ParkingLot { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public string ErrorMessage { get; set; }
        public long RowIndex { get; set; }
    }
}
