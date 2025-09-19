using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_Table_IC_PrivilegeMachineRealtime_and_IC_PrivilegeMachineRealtimeDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_PrivilegeMachineRealtime",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    PrivilegeGroup = table.Column<short>(type: "smallint", nullable: false),
                    GroupDeviceIndex = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceModule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_PrivilegeMachineRealtime", x => new { x.Index, x.CompanyIndex, x.UserName });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_PrivilegeMachineRealtime");
        }
    }
}
