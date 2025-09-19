using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_StudentClassInfo
    {
        [StringLength(100)]
        public string ClassInfoIndex { get; set; }
        [StringLength(100)]
        public string TeacherID { get; set; }
        [StringLength(100)]
        public string NannyID { get; set; }
        [StringLength(100)]
        public string EmployeeATID { get; set; }

        public int CompanyIndex { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedUser { get; set; }
    }
}
