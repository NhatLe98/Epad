using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_OperationLog
    {
        [Column(TypeName = "varchar(50)",Order =0)]
        public string SerialNumber { get; set; }

        public DateTime OpTime { get; set; }

        public int CompanyIndex { get; set; }

        public short OpCode { get; set; }

        [StringLength(10)]
        public string AdminID { get; set; }

        public short? Reserve1 { get; set; }

        public short? Reserve2 { get; set; }

        public short? Reserve3 { get; set; }

        public short? Reserve4 { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}