using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_StatusDock
    {

        [Key]
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
