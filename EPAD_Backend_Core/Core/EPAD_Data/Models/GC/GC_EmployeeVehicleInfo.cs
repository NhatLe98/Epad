using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class GC_EmployeeVehicleInfo
    {
        public string EmployeeATID { get; set; }
        public string Plate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Branch { get; set; }
        public string Note { get; set; }
        public int CategoryId { get; set; }
    }

    public class GC_EmployeeVehicleInfoDetail : GC_EmployeeVehicleInfo
    {
        public string EmployeeName { get; set; }
        public int VehicleTypeId { get; set; }
        public int Price { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class IC_RegisterCardIntegrate
    {
        public string MaThe { get; set; }
        public string BienSo { get; set; }
        public string ChuXe { get; set; }
        public string NgayKichHoat { get; set; }
        public string NgayHetHan { get; set; }
        public int DanhMucId { get; set; }
        public int LoaiVeId { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string NhanHieu { get; set; }
        public string GhiChu { get; set; }
        public string Email { get; set; }
        public int EnrollNumber { get; set; }
    }
}
