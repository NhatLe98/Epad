using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Update_table_GC_TruckExtraDriverLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TruckDriverLog",
                table: "GC_TruckExtraDriverLog");

            migrationBuilder.RenameColumn(
                name: "MachineSerial",
                table: "GC_TruckExtraDriverLog",
                newName: "TripCode");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDay",
                table: "GC_TruckExtraDriverLog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TripCode",
                table: "GC_TruckExtraDriverLog",
                newName: "MachineSerial");

            migrationBuilder.AlterColumn<string>(
                name: "BirthDay",
                table: "GC_TruckExtraDriverLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "TruckDriverLog",
                table: "GC_TruckExtraDriverLog",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
