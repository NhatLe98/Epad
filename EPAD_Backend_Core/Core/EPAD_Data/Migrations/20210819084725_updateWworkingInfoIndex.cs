using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updateWworkingInfoIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_IC_WorkingInfo_EmployeeATID",
            //    table: "IC_WorkingInfo");
          

            migrationBuilder.CreateIndex(
                name: "IX_IC_WorkingInfo_EmployeeATID_CompanyIndex_DepartmentIndex",
                table: "IC_WorkingInfo",
                columns: new[] { "EmployeeATID", "CompanyIndex", "DepartmentIndex" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.DropIndex(
                name: "IX_IC_WorkingInfo_EmployeeATID_CompanyIndex_DepartmentIndex",
                table: "IC_WorkingInfo");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IC_WorkingInfo_EmployeeATID",
            //    table: "IC_WorkingInfo",
            //    column: "EmployeeATID");
        }
    }
}
