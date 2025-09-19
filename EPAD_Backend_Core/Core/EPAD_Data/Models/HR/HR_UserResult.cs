using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class HR_UserResult : HR_User, IHR_User
    {
        public new string Avatar { get; set; }
        public string CardNumber { get; set; }
        public string NameOnMachine { get; set; }
        public long? DepartmentIndexEz { get; set; }
        public bool? IsExpired { get; set; }
        public bool IsInBlackList { get; set; }
        public long? DepartmentIndex { get; set; }
        public string Department { get; set; }
        public bool IsExtraDriver { get; set; }
        public List<HR_UserResult> ListLogDriver { get; set; }
        public List<HR_ParentInfoResult> HR_ParentInfoResult { get; set; }
    }
}
