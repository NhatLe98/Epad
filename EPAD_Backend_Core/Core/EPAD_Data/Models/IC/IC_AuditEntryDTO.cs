using System;
using System.Collections.Generic;
using EPAD_Data;
using EPAD_Data.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using EPAD_Data.Entities;

namespace EPAD_Data.Models
{
    public class IC_AuditEntryDTO
    {
        public IC_AuditEntryDTO(EntityEntry entry)
        {
            Entry = entry;
        }
        public EntityEntry Entry { get; }
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        public string UserName { get; set; }
        public string TableName { get; set; }
        public DateTime DateTime { get; set; }
        public Dictionary<string, object> KeyValues { get;} = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get;} = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get;} = new Dictionary<string, object>();
        public string AffectedColumns {get;set;}
        public string KeyValuesString { get; set; }
        public string OldValuesString { get; set; }
        public string NewValuesString { get; set; }
        public string StateString { get; set; }
        public AuditType State { get; set; }
        public string Description { get; set; }
        public string DescriptionEn { get; set; }
        public string Name { get; set; }
        public AuditStatus? Status { get; set; }
        public string StatusString { get; set; }
        public List<string> ChangedColumns { get; } = new List<string>();
        public string PageName { get; set; }
        public int? NumSuccess { get; set; }
        public int? NumFailure { get; set; }
        public int? IC_SystemCommandIndex { get; set; }
        public virtual IC_SystemCommand IC_SystemCommand { get; set; }

        public IC_AuditEntryDTO(UserInfo user, string pageName,
            AuditType state, AuditStatus auditStatus, string description, string descriptionEn)
        {
            UserName = user.UserName;
            CompanyIndex = user.CompanyIndex;
            State = state;
            Name = user.FullName;
            DateTime = DateTime.Now;
            Status = auditStatus;
            Description = description;
            DescriptionEn = descriptionEn;
            PageName = pageName;
        }

        public void SetOldValuesString(object obj)
        {
            this.OldValuesString = JsonConvert.SerializeObject(obj);
        }

        public void SetNewValuesString(object obj)
        {
            NewValuesString = JsonConvert.SerializeObject(obj);
        }

        public IC_Audit ToAudit()
        {
            var audit = new IC_Audit

            {
                CompanyIndex = CompanyIndex,
                State = State.ToString(),
                TableName = TableName,
                UserName = UserName,
                DateTime = DateTime.Now,
                KeyValues = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns),
                Description = Description,
                DescriptionEn = DescriptionEn,
                Name = Name,
                Status = Status?.ToString(),
                PageName = PageName,
                NumSuccess = NumSuccess,
                NumFailure = NumFailure,
                IC_SystemCommandIndex = IC_SystemCommandIndex,
            };

            return audit;
        }
    }
}
