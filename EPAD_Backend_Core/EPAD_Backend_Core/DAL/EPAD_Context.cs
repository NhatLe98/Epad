using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.DAL
{
    public class EPAD_Context : DbContext
    {
        public EPAD_Context(DbContextOptions<EPAD_Context> options) : base(options)
        {
        }
        public virtual async Task<int> SaveChangesAsync(string username = "EPAD", int companyIndex = 2)
        {
            OnBeforeSaveChanges(username, companyIndex);
            var result = await base.SaveChangesAsync();
            return result;
        }
        public virtual void SaveChangesWithAudit(string username, int companyIndex)
        {
            OnBeforeSaveChanges(username, companyIndex);
            var result = base.SaveChanges();
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IC_AttendanceLog>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.CheckTime, o.SerialNumber });
            modelBuilder.Entity<IC_Command>();
            modelBuilder.Entity<IC_CommandSystemGroup>();
            modelBuilder.Entity<IC_Company>();

            modelBuilder.Entity<IC_Config>();
            modelBuilder.Entity<IC_Department>();
            modelBuilder.Entity<IC_DepartmentAndDevice>().HasKey(o => new { o.DepartmentIndex, o.SerialNumber, o.CompanyIndex });
            modelBuilder.Entity<IC_Device>().HasKey(o => new { o.SerialNumber, o.CompanyIndex });

            modelBuilder.Entity<IC_Employee>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex });
            modelBuilder.Entity<IC_EmployeeTransfer>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.NewDepartment, o.FromTime });
            modelBuilder.Entity<IC_OperationLog>().HasKey(o => new { o.SerialNumber, o.CompanyIndex, o.OpTime });
            modelBuilder.Entity<IC_PrivilegeDetails>().HasKey(o => new { o.PrivilegeIndex, o.CompanyIndex, o.FormName });

            modelBuilder.Entity<IC_Service>();
            modelBuilder.Entity<IC_ServiceAndDevices>().HasKey(o => new { o.ServiceIndex, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_SystemCommand>();
            modelBuilder.Entity<IC_UserAccount>().HasKey(o => new { o.UserName, o.CompanyIndex });

            modelBuilder.Entity<IC_UserFaceTemplate>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber, o.FaceIndex });
            modelBuilder.Entity<IC_UserFinger>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber, o.FingerIndex });
            modelBuilder.Entity<IC_UserInfo>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_UserPrivilege>();
            modelBuilder.Entity<IC_UserMaster>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex });


            modelBuilder.Entity<IC_PrivilegeDeviceDetails>().HasKey(o => new { o.PrivilegeIndex, o.CompanyIndex, o.SerialNumber, o.Role });
            modelBuilder.Entity<IC_PrivilegeDepartment>().HasKey(o => new { o.PrivilegeIndex, o.CompanyIndex, o.DepartmentIndex, o.Role });
            modelBuilder.Entity<IC_GroupDevice>();
            modelBuilder.Entity<IC_GroupDeviceDetails>().HasKey(o => new { o.GroupDeviceIndex, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_ConfigByGroupMachine>();

            modelBuilder.Entity<IC_UserFaceTemplate_v2>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_WorkingInfo>().HasIndex(o => new { o.EmployeeATID ,o.CompanyIndex,o.DepartmentIndex});
            modelBuilder.Entity<IC_AppLicense>();
            modelBuilder.Entity<IC_HardwareLicense>();
            modelBuilder.Entity<IC_AccessToken>();
            modelBuilder.Entity<IC_Controller>();
            modelBuilder.Entity<IC_UserNotification>();
            modelBuilder.Entity<IC_RelayController>();
            modelBuilder.Entity<IC_RelayControllerChannel>().HasKey(o => new { o.RelayControllerIndex, o.ChannelIndex, o.CompanyIndex });
            modelBuilder.Entity<IC_Camera>();
            modelBuilder.Entity<IC_Audit>();


            modelBuilder.Entity<HR_EmployeeReport>(entity =>
            {
                entity.HasKey(e => e.EmployeeATID);
            });
        }

        private void OnBeforeSaveChanges(string username, int companyIndex)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<IC_AuditEntryDTO>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IC_Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new IC_AuditEntryDTO(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserName = username;
                auditEntry.CompanyIndex = companyIndex;
                auditEntries.Add(auditEntry);
                var keyID = "";
                foreach (var property in entry.Properties)
                {

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    if (GetKeyValue(propertyName) && string.IsNullOrWhiteSpace(keyID))
                    {
                        keyID = property.CurrentValue.ToString();
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.State = AuditType.Added;
                            //auditEntry.NewValues[propertyName] = property.CurrentValue;
                            auditEntry.Description = string.IsNullOrWhiteSpace(keyID) ? "" : string.Format("Added Employee: {0}", keyID);
                            break;
                        case EntityState.Deleted:
                            auditEntry.State = AuditType.Deleted;
                            //auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.Description = string.IsNullOrWhiteSpace(keyID) ? "" : string.Format("Deleted Employee: {0}", keyID);
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                //auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.State = AuditType.Modified;
                                //auditEntry.OldValues[propertyName] = property.OriginalValue;
                                //auditEntry.NewValues[propertyName] = property.CurrentValue;
                                auditEntry.Description = string.IsNullOrWhiteSpace(keyID) ? "" : string.Format("Modified Employee: {0}", keyID);
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                IC_Audit.Add(auditEntry.ToAudit());
            }
        }
        private bool GetKeyValue(string propertyName)
        {
            var result = false;
            switch (propertyName) {
                case "EmployeeATID":
                    result = true;
                    break;
                case "EmployeeCode":
                    result = true;
                    break;
                case "UserName":
                    result = true;
                    break;
            }
            return result;
        }

        public DbSet<IC_AttendanceLog> IC_AttendanceLog { get; set; }
        public DbSet<IC_Command> IC_Command { get; set; }
        public DbSet<IC_CommandSystemGroup> IC_CommandSystemGroup { get; set; }
        public DbSet<IC_Company> IC_Company { get; set; }

        public DbSet<IC_Config> IC_Config { get; set; }
        public DbSet<IC_Department> IC_Department { get; set; }
        public DbSet<IC_DepartmentAndDevice> IC_DepartmentAndDevice { get; set; }
        public DbSet<IC_Device> IC_Device { get; set; }

        public DbSet<IC_Employee> IC_Employee { get; set; }
        public DbSet<IC_EmployeeTransfer> IC_EmployeeTransfer { get; set; }
        public DbSet<IC_OperationLog> IC_OperationLog { get; set; }
       

        public DbSet<IC_Service> IC_Service { get; set; }
        public DbSet<IC_ServiceAndDevices> IC_ServiceAndDevices { get; set; }
        public DbSet<IC_SystemCommand> IC_SystemCommand { get; set; }
        public DbSet<IC_UserAccount> IC_UserAccount { get; set; }
        public DbSet<IC_UserMaster> IC_UserMaster { get; set; }
        //public DbSet<IC_UserFingerMaster> IC_UserFingerMaster { get; set; }
        //public DbSet<IC_UserFaceMaster> IC_UserFaceMaster { get; set; }
        //public DbSet<IC_UserFaceV2Master> IC_UserFaceV2Master { get; set; }
        public DbSet<IC_UserFaceTemplate> IC_UserFaceTemplate { get; set; }
        public DbSet<IC_UserFinger> IC_UserFinger { get; set; }
        public DbSet<IC_UserInfo> IC_UserInfo { get; set; }
        public DbSet<IC_UserPrivilege> IC_UserPrivilege { get; set; }
        public DbSet<IC_PrivilegeDetails> IC_PrivilegeDetails { get; set; }
        public DbSet<IC_PrivilegeDeviceDetails> IC_PrivilegeDeviceDetails { get; set; }
        public DbSet<IC_PrivilegeDepartment> IC_PrivilegeDepartment { get; set; }
        public DbSet<IC_GroupDevice> IC_GroupDevice { get; set; }
        public DbSet<IC_GroupDeviceDetails> IC_GroupDeviceDetails { get; set; }
        public DbSet<IC_ConfigByGroupMachine> IC_ConfigByGroupMachine { get; set; }

        public DbSet<IC_UserFaceTemplate_v2> IC_UserFaceTemplate_v2 { get; set; }
        public DbSet<IC_WorkingInfo> IC_WorkingInfo { get; set; }
        public DbSet<IC_AppLicense> IC_AppLicense { get; set; }
        public DbSet<IC_HardwareLicense> IC_HardwareLicense { get; set; }
        public DbSet<IC_AccessToken> IC_AccessToken { get; set; }
        public DbSet<IC_Controller> IC_Controller { get; set; }

        public DbSet<IC_RelayController> IC_RelayController { get; set; }
        public DbSet<IC_RelayControllerChannel> IC_RelayControllerChannel { get; set; }
        public DbSet<IC_Camera> IC_Camera { get; set; }
        public DbSet<IC_UserNotification> IC_UserNotification { get; set; }

        public virtual DbSet<HR_EmployeeReport> HR_EmployeeReport { get; set; }

        public DbSet<IC_Audit> IC_Audit { get; set; }

    }

}
