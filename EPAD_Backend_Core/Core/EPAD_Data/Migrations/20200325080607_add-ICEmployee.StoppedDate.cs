using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addICEmployeeStoppedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StoppedDate",
                table: "IC_Employee",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoppedDate",
                table: "IC_Employee");
        }
    }
}
