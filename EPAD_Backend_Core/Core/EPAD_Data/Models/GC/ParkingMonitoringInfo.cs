using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class ParkingMonitoringInfo : MonitoringInfo
    {
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DepartmentName { get; set; }
        public long DepartmentIndex { get; set; }
        public List<string> BikeModels { get; set; }
        public string BikePlateIn { get; set; }
        public string BikePlateOut { get; set; }
        public List<string> BikePlatesRegister { get; set; }
        public string CardNumber { get; set; }
        public string AccessObject { get; set; }
        public int NumberOfBikeInParking { get; set; }
        //Customer
        public string CompanyName { get; set; }
        public string ContactPersonName { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        //end Customer

        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }

        public string ImagePlateIn { get; set; }
        public string ImageFaceIn { get; set; }
        public string ImagePlateOut { get; set; }
        public string ImageFaceOut { get; set; }
        public bool IsRequiredEmployeeVehicle { get; set; }
        public ParkingMonitoringInfo()
        {
            BikePlatesRegister = new List<string>();
            BikeModels = new List<string>();
        }
    }
}
