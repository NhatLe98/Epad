using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_UserFaceV2MasterDTO
    {
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public int No { get; set; }
        public int Index { get; set; }
        public int Valid { get; set; }
        public int Duress { get; set; }
        public int Type { get; set; }
        public int MajorVer { get; set; }
        public int MinorVer { get; set; }
        public int Format { get; set; }

        public string TemplateBIODATA { get; set; }
        public int Size { get; set; }

        public string Content { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }
    }
}
