using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CustomerCardModel : HR_CustomerCard
    {
        public bool Status { get; set; }
        public string TripCode { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string Object { get; set; }
        public string ObjectString { get; set; }
        public string CreatedDateString { get; set; }
        public string UpdatedDateString { get; set; }
        public DateTime? CardUpdatedDate { get; set; }
        public string CardUpdatedDateString { get; set; }
        public bool IsSyncToDevice { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsActive { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public long CardUserIndex { get; set; }
    }
}
