using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addIC_EmployeeTransfer_and_IC_WorkingInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "IC_WorkingInfo",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedUser",
                table: "IC_WorkingInfo",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "IC_WorkingInfo",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "IC_EmployeeTransfer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedUser",
                table: "IC_EmployeeTransfer",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "IC_EmployeeTransfer",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "IC_WorkingInfo");

            migrationBuilder.DropColumn(
                name: "ApprovedUser",
                table: "IC_WorkingInfo");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IC_WorkingInfo");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "IC_EmployeeTransfer");

            migrationBuilder.DropColumn(
                name: "ApprovedUser",
                table: "IC_EmployeeTransfer");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IC_EmployeeTransfer");
        }
    }
}
