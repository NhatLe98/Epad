using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class TA_Shift_Add_New_Field_OT_First : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOTFirst",
                table: "TA_Shift",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTEndTimeFirst",
                table: "TA_Shift",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTStartTimeFirst",
                table: "TA_Shift",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOTFirst",
                table: "TA_Shift");

            migrationBuilder.DropColumn(
                name: "OTEndTimeFirst",
                table: "TA_Shift");

            migrationBuilder.DropColumn(
                name: "OTStartTimeFirst",
                table: "TA_Shift");
        }
    }
}
