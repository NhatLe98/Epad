using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_EmployeeShiftParam
    {
        public List<string> DepartmentIds { get; set; }
        public List<string> EmployeeATIDs { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }

    public class CM_EmployeeShiftModel
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public int? ShiftIndex { get; set; }
        public int CanteenIndex { get; set; }
        public string DateStr { get; set; }
        public DateTime Date { get; set; }
        public long DepartmentIndex { get; set; }
    }

    public class EmployeeShiftTableRequest
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string Employee { get; set; }
        public string Department { get; set; }
        public List<DailyShift> DailyShifts { get; set; }
        public string ErrorMessage { get; set; }

    }
    public class DailyShift
    {
        public DateTime Date { get; set; }
        public string ShiftValue { get; set; }
    }
    public class ImportShiftTableRequest<T>
    {
        public T[] Data { get; set; }
    }
}
