using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TruckDriverInfoModel : IC_PlanDock
    {
        public string CardNumber { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string TimeInString { get; set; }
        public string TimeOutString { get; set; }
        public string CompanyName { get; set; }
        public string StatusDockName { get; set; }
        public bool IsActive { get; set; }
        public bool IsRegisDri { get; set; }
        public List<ExtraTruckDriverLogModel> ExtraDriver { get; set; }
    }

    public class TruckDriverLogModel : GC_TruckDriverLog
    { 
        public string InOutModeString { get; set; }
        public string TimeString { get; set; }
        public List<ExtraTruckDriverLogModel> ExtraDriver { get; set; }
    }

    public class ExtraTruckDriverLogModel : GC_TruckExtraDriverLog
    { 
        public string BirthDayString { get; set; }
        public long CardUserIndex { get; set; }
    }

    public class ReturnDriverCardModel
    { 
        public string TripCode { get; set; }
        public string CardNumber { get; set; }
        public string Description { get; set; }
        public string SerialNumber { get; set; }
    }
}
