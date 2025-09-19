using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_Department_Integrate_OVN
    {
        public int ID { get; set; }
        public int OrgUnitID { get; set; }
        public int ParentNodeID { get; set; }
        public string NameEN { get; set; }
        public string Code { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
