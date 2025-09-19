using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_Employee_Shift_add_date : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftApplyDate",
                table: "IC_Employee_Shift",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftFromTime",
                table: "IC_Employee_Shift",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftToTime",
                table: "IC_Employee_Shift",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedDate",
                table: "IC_Employee_Shift",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShiftApplyDate",
                table: "IC_Employee_Shift");

            migrationBuilder.DropColumn(
                name: "ShiftFromTime",
                table: "IC_Employee_Shift");

            migrationBuilder.DropColumn(
                name: "ShiftToTime",
                table: "IC_Employee_Shift");

            migrationBuilder.DropColumn(
                name: "ShippedDate",
                table: "IC_Employee_Shift");
        }
    }
}
