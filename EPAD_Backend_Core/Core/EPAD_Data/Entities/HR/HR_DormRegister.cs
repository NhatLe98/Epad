using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_DormRegister
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool StayInDorm { get; set; }
        public int DormRoomIndex { get; set; }
        public string DormEmployeeCode { get; set; }
        public int DormLeaveIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(200)]
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyIndex { get; set; }
    }
}
