using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addvirtualtableemployeetransfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IC_DepartmentIndex",
                table: "IC_EmployeeTransfer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IC_EmployeeTransfer_IC_DepartmentIndex",
                table: "IC_EmployeeTransfer",
                column: "IC_DepartmentIndex");

            migrationBuilder.AddForeignKey(
                name: "FK_IC_EmployeeTransfer_IC_Department_IC_DepartmentIndex",
                table: "IC_EmployeeTransfer",
                column: "IC_DepartmentIndex",
                principalTable: "IC_Department",
                principalColumn: "Index",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IC_EmployeeTransfer_IC_Employee_EmployeeATID_CompanyIndex",
                table: "IC_EmployeeTransfer",
                columns: new[] { "EmployeeATID", "CompanyIndex" },
                principalTable: "IC_Employee",
                principalColumns: new[] { "EmployeeATID", "CompanyIndex" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IC_EmployeeTransfer_IC_Department_IC_DepartmentIndex",
                table: "IC_EmployeeTransfer");

            migrationBuilder.DropForeignKey(
                name: "FK_IC_EmployeeTransfer_IC_Employee_EmployeeATID_CompanyIndex",
                table: "IC_EmployeeTransfer");

            migrationBuilder.DropIndex(
                name: "IX_IC_EmployeeTransfer_IC_DepartmentIndex",
                table: "IC_EmployeeTransfer");

            migrationBuilder.DropColumn(
                name: "IC_DepartmentIndex",
                table: "IC_EmployeeTransfer");
        }
    }
}
