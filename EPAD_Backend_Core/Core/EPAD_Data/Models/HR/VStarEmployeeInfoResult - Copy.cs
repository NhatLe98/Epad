using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class TeacherInfomationReponseEMS
    {
        public string employeeATID { get; set; }
        public string fullName { get; set; }
        public string schoolYearId { get; set; }
        public string schoolYearName { get; set; }
        public string semesterPlanId { get; set; }
        public string semesterPlanName { get; set; }
        public string mainClassId { get; set; }
        public string mainClassName { get; set; }
        public List<BaseLevel> baseLevelForTeacher { get; set; }

    }

    public class BaseLevel
    {
        public string baseLevelId { get; set; }
        public string baseLevelName { get; set; }
    }
}
