using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_PlanDockIntegrate
    {
        public long Index { get; set; }
        public string TripId { get; set; } // ma chuyen
        public string Vc { get; set; } // xe vang lai hay khong
        public string TrailerNumber { get; set; } // bien so xe
        public string DriverName { get; set; } // ten tai xe
        public string DriverPhone { get; set; } // sdt tai xe
        public string DriverCode { get; set; } // cmnd/cccd tai xe
        public DateTime? Eta { get; set; }
        public string LocationFrom { get; set; } // diem nhan hang
        public string Status { get; set; }
        public string OrderCode { get; set; } // ma don hang
        public DateTime? TimesDock { get; set; } // thoi gian vao dock
        public string StatusDock { get; set; } // trang thai xe
        public string Type { get; set; }
        public string Supplier { get; set; }
        public string Operation { get; set; }
    }
}
