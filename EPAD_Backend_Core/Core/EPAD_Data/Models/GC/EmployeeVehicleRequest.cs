using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmployeeVehicleRequest
    {
        public List<string> EmployeeATIDs { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public short Type { get; set; }
        public string TypeName { get; set; }
        public short StatusType { get; set; }
        public string StatusTypeName { get; set; }
        public string Plate { get; set; }
        public string VehicleImage { get; set; }
        public string Branch { get; set; }
        public string RegistrationImage { get; set; }
        public string Color { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public long RowIndex { get; set; }
        public string ErrorMessage { get; set; }
        public string Filter { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<long> DepartmentIndexes { get; set; }
    }

    public class EmployeeVehicleImportData : EmployeeVehicleRequest
    {
        public string FullName { get; set; }
    }

    public class CustomerVehicleRequest
    {
        public List<string> EmployeeATIDs { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public short Type { get; set; }
        public string TypeName { get; set; }
        public short StatusType { get; set; }
        public string StatusTypeName { get; set; }
        public string Plate { get; set; }
        public string VehicleImage { get; set; }
        public string Branch { get; set; }
        public string RegistrationImage { get; set; }
        public string Color { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public long RowIndex { get; set; }
        public string ErrorMessage { get; set; }
        public string Filter { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<long> DepartmentIndexes { get; set; }
    }

    public class CustomerVehicleImportData : CustomerVehicleRequest
    {
        public string FullName { get; set; }
    }
}
