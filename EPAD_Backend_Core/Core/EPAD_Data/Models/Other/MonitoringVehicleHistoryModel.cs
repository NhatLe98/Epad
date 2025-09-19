using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class MonitoringVehicleHistoryModel
    {
        public string VehiclePlate { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        //public int GateIndexIn { get; set; }
        public string GateNameIn { get; set; }
        public string GateNameOut { get; set; }
        public string EmployeeATIDIn { get; set; }
        public string EmployeeATIDOut { get; set; }
        public string EmployeeCodeIn { get; set; }
        public string EmployeeCodeOut { get; set; }
        public string EmployeeNameIn { get; set; }
        public string EmployeeNameOut { get; set; }
        //public int DepartmentIndexIn { get; set; }
        public string DepartmentNameIn { get; set; }
        public string DepartmentNameOut { get; set; }
        public string ImagePlateIn { get; set; }
        public string ImagePlateOut { get; set; }
        public string ImageFaceIn { get; set; }
        public string ImageFaceOut { get; set; }
        public string UpdatedUserIn { get; set; }
        public string UpdatedUserOut { get; set; }
        public string NoteIn { get; set; }
        public string NoteOut { get; set; }

        public string CustomerName { get; set; }
        public string CustomerNRIC { get; set; }
        public string ViolationRule { get; set; }
    }
}
