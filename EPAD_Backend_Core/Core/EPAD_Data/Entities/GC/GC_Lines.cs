using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Lines
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(200)]
        [Required]
        public string Name { get; set; }


        [StringLength(3000)]
        public string Description { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
        public bool LineForCustomer { get; set; }
        public bool LineForDriver { get; set; }
        public bool LineForCustomerIssuanceReturnCard { get; set; }
        public bool LineForDriverIssuanceReturnCard { get; set; }
    }
}
