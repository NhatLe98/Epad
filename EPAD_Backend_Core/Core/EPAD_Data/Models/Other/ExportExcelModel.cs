using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class ExportExcelModel
    {
        public List<MonitoringVehicleHistoryModel> Data { get; set; }
        public string LogType { get; set; }
        public int Status { get; set; }
    }

    public class ExportExcelResponse
    {
        public byte[] Byte { get; set; }
        public string FileName { get; set; }
    }
}
