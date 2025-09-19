using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations.Sync_
{
    public partial class IC_Employee_Shift_Integrate_add_new : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_Employee_Shift_Integrate",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ShiftFromTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    ShiftToTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    ShiftApplyDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Employee_Shift_Integrate", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_Employee_Shift_Integrate");
        }
    }
}
