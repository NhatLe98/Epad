using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updateissuccessic_HistoryTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Failed",
                table: "IC_HistoryTrackingIntegrate");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "IC_HistoryTrackingIntegrate");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "IC_HistoryTrackingIntegrate",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "IC_HistoryTrackingIntegrate",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "IC_HistoryTrackingIntegrate");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "IC_HistoryTrackingIntegrate");

            migrationBuilder.AddColumn<short>(
                name: "Failed",
                table: "IC_HistoryTrackingIntegrate",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Success",
                table: "IC_HistoryTrackingIntegrate",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
