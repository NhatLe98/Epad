using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addTableIC_UserMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthenType",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IC_UserMaster",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NameOnMachine = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    Password = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    FingerData0 = table.Column<string>(type: "varchar(max)", maxLength: 200, nullable: true),
                    FingerData1 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData2 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData3 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData4 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData5 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData6 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData7 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData8 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerData9 = table.Column<string>(type: "varchar(max)", nullable: true),
                    FingerVersion = table.Column<string>(maxLength: 10, nullable: true),
                    FaceTemplate = table.Column<string>(type: "text", nullable: true),
                    FaceVersion = table.Column<string>(maxLength: 10, nullable: true),
                    VeinsData1 = table.Column<string>(type: "nvarchar(3000)", nullable: true),
                    VeinsData2 = table.Column<string>(type: "nvarchar(3000)", nullable: true),
                    VeinsData3 = table.Column<string>(type: "nvarchar(3000)", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserMaster", x => new { x.EmployeeATID, x.CompanyIndex });
                    table.UniqueConstraint("AK_IC_UserMaster_Index", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "AuthenType",
                table: "IC_UserInfo");
        }
    }
}
