using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_PrivilegeDetailParamDTO
    {
        public string Menu { get; set; }
        public List<Role> Roles { get; set; }

        public IC_PrivilegeDetailParamDTO()
        {
            Roles = new List<Role>();
        }
    }
    public class Role
    {
        public int PrivilegeId { get; set; }
        public string State { get; set; }
    }
}
