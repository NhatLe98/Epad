using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_User_Note
    {
        [Key]
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }
        public string Area { get; set; }
        public string Department { get; set; }
        public string Note { get; set; }
    }
}
