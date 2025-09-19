using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_EmployeeTreeDTO
    {
        public long? ID { get; set; }
        public string IDLocal { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string NRIC { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public string Gender { get; set; }
        public List<IC_EmployeeTreeDTO> ListChildrent { get; set; }
        public  List<long> listIds { get; set; }
        public bool? IsDriverDepartment { get; set; }
        public bool? IsContractorDepartment { get; set; }

        public IC_EmployeeTreeDTO()
        {
            ListChildrent = new List<IC_EmployeeTreeDTO>();
        }
    }
}
