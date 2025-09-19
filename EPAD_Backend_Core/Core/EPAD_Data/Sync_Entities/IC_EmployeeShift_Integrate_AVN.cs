using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_EmployeeShift_Integrate_AVN : Att_Roster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public DateTime IntegrateDate { get; set; }
    }
}
