using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class VehicleLog
    {
        public int Stt { get; set; }
        public int Id { get; set; }
        public string MaThe { get; set; }
        public int SoThe { get; set; }
        public string BienSo { get; set; }
        public int LoaiVeId { get; set; }
        public string ChuXe { get; set; }
        public int VeThangId { get; set; }
        public string DanhMucVeThang { get; set; }
        public string TrangThai { get; set; }
        public DateTime ThoiGianVao { get; set; }
        public DateTime? ThoiGianRa { get; set; }
        public int GiaVe { get; set; }
        public int NhanVienIdVao { get; set; }
        public int NhanVienIdRa { get; set; }
        public string TenNhanVienVao { get; set; }
        public string TenNhanVienRa { get; set; }
        public string MayTinhVao { get; set; }
        public string MayTinhRa { get; set; }
    }

    public class VehicleLogLogResult
    {
        public int status { get; set; }
        public List<VehicleLog> data { get; set; }
    }
}
