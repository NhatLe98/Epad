using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_column_table_TA_Rules_Shift_InOut : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TA_Rules_Shift_InOut",
                table: "TA_Rules_Shift_InOut");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "TA_Rules_Shift_InOut",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TA_Rules_Shift_InOut",
                table: "TA_Rules_Shift_InOut",
                column: "Index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TA_Rules_Shift_InOut",
                table: "TA_Rules_Shift_InOut");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "TA_Rules_Shift_InOut");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TA_Rules_Shift_InOut",
                table: "TA_Rules_Shift_InOut",
                columns: new[] { "RuleShiftIndex", "CompanyIndex", "FromTime", "ToTime" });
        }
    }
}
