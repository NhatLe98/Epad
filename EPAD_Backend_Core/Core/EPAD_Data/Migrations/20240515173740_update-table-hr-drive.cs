using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatetablehrdrive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Avatar",
                table: "IC_PlanDock",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyIndex",
                table: "IC_PlanDock",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "IC_PlanDock",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "IC_PlanDock",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "IC_PlanDock",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "IC_PlanDock");

            migrationBuilder.DropColumn(
                name: "CompanyIndex",
                table: "IC_PlanDock");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "IC_PlanDock");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "IC_PlanDock");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "IC_PlanDock");
        }
    }
}
