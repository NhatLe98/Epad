using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addtableICGroupDeviceAndICGroupDeviceDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_GroupDevice",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_GroupDevice", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_GroupDeviceDetails",
                columns: table => new
                {
                    GroupDeviceIndex = table.Column<int>(nullable: false),
                    SerialNumber = table.Column<string>(maxLength: 50, nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_GroupDeviceDetails", x => new { x.GroupDeviceIndex, x.CompanyIndex, x.SerialNumber });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_GroupDevice");

            migrationBuilder.DropTable(
                name: "IC_GroupDeviceDetails");
        }
    }
}
