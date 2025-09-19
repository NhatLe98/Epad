using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EPAD_Data.Entities
{
    public class TA_TimeAttendanceLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public DateTime Date { get; set; }
        public int? ShiftIndex { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public double TotalWorkingDay { get; set; } //Tổng công
        public double TotalWorkingTime { get; set; } //TỔNG GIỜ LÀM VIỆC
        public double TotalDayOff { get; set; } //CÔNG NGÀY NGHỈ
        public double TotalHoliday { get; set; } //CÔNG NGÀY LỄ
        public double TotalOverTime { get; set; } //CÔNG TĂNG CA
        public double TotalBusinessTrip { get; set; } //CÔNG TĂNG CA
        public double TotalWorkingTimeNormal { get; set; } //TỔNG GIỜ LÀM BÌNH THƯỜNG
        public double TotalOverTimeNormal { get; set; } //TĂNG CA NGÀY THƯỜNG
        public double TotalOverTimeNightNormal { get; set; } //TĂNG CA ĐÊM NGÀY THƯỜNG
        public double TotalOverTimeDayOff { get; set; } //TĂNG CA NGÀY NGHỈ
        public double TotalOverTimeNightDayOff { get; set; } //TĂNG CA ĐÊM NGÀY NGHỈ
        public double TotalOverTimeHoliday { get; set; } //TĂNG CA NGÀY LỄ
        public double TotalOverTimeNightHoliday { get; set; } //TĂNG CA ĐÊM NGÀY LỄ
        public double TotalWorkingTimeNight { get; set; } //Tổng giờ làm đêm
        public double CheckInLate { get; set; } //VÀO TRỄ (PHÚT)
        public double CheckOutEarly { get; set; } //RA SỚM (PHÚT)
    }
}
