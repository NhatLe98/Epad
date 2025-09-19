using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_ConfigDTO
    {
        public int Index { get; set; }

        public int CompanyIndex { get; set; }

        public string TimePos { get; set; }

        public string EventType { get; set; }

        public int? PreviousDays { get; set; }

        public string ProceedAfterEvent { get; set; }

        public string Email { get; set; }

        public bool SendMailWhenError { get; set; }

        public bool AlwaysSend { get; set; }

        public string TitleEmailSuccess { get; set; }

        public string BodyEmailSuccess { get; set; }

        public string TitleEmailError { get; set; }

        public string BodyEmailError { get; set; }

        public string CustomField { get; set; }

        public IntegrateLogParam IntegrateLogParam { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public IC_ConfigDTO() {
            IntegrateLogParam = new IntegrateLogParam();
        }
    }
}
