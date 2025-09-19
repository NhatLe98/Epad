using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    /// <summary>
    /// Khách
    /// </summary>
    public class HR_CustomerInfo
    {
        [Key]
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        /// <summary>
        /// Tự gen khi add
        /// </summary>
        [Key]
        [Required]
        public int CompanyIndex { get; set; }

        /// <summary>
        /// CMND / CCCD
        /// </summary>
        [StringLength(100)]
        public string NRIC { get; set; }

        /// <summary>
        /// Thuộc công ty...
        /// </summary>
        [StringLength(200)]
        public string Company { get; set; }

        /// <summary>
        /// Địa chỉ
        /// </summary>
        [StringLength(500)]
        public string Address { get; set; }

        /// <summary>
        ///  Là khách V.I.P
        /// </summary>
        public bool? IsVIP { get; set; }

        /// <summary>
        /// Ảnh chứng thực
        /// </summary>
        public byte[] IdentityImage { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        public bool IsAllowPhone { get; set; }
        /// <summary>
        /// Not required, EmployeeATID
        /// </summary>
        [Column(TypeName = "nvarchar(100)")]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Not required, DepartmentIndex
        /// </summary>
        [Column(TypeName = "nvarchar(100)")]
        public string ContactDepartment { get; set; }
        /// <summary>
        /// Mã đăng ký
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public string RegisterCode { get; set; }

        /// <summary>
        /// ID Khách, thường = EmployeeATID
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public string CustomerID { get; set; }

        /// <summary>
        /// Thời gian lưu dữ liệu
        /// </summary>
        public int? DataStorageTime { get; set; }

        /// <summary>
        /// Người liên hệ (Ex: 90023, 90010)
        /// </summary>
        public string ContactPersonATIDs { get; set; }

        public string AccompanyingPersonList { get; set; }

        public short NumberOfContactPerson { get; set; }

        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        public DateTime RegisterTime { get; set; }

        /// <summary>
        /// Hiệu lực từ thời gian
        /// </summary>
        public DateTime FromTime { get; set; }

        /// <summary>
        /// Hiệu lực đến thời gian
        /// </summary>
        public DateTime ToTime { get; set; }

        /// <summary>
        /// Thời gian mở rộng
        /// </summary>
        public DateTime? ExtensionTime { get; set; }

        /// <summary>
        /// Nội dung công việc
        /// </summary>
        public string WorkContent { get; set; }

        /// <summary>
        /// Loại phương tiện
        /// </summary>
        public short BikeType { get; set; }

        /// <summary>
        /// Model phương tiện
        /// </summary>
        [Column(TypeName = "nvarchar(500)")]
        public string BikeModel { get; set; }

        /// <summary>
        /// Biển số phương tiện
        /// </summary>
        [Column(TypeName = "nvarchar(50)")]
        public string BikePlate { get; set; }

        /// <summary>
        /// Mô tả phương tiện
        /// </summary>
        public string BikeDescription { get; set; }

        /// <summary>
        /// Ảnh mặt trước cmnd
        /// </summary>
        public string NRICFrontImage { get; set; }

        /// <summary>
        /// ảnh mặt sau cmnd
        /// </summary>
        public string NRICBackImage { get; set; }

        /// <summary>
        /// ảnh mặt trươc bằng lái xe
        /// </summary>
        public string LicensePlateFrontImage { get; set; }

        /// <summary>
        /// ảnh mặt sau bằng lái xe
        /// </summary>
        public string LicensePlateBackImage { get; set; }

        public bool? GoInSystem { get; set; }

        /// <summary>
        /// Quy định cho khách
        /// </summary>

        public int? RulesCustomerIndex { get; set; }

        /// <summary>
        /// Tự gen khi add / update
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Tự gen khi add / update
        /// </summary>
        [StringLength(50)]
        public string UpdatedUser { get; set; }

        public string WorkingContent { get; set; }
        public string Note { get; set; }
        public string StudentOfParent { get; set; }
        public DateTime? Timestamp { get; set; }
        [StringLength(50)]
        public string ContactPersonPhoneNumber { get; set; }
    }
}
