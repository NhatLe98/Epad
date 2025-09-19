using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class VehicleHistoryModel
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerImage { get; set; }
        public int GateIndex { get; set; }
        public string GateName { get; set; }
        public int LineIndex { get; set; }
        public string LineName { get; set; }
        public DateTime CheckTime { get; set; }
        public string DepartmentName { get; set; }
        public string InOutMode { get; set; }
        public string CardNumber { get; set; }
        public string Note { get; set; }
        public string Error { get; set; }
        public string VerifyMode { get; set; }
        public string PhoneNumber { get; set; }
        public string StatusLog { get; set; }
        public string VehicleType { get; set; }
        public string Plate { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }

        public string ComputerIn { get; set; }
        public string ComputerOut { get; set; }

        public string Reason { get; set; }

    }

    public class TruckHistoryModel : GC_TruckDriverLog
    {
        public bool Vc { get; set; } // xe vang lai hay khong
        public string TrailerNumber { get; set; } // bien so xe
        public string DriverName { get; set; } // ten tai xe
        public string DriverPhone { get; set; } // sdt tai xe
        public string DriverCode { get; set; } // cmnd/cccd tai xe
        public DateTime? Eta { get; set; } // thoi gian lay hang
        public string LocationFrom { get; set; } // diem nhan hang
        public string Status { get; set; }
        public string OrderCode { get; set; } // ma don hang
        public DateTime? TimesDock { get; set; } // thoi gian vao dock
        public string StatusDock { get; set; } // trang thai xe  
        public string Type { get; set; }
        public string Supplier { get; set; } // nha cung cap
        public string EtaString { get; set; }
        public string TimesDockString { get; set; }
        public string TimeString { get; set; }
        public string MachineName { get; set; }
        public string StatusDockName { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string TimeInString { get; set; }
        public string TimeOutString { get; set; }
        public string MachineSerialIn { get; set; }
        public string MachineSerialOut { get; set; }
        public string MachineNameIn { get; set; }
        public string MachineNameOut { get; set; }
        public List<GC_TruckExtraDriverLog> ExtraDriver { get; set; }
    }
}
