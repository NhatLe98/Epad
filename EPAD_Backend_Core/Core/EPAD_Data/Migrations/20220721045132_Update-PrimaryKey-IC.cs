using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class UpdatePrimaryKeyIC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IC_RelayControllerChannel",
                table: "IC_RelayControllerChannel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IC_RelayControllerChannel",
                table: "IC_RelayControllerChannel",
                columns: new[] { "RelayControllerIndex", "ChannelIndex", "CompanyIndex", "SignalType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IC_RelayControllerChannel",
                table: "IC_RelayControllerChannel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IC_RelayControllerChannel",
                table: "IC_RelayControllerChannel",
                columns: new[] { "RelayControllerIndex", "ChannelIndex", "CompanyIndex" });
        }
    }
}
