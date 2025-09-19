using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_AccessToken
    {
        [Key]
        public int Index { get; set; }
        public int CompanyIndex { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Scope { get; set; }

        [StringLength(200)]
        public string AccessToken { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ExpiredDate { get; set; }


        [StringLength(500)]
        public string Note { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
