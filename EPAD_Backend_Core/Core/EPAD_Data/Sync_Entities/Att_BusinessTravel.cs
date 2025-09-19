using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Att_BusinessTravel
    {
        public  string CodeAttendance { get; set; }
        public  string CodeEmp { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string DurationType { get; set; }
        public string BusinessTravelName { get; set; }
        public string PlaceFrom { get; set; }
        public string PlaceTo { get; set; }
        public int BusinessTripType { get; set; }
    }

    public class Att_BusinessTravelApiResult
    {
        public bool success { get; set; }
        public List<Att_BusinessTravel> data { get; set; }
        public string Message { get; set; }
    }
}
