using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addtableac : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IC_PrivilegeMachineRealtime",
                table: "IC_PrivilegeMachineRealtime");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "IC_PrivilegeMachineRealtime",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IC_PrivilegeMachineRealtime",
                table: "IC_PrivilegeMachineRealtime",
                column: "Index");

            migrationBuilder.CreateTable(
                name: "AC_AccGroup",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UID = table.Column<int>(type: "int", nullable: false),
                    Verify = table.Column<int>(type: "int", nullable: false),
                    ValidHoliday = table.Column<bool>(type: "bit", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_AccGroup", x => new { x.SerialNumber, x.UID });
                });

            migrationBuilder.CreateTable(
                name: "AC_AccHoliday",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_AccHoliday", x => new { x.SerialNumber, x.UID });
                });

            migrationBuilder.CreateTable(
                name: "AC_TimeZone",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UID = table.Column<int>(type: "int", nullable: false),
                    SunStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThurStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThursEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatEnd = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_TimeZone", x => new { x.SerialNumber, x.UID });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AC_AccGroup");

            migrationBuilder.DropTable(
                name: "AC_AccHoliday");

            migrationBuilder.DropTable(
                name: "AC_TimeZone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IC_PrivilegeMachineRealtime",
                table: "IC_PrivilegeMachineRealtime");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "IC_PrivilegeMachineRealtime",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IC_PrivilegeMachineRealtime",
                table: "IC_PrivilegeMachineRealtime",
                columns: new[] { "Index", "CompanyIndex", "UserName" });
        }
    }
}
