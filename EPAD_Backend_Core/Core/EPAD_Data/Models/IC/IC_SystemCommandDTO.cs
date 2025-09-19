using EPAD_Data.Entities;
using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class IC_SystemCommandDTO
    {
        public int Index { get; set; }

        public string SerialNumber { get; set; }

        public string DeviceName { get; set; }

        public string CommandName { get; set; }

        public string Command { get; set; }

        public string Params { get; set; }
        public object ParamBody { get; set; }
        public string EmployeeATIDs { get; set; }

        public DateTime? RequestedTime { get; set; }

        public DateTime? ExcutedTime { get; set; }

        public bool Excuted { get; set; }
        public string SystemCommandStatus { get; set; }
        public string SDKFuntion { get; set; }
        public string Error { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public int CompanyIndex { get; set; }

        public int GroupIndex { get; set; }
        public int ExcutingServiceIndex { get; set; }

        public bool IsOverwriteData { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<IC_AuditEntryDTO> IC_Audits { get; set; }
        /// <summary>
        ///     Dùng IC_DeviceDTO thì hợp lý hơn nhưng 2 class như nhau + ko có automapper nên dùng thẳng IC_Device
        /// </summary>
        public virtual IC_Device IC_Device { get; set; }
        public string Status
        {
            get
            {
                if (this.IsActive == false)
                {
                    return "IsDeleted";
                }
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    return "Error";
                }
                if (Excuted)
                {
                    return "Completed";
                }
                else if (!Excuted && ExcutedTime != null)
                {
                    return "Processing";
                }
                else if (!Excuted && ExcutedTime == null)
                {
                    return "Unexecuted";
                }
                return "Error";
            }
        }
    }
}
