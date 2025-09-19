using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_DormRegister_Activity
    {
        public int DormRegisterIndex { get; set; }
        public int DormActivityIndex { get; set; }
        public int DormAccessMode { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(200)]
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyIndex { get; set; }
    }
}
