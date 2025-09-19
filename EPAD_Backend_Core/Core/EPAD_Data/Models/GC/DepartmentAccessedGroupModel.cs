using EPAD_Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Models
{
    public class DepartmentAccessedGroupModel
    {
        public int Index { get; set; }
        public string DepartmentName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromDateFormat { get; set; }
        public string ToDateFormat { get; set; }
        public int CompanyIndex { get; set; }
        public int? AccessedGroupIndex { get; set; }
        public string AccessedGroupName { get; set; }
        public long? DepartmentIndex { get; set; }
        public long? DepartmentIDs { get; set; }
        public string ErrorMessage { get; set; }

    }

}
