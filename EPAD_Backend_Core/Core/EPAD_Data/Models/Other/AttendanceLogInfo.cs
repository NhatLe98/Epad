using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class GetAttendanceLogInfo
    {
        public int page { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public string filter { get; set; }
        public List<string> employee { get; set; }
        public int limit { get; set; }
        public List<string> ListDevice { get; set; }
    }

    public class GetACAttendanceLogInfo
    {
        public int page { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public string filter { get; set; }
        public int limit { get; set; }
        public List<int> listArea { get; set; }
        public List<int> listDoor { get; set; }
        public List<long> departmentIds { get; set; }
        public int viewMode { get; set; }
        public List<int> viewOperation { get; set; }
    }

    public class AttendanceLogInfoResult
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public DateTime Time { get; set; }
        public string SerialNumber { get; set; }
        public string AliasName { get; set; }
        public string IPAddress { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string InOutMode { get; set; }
        public string VerifyMode { get; set; }

        public string FaceMask { get; set; }
        public string BodyTemperature{ get; set; }
        public bool IsOverBodyTemperature { get; set; }
        public int? DeviceNumber { get; set; }
        public string DeviceId { get; set; }
        public long PositionIndex { get; set; }
    }

    public class WorkingLogResult
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public int UserType { get; set; }
        public string UserTypeName { get; set; }
        public long DepartmentIndex { get; set; }
        public bool RootDepartment { get; set; }
        public string DepartmentName { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceName { get; set; }
        public List<int> GroupDeviceIndex { get; set; }
        public List<string> GroupDeviceName { get; set; }
        public List<int> AreaGroupIndex { get; set; }
        public List<string> AreaGroupName { get; set; }
        public int? VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public string Plate { get; set; }
        public int LineIndex { get; set; }
        public string LineName { get; set; }
        public List<int> GateIndex { get; set; }
        public List<string> GateName { get; set; }
        public DateTime Time { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ComputerIn { get; set; }
        public string ComputerOut { get; set; }
        public string InOutMode { get; set; }
        public string VerifyMode { get; set; }
        public string FaceMask { get; set; }
        public string BodyTemperature { get; set; }
        public bool IsOverBodyTemperature { get; set; }
        public string CardNumber { get; set; }
        public string TimeString { get; set; }
        public bool IsExtraDriver { get; set; }
        public bool IsIntegratedLog { get; set; }
    }

    public class AttendanceACLogInfoResult
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public DateTime Time { get; set; }
        public string SerialNumber { get; set; }
        public string AliasName { get; set; }
        public string IPAddress { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string InOutMode { get; set; }
        public string VerifyMode { get; set; }

        public string FaceMask { get; set; }
        public string BodyTemperature { get; set; }
        public bool IsOverBodyTemperature { get; set; }
        public int? DeviceNumber { get; set; }
        public string DeviceId { get; set; }
        public long PositionIndex { get; set; }
        public string AreaName { get; set; }
        public string DoorName { get; set; }
        public string TimeString { get; set; }
    }

    public class PostAttendanceLog
    {
        public List<LogInfo> ListAttendanceLog { get; set; }
        public string SerialNumber { get; set; }
        public int? DeviceNumber { get; set; }
    }

    public class LogInfo
    {
        public string UserId { get; set; }
        public DateTime Time { get; set; }
        public int InOutMode { get; set; }
        public int VerifiedMode { get; set; }
        public int FaceMask { get; set; }
        public double BodyTemperature { get; set; }
    }

    public class ClassHourAttendanceLog
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }
        public string UserId { get; set; }
        public string RoomId { get; set; }
    }
}
