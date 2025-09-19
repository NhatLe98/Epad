using System;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAD_Data
{
    public partial class ezHR_Context : DbContext
    {
        public ezHR_Context()
        {
        }

        public ezHR_Context(DbContextOptions<ezHR_Context> options)
            : base(options)
        {
        }

        public virtual DbSet<HR_Company> HR_Company { get; set; }
        public virtual DbSet<HR_Department> HR_Department { get; set; }
        public virtual DbSet<HR_Employee> HR_Employee { get; set; }
        public virtual DbSet<HR_WorkingInfo> HR_WorkingInfo { get; set; }
        public virtual DbSet<HR_EmployeeStoppedWorkingInfo> HR_EmployeeStoppedWorkingInfo { get; set; }
        public virtual DbSet<HR_EmployeeReport> HR_EmployeeReport { get; set; }
        public virtual DbSet<HR_Position> HR_Position { get; set; }
        public virtual DbSet<HR_Titles> HR_Titles { get; set; }
        public virtual DbSet<HR_Country> HR_Country { get; set; }
        //custom data
        public virtual DbSet<EmployeeFullInfo> EmployeeFullInfo { get; set; }
        public virtual DbSet<EM_NannyClassroom> EM_NannyClassroom { get; set; }
        public virtual DbSet<HR_EmployeeContactInfo> HR_EmployeeContactInfo { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<HR_Company>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("HR_Company");

                entity.Property(e => e.Index).ValueGeneratedNever();

                entity.Property(e => e.AccountNo).HasMaxLength(50);

                entity.Property(e => e.Address).HasMaxLength(500);

                entity.Property(e => e.BankInfo).HasMaxLength(200);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.EstablishedDate).HasColumnType("datetime");

                entity.Property(e => e.Fax).HasMaxLength(50);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnName("ID")
                    .HasMaxLength(200);

                entity.Property(e => e.Mail).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.Property(e => e.NameInEng).HasMaxLength(500);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.Phone1).HasMaxLength(50);

                entity.Property(e => e.Phone2).HasMaxLength(50);

                entity.Property(e => e.SecretKey).HasMaxLength(100);

                entity.Property(e => e.Slogan).HasMaxLength(200);

                entity.Property(e => e.TaxCode).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedUser).HasMaxLength(100);

                entity.Property(e => e.Website).HasMaxLength(200);
            });

            modelBuilder.Entity<HR_Department>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("HR_Department");

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.CompanyId)
                    .HasColumnName("CompanyID")
                    .HasMaxLength(200);

                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");

                entity.Property(e => e.ContactEmail).HasMaxLength(100);

                entity.Property(e => e.DateOfCreation).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Location).HasMaxLength(300);

                entity.Property(e => e.Name).HasMaxLength(400);

                entity.Property(e => e.NameInEng).HasMaxLength(400);

                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedUser).HasMaxLength(100);
            });

            modelBuilder.Entity<HR_Employee>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });

                entity.ToTable("HR_Employee");

                entity.Property(e => e.EmployeeATID)
                    .HasColumnName("EmployeeATID")
                    .HasMaxLength(30);

                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");

                entity.Property(e => e.CardNumber).HasMaxLength(50);

                entity.Property(e => e.DateOfNric)
                    .HasColumnName("DateOfNRIC")
                    .HasColumnType("datetime");

                entity.Property(e => e.EducationLevel).HasMaxLength(100);

                entity.Property(e => e.EmployeeCode).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.Image).HasColumnType("image");

                entity.Property(e => e.JoinedDate).HasColumnType("datetime");

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.MaritalDate).HasColumnType("smalldatetime");

                entity.Property(e => e.MidName).HasMaxLength(150);

                entity.Property(e => e.NativeCity).HasMaxLength(200);

                entity.Property(e => e.NativeCityCity)
                    .HasColumnName("NativeCity_City")
                    .HasMaxLength(100);

                entity.Property(e => e.NativeCityDistrict)
                    .HasColumnName("NativeCity_District")
                    .HasMaxLength(100);

                entity.Property(e => e.NativeCityDistrictIndex).HasColumnName("NativeCity_DistrictIndex");

                entity.Property(e => e.NativeCityNo)
                    .HasColumnName("NativeCity_No")
                    .HasMaxLength(100);

                entity.Property(e => e.NativeCityStreet)
                    .HasColumnName("NativeCity_Street")
                    .HasMaxLength(100);

                entity.Property(e => e.NativeCityWardIndex).HasColumnName("NativeCity_WardIndex");

                entity.Property(e => e.NativeCityWards)
                    .HasColumnName("NativeCity_Wards")
                    .HasMaxLength(100);

                entity.Property(e => e.NickName).HasMaxLength(200);

                entity.Property(e => e.Nric)
                    .HasColumnName("NRIC")
                    .HasMaxLength(50);

                entity.Property(e => e.PlaceOfBirth).HasMaxLength(200);

                entity.Property(e => e.PlaceOfNric)
                    .HasColumnName("PlaceOfNRIC")
                    .HasMaxLength(200);

                entity.Property(e => e.SocialInsNo).HasMaxLength(30);

                entity.Property(e => e.TaxNumber).HasMaxLength(50);

                entity.Property(e => e.UnionJoinedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedUser).HasMaxLength(100);

                entity.Property(e => e.WorkingDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<HR_WorkingInfo>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("HR_WorkingInfo");

                entity.Property(e => e.AttachmentFile).HasMaxLength(1000);

                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");

                entity.Property(e => e.DirectManager).HasMaxLength(30);

                entity.Property(e => e.EmployeeATID)
                    .IsRequired()
                    .HasColumnName("EmployeeATID")
                    .HasMaxLength(30);

                entity.Property(e => e.EmployeeCodeByWorking).HasMaxLength(50);

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.JobDescription).HasMaxLength(100);

                entity.Property(e => e.ManagedOtherDepartments).HasMaxLength(3000);

                entity.Property(e => e.Note).HasMaxLength(200);

                entity.Property(e => e.NumberOfDocument).HasMaxLength(50);

                entity.Property(e => e.SignedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SignedPerson).HasMaxLength(100);

                entity.Property(e => e.SynchErrorDevices)
                    .HasColumnType("ntext")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ToDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedUser).HasMaxLength(100);
            });
            modelBuilder.Entity<HR_EmployeeStoppedWorkingInfo>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("HR_EmployeeStoppedWorkingInfo");
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");
                entity.Property(e => e.EmployeeATID)
                   .IsRequired()
                   .HasColumnName("EmployeeATID")
                   .HasMaxLength(30);
                entity.Property(e => e.StartedDate).HasColumnType("datetime");
                entity.Property(e => e.ReturnedDate).HasColumnType("datetime");
                entity.Property(e => e.SignedDate).HasColumnType("smalldatetime");
                entity.Property(e => e.SignedPersion).HasMaxLength(100);
                entity.Property(e => e.SynchErrorDevices)
                    .HasColumnType("ntext")
                    .HasDefaultValueSql("('')");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedUser).HasMaxLength(100);
            });
            modelBuilder.Entity<HR_EmployeeReport>(entity =>
            {
                entity.HasKey(e => e.EmployeeATID);
            });

            modelBuilder.Entity<HR_Position>(entity =>
            {
                entity.HasKey(e => e.Index);
                entity.ToTable("HR_Position");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<HR_Titles>(entity =>
            {
                entity.HasKey(e => e.Index);
                entity.ToTable("HR_Titles");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");
            });
            modelBuilder.Entity<HR_Country>(entity =>
            {
                entity.HasKey(e => e.Index);
                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");
            });
            modelBuilder.Entity<EmployeeFullInfo>(entity =>
            {
                entity.HasKey(o => new { o.EmployeeATID, o.CompanyIndex });
            });

            modelBuilder.Entity<EM_NannyClassroom>(entity =>
            {
                entity.HasKey(o => o.Index);

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.ToDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.Class).HasMaxLength(100);

                entity.Property(e => e.UpdatedUser).HasMaxLength(100);

            });

            modelBuilder.Entity<HR_EmployeeContactInfo>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeATID, e.CompanyIndex });

                entity.Property(e => e.CompanyIndex).HasDefaultValueSql("((1))");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
