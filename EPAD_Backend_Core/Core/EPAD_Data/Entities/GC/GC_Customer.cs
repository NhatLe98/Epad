using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EPAD_Data.Entities
{
    public class GC_Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(30)")]
        public string EmployeeATID { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string RegisterCode { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string CustomerID { get; set; }
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Required]
        public string CustomerName { get; set; }
        [Column(TypeName = "varchar(50)")]
        [Required]
        public string CustomerNRIC { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string CustomerPhone { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CustomerEmail { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string CustomerCompany { get; set; }
        public bool Gender { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string CustomerAddress { get; set; }
        public bool IsVip { get; set; }
        public int DataStorageTime { get; set; }


        public string ContactPersonATIDs { get; set; }
        public string AccompanyingPersonList { get; set; }
        public short NumberOfContactPerson { get; set; }
        public DateTime RegisterTime { get; set; }
        //[Column(TypeName = "varchar(30)")]
        //public string CardNumber { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public DateTime? ExtensionTime { get; set; }
        public string WorkContent { get; set; }

        public short? BikeType { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string BikeModel { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string BikePlate { get; set; }
        public string BikeDescription { get; set; }

        public string CustomerImage { get; set; }
        public string CustomerFaceImage { get; set; }
        public string NRICFrontImage { get; set; }
        public string NRICBackImage { get; set; }
        public string LicensePlateFrontImage { get; set; }
        public string LicensePlateBackImage { get; set; }

        public bool? GoInSystem { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string CardNumber { get; set; }


        [Required]
        public int RulesCustomerIndex { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
