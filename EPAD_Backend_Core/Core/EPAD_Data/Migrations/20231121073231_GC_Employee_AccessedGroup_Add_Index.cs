using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class GC_Employee_AccessedGroup_Add_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GC_Employee_AccessedGroup",
                table: "GC_Employee_AccessedGroup");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeATID",
                table: "GC_Employee_AccessedGroup",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "GC_Employee_AccessedGroup",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GC_Employee_AccessedGroup",
                table: "GC_Employee_AccessedGroup",
                column: "Index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GC_Employee_AccessedGroup",
                table: "GC_Employee_AccessedGroup");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "GC_Employee_AccessedGroup");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeATID",
                table: "GC_Employee_AccessedGroup",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GC_Employee_AccessedGroup",
                table: "GC_Employee_AccessedGroup",
                columns: new[] { "CompanyIndex", "EmployeeATID", "FromDate" });
        }
    }
}
