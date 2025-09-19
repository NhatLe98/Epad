using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addIndexForTableWorkingInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateIndex(
            //    name: "IX_IC_WorkingInfo_EmployeeATID_CompanyIndex",
            //    table: "IC_WorkingInfo",
            //    columns: new[] { "EmployeeATID", "CompanyIndex" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_IC_WorkingInfo_EmployeeATID_CompanyIndex",
            //    table: "IC_WorkingInfo");
        }
    }
}
