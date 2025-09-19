using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_Department_Add_New_Field_OVN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStore",
                table: "IC_Department",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobGradeGrade",
                table: "IC_Department",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStore",
                table: "IC_Department");

            migrationBuilder.DropColumn(
                name: "JobGradeGrade",
                table: "IC_Department");
        }
    }
}
