using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class UpdateDeviceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                 name: "ConnectionCode",
                 table: "IC_Device",
                 maxLength: 50,
                 nullable: true);
            migrationBuilder.AddColumn<string>(
               name: "DeviceId",
               table: "IC_Device",
               maxLength: 50,
               nullable: true);
            migrationBuilder.AddColumn<string>(
               name: "DeviceModel",
               table: "IC_Device",
               maxLength: 50,
               nullable: true);
            migrationBuilder.AddColumn<int>(
               name: "DeviceStatus",
               table: "IC_Device",
               nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                 name: "ConnectionCode",
                 table: "IC_Device");
            migrationBuilder.DropColumn(
               name: "DeviceId",
               table: "IC_Device");
            migrationBuilder.DropColumn(
              name: "DeviceModel",
              table: "IC_Device");
            migrationBuilder.DropColumn(
               name: "DeviceStatus",
               table: "IC_Device");
        }
    }
}
