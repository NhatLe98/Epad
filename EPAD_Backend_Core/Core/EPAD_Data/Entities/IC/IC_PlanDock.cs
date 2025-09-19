using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_PlanDock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public byte[] Avatar { get; set; }
        public string TripId { get; set; } // ma chuyen
        public bool Vc { get; set; } // xe vang lai hay khong
        public string TrailerNumber { get; set; } // bien so xe
        public string DriverName { get; set; } // ten tai xe
        public string DriverPhone { get; set; } // sdt tai xe
        public string DriverCode { get; set; } // cmnd/cccd tai xe
        public DateTime? Eta { get; set; } // thoi gian lay hang
        public string LocationFrom { get; set; } // diem nhan hang
        public string Status { get; set; }
        public string OrderCode { get; set; } // ma don hang
        public DateTime? TimesDock { get; set; } // thoi gian vao dock
        public string StatusDock { get; set; } // trang thai xe
        public string Type { get; set; }
        public string Supplier { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public int CompanyIndex { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Operation { get; set; }
        public long? SupplierId { get; set; }
    }
}
