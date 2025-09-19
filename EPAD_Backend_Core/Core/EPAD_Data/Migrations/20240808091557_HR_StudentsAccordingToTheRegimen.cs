using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class HR_StudentsAccordingToTheRegimen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBoarding",
                table: "HR_Rules_InOutTime",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDayBoarding",
                table: "HR_Rules_InOutTime",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSession",
                table: "HR_Rules_InOutTime",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HR_Rules_InOutTimeDetail",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RulesIndex = table.Column<int>(type: "int", nullable: false),
                    TypeRules = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxEarlyCheckInMinute = table.Column<int>(type: "int", nullable: false),
                    MaxLateCheckInMinute = table.Column<int>(type: "int", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxEarlyCheckOutMinute = table.Column<int>(type: "int", nullable: false),
                    MaxLateCheckOutMinute = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_Rules_InOutTimeDetail", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_StudentsAccordingToTheRegimen",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeATID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeIndex = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_StudentsAccordingToTheRegimen", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_Rules_InOutTimeDetail");

            migrationBuilder.DropTable(
                name: "HR_StudentsAccordingToTheRegimen");

            migrationBuilder.DropColumn(
                name: "IsBoarding",
                table: "HR_Rules_InOutTime");

            migrationBuilder.DropColumn(
                name: "IsDayBoarding",
                table: "HR_Rules_InOutTime");

            migrationBuilder.DropColumn(
                name: "IsSession",
                table: "HR_Rules_InOutTime");
        }
    }
}
