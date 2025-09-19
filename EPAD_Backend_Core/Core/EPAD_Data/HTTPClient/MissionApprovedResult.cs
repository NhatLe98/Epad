using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.HTTPClient
{
    public class MissionApprovedResult
    {
        public string ID { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }

        public DateTime Date { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string NhomPhuCap { get; set; }
        public string MissionFee { get; set; }
        public string Distance { get; set; }
        public string TransportVehicle { get; set; }
        public string Note { get; set; }
        public string Reason { get; set; }

        public string Place { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ContactInfo { get; set; }

        public DateTime SignedDate { get; set; }
        public string SignedPerson { get; set; }
    }

    public class MissionApprovedResultReponse
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public List<MissionApprovedResult> Data { get; set; }
    }
}
