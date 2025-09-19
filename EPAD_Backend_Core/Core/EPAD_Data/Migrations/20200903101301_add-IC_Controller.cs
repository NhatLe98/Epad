using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addIC_Controller : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_RelayController",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(maxLength: 50, nullable: true),
                    Port = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_RelayController", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_RelayControllerChannel",
                columns: table => new
                {
                    RelayControllerIndex = table.Column<int>(nullable: false),
                    ChannelIndex = table.Column<short>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    NumberOfSecondsOff = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_RelayControllerChannel", x => new { x.RelayControllerIndex, x.ChannelIndex, x.CompanyIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_RelayController");

            migrationBuilder.DropTable(
                name: "IC_RelayControllerChannel");
        }
    }
}
