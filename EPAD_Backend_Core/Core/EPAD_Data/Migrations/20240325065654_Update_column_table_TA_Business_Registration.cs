using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Update_column_table_TA_Business_Registration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LeaveDate",
                table: "TA_BusinessRegistration",
                newName: "BusinessDate");

            migrationBuilder.RenameColumn(
                name: "BusinessPlace",
                table: "TA_BusinessRegistration",
                newName: "WorkPlace");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkPlace",
                table: "TA_BusinessRegistration",
                newName: "BusinessPlace");

            migrationBuilder.RenameColumn(
                name: "BusinessDate",
                table: "TA_BusinessRegistration",
                newName: "LeaveDate");
        }
    }
}
