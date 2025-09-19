using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_column_table_GC_TruckExtraDriverLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GC_TruckExtraDriverLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "GC_TruckExtraDriverLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "GC_TruckExtraDriverLog",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "GC_TruckExtraDriverLog");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "GC_TruckExtraDriverLog");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "GC_TruckExtraDriverLog");
        }
    }
}
