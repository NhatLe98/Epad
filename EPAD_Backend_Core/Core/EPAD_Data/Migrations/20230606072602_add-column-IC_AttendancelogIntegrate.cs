using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addcolumnIC_AttendancelogIntegrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_AttendancelogIntegrate",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: true),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    CheckTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    VerifyMode = table.Column<short>(type: "smallint", nullable: true),
                    InOutMode = table.Column<short>(type: "smallint", nullable: false),
                    WorkCode = table.Column<int>(type: "int", nullable: true),
                    Reserve1 = table.Column<int>(type: "int", nullable: true),
                    FaceMask = table.Column<int>(type: "int", nullable: true),
                    BodyTemperature = table.Column<double>(type: "float", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Function = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSend = table.Column<bool>(type: "bit", nullable: true),
                    AccessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceNumber = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_AttendancelogIntegrate", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_AttendancelogIntegrate");
        }
    }
}
