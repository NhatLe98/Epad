using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Sync_Entities
{
    public class IC_Department_Integrate_AEON
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("parentId")]
        public string ParentId { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("sapCode")]
        public string SAPCode { get; set; }
        [JsonProperty("positionCode")]
        public string PositionCode { get; set; }
        [JsonProperty("positionName")]
        public string PositionName { get; set; }
        [JsonProperty("jobGradeCaption")]
        public string JobGradeCaption { get; set; }
        [JsonProperty("jobGradeGrade")]
        public int JobGradeGrade { get; set; }
        [JsonProperty("isStore")]
        public bool IsStore { get; set; }
        [JsonProperty("isHr")]
        public bool IsHr { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("isCB")]
        public bool IsCB { get; set; }
        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }
        [JsonProperty("isFacility")]
        public bool IsFacility { get; set; }
        [JsonProperty("items")]
        public List<IC_Department_Integrate_AEON> Items { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("userDepartmentMappings")]
        public List<UserDepartmentMappings> UserDepartmentMappings { get; set; }
    }

    public class UserDepartmentMappings
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("userFullName")]
        public string UserFullName { get; set; }
        [JsonProperty("userSAPCode")]
        public string UserSAPCode { get; set; }
        [JsonProperty("userEmai")]
        public string UserEmai { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("isHeadCount")]
        public bool IsHeadCount { get; set; }
        [JsonProperty("departmentId")]
        public string DepartmentId { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
    }

    public class DepartmentDetailItem
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("data")]
        public List<IC_Department_Integrate_AEON> Data { get; set; }
    }

    public class DepartmentItem
    {
        [JsonProperty("errorCodes")]
        public List<string> ErrorCodes { get; set; }
        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
        [JsonProperty("object")]
        public DepartmentDetailItem Objects { get; set; }
    }

    public class IC_Department_Integrate_AEON_Dto
    {
        public int IsDb { get; set; }
        public int? IsDbParent { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Code { get; set; }
        public string SAPCode { get; set; }
        public string PositionCode { get; set; }
        public string PostitionName { get; set; }
        public string JobGradeCaption { get; set; }
        public int JobGradeGrade { get; set; }
        public bool IsStore { get; set; }
        public bool IsHr { get; set; }
        public bool IsCB { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsFacility { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public List<UserDepartmentMappings> UserDepartmentMappings { get; set; }
    }

    public class UserDepartmentMappingsDto
    {
        public string UserId { get; set; }
        public string DepartmentId { get; set; }
        public int DepartmentDbId { get; set; }
    }

    public class DepartmentFromJson
    {
        public List<IC_Department_Integrate_AEON_Dto> Departments { get; set; }
        public List<UserDepartmentMappingsDto> UserDepartmentMappings { get; set; }
    }

    public class AEONDepartmentMapping
    {
        public string DepartmentCode { get; set; }
        public string DepartmentFormatCode { get; set; }
    }
}
