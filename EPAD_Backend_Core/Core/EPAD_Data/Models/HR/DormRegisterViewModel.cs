using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class DormRegisterViewModel
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public DateTime FromDate { get; set; }
        public string FromDateString { get; set; }
        public DateTime ToDate { get; set; }
        public string ToDateString { get; set; }
        public DateTime RegisterDate { get; set; }
        public string RegisterDateString { get; set; }
        public bool StayInDorm { get; set; }
        public int DormRoomIndex { get; set; }
        public string DormRoomName { get; set; }
        public int DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string DormEmployeeCode  { get; set; }
        public int DormLeaveIndex { get; set; }
        public string DormLeaveName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public string ErrorMessage { get; set; }
        public List<DormRegisterRationViewModel> DormRegisterRation { get; set; }
        public string DormRegisterRationName { get; set; }
        public List<DormRegisterActivityViewModel> DormRegisterActivity { get; set; }
        public string DormRegisterActivityName { get; set; }
    }

    public class DormRegisterRationViewModel : HR_DormRegister_Ration
    { 
        public string DormRationName { get; set; }
    }

    public class DormRegisterActivityViewModel : HR_DormRegister_Activity
    {
        public string DormActivityName { get; set; }
    }

    public class DormRegisterRequest
    {
        public int page { get; set; }
        public int limit { get; set; }
        public string filter { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public List<int> departments { get; set; }
        public List<int> dormRooms { get; set; }
    }

    public enum DormAccessMode
    { 
        In = 1, Out = 2
    }
}
