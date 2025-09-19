using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace EPAD_Data
{
    public partial class EPAD_Context : DbContext
    {
        public EPAD_Context(DbContextOptions<EPAD_Context> options) : base(options)
        {
        }
        public virtual async Task<int> SaveChangesAsync(string username = "EPAD", int companyIndex = 2)
        {
            // OnBeforeSaveChanges(username, companyIndex);
            var result = await base.SaveChangesAsync();
            return result;
        }
        public virtual void SaveChangesWithAudit(string username, int companyIndex)
        {
            // OnBeforeSaveChanges(username, companyIndex);
            var result = base.SaveChanges();
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API always has higher priority than the data annotations (attributes).

            modelBuilder.Entity<HR_User>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });
            modelBuilder.Entity<HR_UserContactInfo>(entity =>
            {
                entity.HasKey(e => new { e.Index, e.CompanyIndex });
            });
            modelBuilder.Entity<HR_UserType>(entity =>
            {
                entity.HasKey(e => new { e.Index, e.CompanyIndex });
            });


            modelBuilder.Entity<HR_EmployeeInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_CustomerInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_StudentInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_ParentInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_ContractorInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_NannyInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_TeacherInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_ClassInfo>(entity =>
            {
                entity.HasKey(e => new { e.Index, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_PositionInfo>(entity =>
            {
                entity.HasKey(e => new { e.Index, e.CompanyIndex });
            });

            modelBuilder.Entity<HR_StudentClassInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.TeacherID, e.ClassInfoIndex });
            });

            modelBuilder.Entity<HR_CardNumberInfo>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });

            modelBuilder.Entity<HR_CustomerCard>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<HR_GradeInfo>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });

            modelBuilder.Entity<HR_TeamInfo>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });

            modelBuilder.Entity<IC_AttendanceLogClassRoom>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.CheckTime, o.RoomId });

            modelBuilder.Entity<IC_AttendanceLog>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.CheckTime, o.SerialNumber });
            modelBuilder.Entity<IC_Command>();
            modelBuilder.Entity<IC_CommandSystemGroup>();
            modelBuilder.Entity<IC_Company>();
            modelBuilder.Entity<IC_EmployeeStopped>();

            modelBuilder.Entity<IC_Config>();
            modelBuilder.Entity<IC_Department>();
            modelBuilder.Entity<IC_DepartmemtAEONSync>();
            modelBuilder.Entity<IC_DepartmentAndDevice>().HasKey(o => new { o.DepartmentIndex, o.SerialNumber, o.CompanyIndex });
            modelBuilder.Entity<IC_Device>().HasKey(o => new { o.SerialNumber, o.CompanyIndex });
            modelBuilder.Entity<IC_EmployeeType>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });

            //modelBuilder.Entity<IC_Employee>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex });
            modelBuilder.Entity<IC_EmployeeTransfer>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.NewDepartment, o.FromTime });
            modelBuilder.Entity<IC_OperationLog>().HasKey(o => new { o.SerialNumber, o.CompanyIndex, o.OpTime });
            modelBuilder.Entity<IC_PrivilegeDetails>().HasKey(o => new { o.PrivilegeIndex, o.CompanyIndex, o.FormName });

            modelBuilder.Entity<IC_Service>();
            modelBuilder.Entity<IC_ServiceAndDevices>().HasKey(o => new { o.ServiceIndex, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_Dashboard>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<IC_SystemCommand>();
            modelBuilder.Entity<IC_UserAccount>().HasKey(o => new { o.UserName, o.CompanyIndex });

            modelBuilder.Entity<IC_UserFaceTemplate>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber, o.FaceIndex });
            modelBuilder.Entity<IC_UserFinger>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber, o.FingerIndex });
            modelBuilder.Entity<IC_UserInfo>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_UserPrivilege>();
            modelBuilder.Entity<IC_UserMaster>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex });
            modelBuilder.Entity<IC_PrivilegeMachineRealtime>().HasKey(o => new { o.Index });
            modelBuilder.Entity<IC_PrivilegeDeviceDetails>().HasKey(o => new { o.PrivilegeIndex, o.CompanyIndex, o.SerialNumber, o.Role });
            modelBuilder.Entity<IC_PrivilegeDepartment>().HasKey(o => new { o.PrivilegeIndex, o.CompanyIndex, o.DepartmentIndex, o.Role });
            modelBuilder.Entity<IC_GroupDevice>();
            modelBuilder.Entity<IC_GroupDeviceDetails>().HasKey(o => new { o.GroupDeviceIndex, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_ConfigByGroupMachine>();

            modelBuilder.Entity<IC_UserFaceTemplate_v2>().HasKey(o => new { o.EmployeeATID, o.CompanyIndex, o.SerialNumber });
            modelBuilder.Entity<IC_WorkingInfo>();
            modelBuilder.Entity<IC_AppLicense>();
            modelBuilder.Entity<IC_HardwareLicense>();
            modelBuilder.Entity<IC_AccessToken>();
            modelBuilder.Entity<IC_Controller>();
            modelBuilder.Entity<IC_UserNotification>();
            modelBuilder.Entity<IC_RelayController>();
            modelBuilder.Entity<IC_RelayControllerChannel>().HasKey(o => new { o.RelayControllerIndex, o.ChannelIndex, o.CompanyIndex, o.SignalType });
            modelBuilder.Entity<IC_Camera>();
            modelBuilder.Entity<IC_Audit>();
            modelBuilder.Entity<IC_Printer>();
            modelBuilder.Entity<IC_HistoryTrackingIntegrate>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<IC_MailHistory>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
                entity.Property(e => e.Status).HasDefaultValueSql("((0))");
                entity.Property(e => e.Times).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<HR_EmployeeReport>(entity =>
            {
                entity.HasKey(e => e.EmployeeATID);
            });
            modelBuilder.Entity<HR_ExcusedAbsent>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_ExcusedAbsentReason>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_ExcusedLateEntry>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_Rules_InOutTime>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_FloorLevel>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormActivity>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormRation>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormLeaveType>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormRoom>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormRegister>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormRegister_Activity>(entity =>
            {
                entity.HasKey(e => new { e.DormRegisterIndex, e.DormActivityIndex, e.DormAccessMode });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_DormRegister_Ration>(entity =>
            {
                entity.HasKey(e => new { e.DormRegisterIndex, e.DormRationIndex });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Ignore<HR_EmployeeReport>();
            modelBuilder.Entity<IC_AttendancelogIntegrate>();
            modelBuilder.Entity<IC_DeviceHistory>();
            modelBuilder.Entity<AC_TimeZone>(entity =>
            {
                entity.HasKey(e => e.UID);

            });
            modelBuilder.Entity<AC_AccGroup>(entity =>
            {
                entity.HasKey(e => e.UID);
            });
            modelBuilder.Entity<AC_AccHoliday>(entity =>
            {
                entity.HasKey(e => e.UID);
            });
            modelBuilder.Entity<AC_Door>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<AC_DoorAndDevice>(entity =>
            {
                entity.HasKey(e => new { e.DoorIndex, e.SerialNumber });
            });
            modelBuilder.Entity<AC_Area>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<AC_AreaAndDoor>(entity =>
            {
                entity.HasKey(e => new { e.AreaIndex, e.DoorIndex });
            });
            modelBuilder.Entity<AC_UserMaster>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<AC_AreaLimited>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<AC_AreaLimitedAndDoor>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_AreaGroup>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_AreaGroup_GroupDevice>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.AreaGroupIndex,
                    e.DeviceGroupIndex,
                    e.CompanyIndex
                });
            });
            modelBuilder.Entity<GC_Gates>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_Lines>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_Gates_Lines>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.GateIndex,
                    e.LineIndex,
                    e.CompanyIndex
                });
            });
            modelBuilder.Entity<GC_AccessedGroup>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_ParkingLot>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_ParkingLotDetail>(entity =>
            {
                entity.HasKey(e => e.Index);
            });
            modelBuilder.Entity<GC_Rules_General_AreaGroup>().HasKey(o => new { o.AreaGroupIndex, o.Rules_GeneralIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Rules_General>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.Index
                });
            });
            modelBuilder.Entity<GC_Rules_General_Log>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.Index
                });
            });
            modelBuilder.Entity<GC_Rules_Warning>();
            modelBuilder.Entity<GC_Rules_WarningGroup>();
            modelBuilder.Entity<GC_Rules_Warning_ControllerChannel>();
            modelBuilder.Entity<GC_Rules_Warning_EmailSchedule>(e =>
            {
                e.HasKey(p => p.Index);
                e.HasAlternateKey(o => new { o.Time, o.CompanyIndex, o.DayOfWeekIndex, o.RulesWarningIndex });
            });
            modelBuilder.Entity<GC_TimeLog>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.EmployeeATID,
                    e.CompanyIndex,
                    e.Time,
                    e.MachineSerial
                });
            });
            modelBuilder.Entity<GC_TimeLog_Image>();

            modelBuilder.Entity<GC_TruckDriverLog>(entity =>
            {
                entity.HasKey(e => e.Index);
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<GC_TruckExtraDriverLog>(entity =>
            {
                entity.HasKey(e => e.Index);
            });

            modelBuilder.Entity<GC_Rules_GeneralAccess>();
            modelBuilder.Entity<GC_Rules_ParkingLot>();
            modelBuilder.Entity<GC_Lines_CheckOutDevice>().HasKey(o => new { o.LineIndex, o.CompanyIndex, o.CheckOutDeviceSerial });
            modelBuilder.Entity<GC_Lines_CheckInDevice>().HasKey(o => new { o.LineIndex, o.CompanyIndex, o.CheckInDeviceSerial });
            modelBuilder.Entity<GC_Lines_CheckInRelayController>().HasKey(o => new { o.LineIndex, o.RelayControllerIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Lines_CheckOutRelayController>().HasKey(o => new { o.LineIndex, o.RelayControllerIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Lines_CheckInCamera>().HasKey(o => new { o.LineIndex, o.CameraIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Lines_CheckOutCamera>().HasKey(o => new { o.LineIndex, o.CameraIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Employee_AccessedGroup>(entity =>
            {
                entity.HasKey(e => e.Index);
            });

            modelBuilder.Entity<GC_Department_AccessedGroup>(entity =>
            {
                entity.HasKey(e => e.Index);
            });


            modelBuilder.Entity<GC_Rules_GeneralAccess_Gates>().HasKey(o => new { o.RulesGeneralIndex, o.GateIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Rules_General_AreaGroup>().HasKey(o => new { o.AreaGroupIndex, o.Rules_GeneralIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_Customer>();
            modelBuilder.Entity<GC_Customer>(entity =>
            {
                entity.Property(e => e.DataStorageTime).HasDefaultValue(18);
            });
            modelBuilder.Entity<GC_Rules_Customer>();
            modelBuilder.Entity<GC_Rules_Customer_Gates>().HasKey(o => new { o.RulesCustomerIndex, o.GateIndex, o.CompanyIndex });
            modelBuilder.Entity<GC_ParkingLotAccessed>();
            modelBuilder.Entity<GC_EmployeeVehicle>();
            modelBuilder.Entity<GC_CustomerVehicle>();
            modelBuilder.Entity<GC_BlackList>();
            modelBuilder.Entity<AC_AccessedGroup>();
            modelBuilder.Entity<AC_DepartmentAccessedGroup>();  
            modelBuilder.Entity<AC_StateLog>();
            modelBuilder.Entity<IC_RegisterCard>();
            modelBuilder.Entity<IC_VehicleLog>();
            modelBuilder.Entity<TA_Rules_Global>();
            
            modelBuilder.Entity<TA_Rules_Shift>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_Rules_Shift_InOut>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.Index
                });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_Shift>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_AnnualLeave>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_AjustAttendanceLog>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<TA_EmployeeShift>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<TA_ScheduleFixedByDepartment>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_ScheduleFixedByEmployee>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_LeaveRegistration>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_LeaveDateType>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_Holiday>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_BusinessRegistration>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<TA_TimeAttendanceLog>(entity =>
            {
                entity.HasKey(e => new { e.Index });
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((2))");
            });
            modelBuilder.Entity<HR_User_Note>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID });
            });
            modelBuilder.Entity<TA_AjustAttendanceLogHistory>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<TA_AjustTimeAttendanceLog>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<IC_PlanDock>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<IC_StatusDock>(entity =>
            {
                entity.HasKey(e => new { e.Key });
            });

            modelBuilder.Entity<IC_StatusDock>().HasData(
                new IC_StatusDock() { Key = "0001", Name = "Đăng tài" },
                new IC_StatusDock() { Key = "0002", Name = "Xe vào cổng" },
                new IC_StatusDock() { Key = "0003", Name = "Xe ra cổng" }
                );
            modelBuilder.Entity<IC_LocationOperator>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<HR_StudentsAccordingToTheRegimen>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<HR_Rules_InOutTimeDetail>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });   
            modelBuilder.Entity<HR_EmailDeclareGuest>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<IC_UserAudit>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            
            modelBuilder.Entity<TA_ListLocation>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<TA_LocationByDepartment>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
            modelBuilder.Entity<TA_LocationByEmployee>(entity =>
            {
                entity.HasKey(e => new { e.Index });
            });
        }

        //private void OnBeforeSaveChanges(string username, int companyIndex)
        //{
        //    ChangeTracker.DetectChanges();
        //    var auditEntries = new List<IC_AuditEntryDTO>();
        //    foreach (var entry in ChangeTracker.Entries())
        //    {
        //        if (entry.Entity is IC_Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
        //            continue;
        //        var auditEntry = new IC_AuditEntryDTO(entry);
        //        auditEntry.TableName = entry.Entity.GetType().Name;
        //        auditEntry.UserName = username;
        //        auditEntry.CompanyIndex = companyIndex;
        //        auditEntries.Add(auditEntry);
        //        foreach (var property in entry.Properties)
        //        {
        //            string propertyName = property.Metadata.Name;
        //            if (property.Metadata.IsPrimaryKey())
        //            {
        //                auditEntry.KeyValues[propertyName] = property.CurrentValue;
        //                continue;
        //            }
        //            switch (entry.State)
        //            {
        //                case EntityState.Added:
        //                    auditEntry.State = AuditType.Added;
        //                    auditEntry.NewValues[propertyName] = property.CurrentValue;
        //                    break;
        //                case EntityState.Deleted:
        //                    auditEntry.State = AuditType.Deleted;
        //                    auditEntry.OldValues[propertyName] = property.OriginalValue;
        //                    break;
        //                case EntityState.Modified:
        //                    if (property.IsModified)
        //                    {
        //                        auditEntry.ChangedColumns.Add(propertyName);
        //                        auditEntry.State = AuditType.Modified;
        //                        auditEntry.OldValues[propertyName] = property.OriginalValue;
        //                        auditEntry.NewValues[propertyName] = property.CurrentValue;
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    foreach (var auditEntry in auditEntries)
        //    {
        //        IC_Audit.Add(auditEntry.ToAudit());
        //    }
        //}
    }

}
