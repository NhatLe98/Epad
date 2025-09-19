using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class UserPrivilege
    {
        public string FormName { get; set; }
        public List<FormRole> Roles { get; set; }
    }
}