using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addcapcitycolumnsinIC_Device : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttendanceLogCapacity",
                table: "IC_Device",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerCapacity",
                table: "IC_Device",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserCapacity",
                table: "IC_Device",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceLogCapacity",
                table: "IC_Device");

            migrationBuilder.DropColumn(
                name: "FingerCapacity",
                table: "IC_Device");

            migrationBuilder.DropColumn(
                name: "UserCapacity",
                table: "IC_Device");
        }
    }
}
