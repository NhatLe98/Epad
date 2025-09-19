using EPAD_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EPAD_Data
{
    public partial class Sync_Context : DbContext
    {
        public Sync_Context()
        {
        }

        public Sync_Context(DbContextOptions<Sync_Context> options)
            : base(options)
        {
        }

        public DbSet<IC_Department_Integrate_OVN> IC_Department_Integrate_OVN { get; set; }
        public DbSet<IC_Employee_Integrate> IC_Employee_Integrate { get; set; }
        public DbSet<IC_Employee_Shift_Integrate> IC_Employee_Shift_Integrate { get; set; }
        public DbSet<IC_Department_Integrate_AVN> IC_Department_Integrate_AVN { get; set; }
        public DbSet<IC_Employee_Integrate_AVN> IC_Employee_Integrate_AVN { get; set; }
        public DbSet<IC_Position_Integrate_AVN> IC_Position_Integrate_AVN { get; set; }
        public DbSet<IC_Shift_Integrate_AVN> IC_Shift_Integrate_AVN { get; set; }
        public DbSet<IC_OverTimePlan_Integrate_AVN> IC_OverTimePlan_Integrate_AVN { get; set; }
        public DbSet<IC_EmployeeShift_Integrate_AVN> IC_EmployeeShift_Integrate_AVN { get; set; }
        public DbSet<IC_BussinessTravel_Integrate_AVN> IC_BussinessTravel_Integrate_AVN { get; set; }
        public DbSet<IC_DepartmentIntegrate> IC_DepartmentIntegrate { get; set; }
        public DbSet<IC_EmployeeIntegrate> IC_EmployeeIntegrate { get; set; }
        public DbSet<IC_CardNumberInfo> IC_CardNumberInfo { get; set; }
        public DbSet<IC_PositionIntegrate> IC_PositionIntegrate { get; set; }
        public DbSet<IC_TransferUserIntegrate> IC_TransferUserIntegrate { get; set; }
        public DbSet<IC_AttendanceLogIntegrate> IC_AttendanceLogIntegrate { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<IC_Employee_Integrate>(entity =>
            {
                entity.HasKey(e => e.Index)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_Employee_Integrate");

                entity.Property(e => e.StoppedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<IC_Employee_Shift_Integrate>(entity =>
            {
                entity.HasKey(e => e.Index)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_Employee_Shift_Integrate");

                entity.Property(e => e.ShiftDate).HasColumnType("datetime");

                entity.Property(e => e.ShiftFromTime).HasColumnType("datetime");

                entity.Property(e => e.ShiftToTime).HasColumnType("datetime");

                entity.Property(e => e.ShiftApplyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<IC_CardNumberInfo>(entity =>
            {
                entity.HasKey(e => e.Index)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_CardNumberInfo");

            });

            modelBuilder.Entity<IC_DepartmentIntegrate>(entity =>
                {
                    entity.HasKey(e => e.Code)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    entity.ToTable("IC_Department");

                });

            modelBuilder.Entity<IC_EmployeeIntegrate>(entity =>
            {
                entity.HasKey(e => e.EmployeeATID)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_Employee");

            });

            modelBuilder.Entity<IC_PositionIntegrate>(entity =>
            {
                entity.HasKey(e => e.Code)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_Position");

            });

            modelBuilder.Entity<IC_TransferUserIntegrate>(entity =>
            {
                entity.HasKey(e => e.Index)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_TransferUser");

            });

            modelBuilder.Entity<IC_AttendanceLogIntegrate>(entity =>
            {
                entity.HasKey(e => e.Index)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                entity.ToTable("IC_AttendanceLog");

            });
        }
    }
}
