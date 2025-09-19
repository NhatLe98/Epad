using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatetableacaccgroupupdatetimezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Timezone1",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Timezone2",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Timezone3",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone1",
                table: "AC_AccGroup");

            migrationBuilder.DropColumn(
                name: "Timezone2",
                table: "AC_AccGroup");

            migrationBuilder.DropColumn(
                name: "Timezone3",
                table: "AC_AccGroup");
        }
    }
}
