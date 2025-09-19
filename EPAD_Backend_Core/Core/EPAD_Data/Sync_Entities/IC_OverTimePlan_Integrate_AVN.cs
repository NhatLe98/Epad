using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_OverTimePlan_Integrate_AVN : Att_OverTimePlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public DateTime IntegrateDate { get; set; }
    }
}
