using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class update_context : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HR_ExcusedAbsent",
                table: "HR_ExcusedAbsent");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeATID",
                table: "HR_ExcusedAbsent",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HR_ExcusedAbsent",
                table: "HR_ExcusedAbsent",
                column: "Index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HR_ExcusedAbsent",
                table: "HR_ExcusedAbsent");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeATID",
                table: "HR_ExcusedAbsent",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HR_ExcusedAbsent",
                table: "HR_ExcusedAbsent",
                columns: new[] { "EmployeeATID", "AbsentDate" });
        }
    }
}
