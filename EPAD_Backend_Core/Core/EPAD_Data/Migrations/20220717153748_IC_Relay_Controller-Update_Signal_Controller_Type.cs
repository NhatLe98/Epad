using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_Relay_ControllerUpdate_Signal_Controller_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ControllerType",
                table: "IC_RelayController",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SignalType",
                table: "IC_RelayController",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ControllerType",
                table: "IC_RelayController");

            migrationBuilder.DropColumn(
                name: "SignalType",
                table: "IC_RelayController");
        }
    }
}
