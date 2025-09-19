using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class Add_License : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_AccessToken",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyIndex = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Scope = table.Column<string>(maxLength: 500, nullable: true),
                    AccessToken = table.Column<string>(maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    ExpiredDate = table.Column<DateTime>(nullable: true),
                    Note = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_AccessToken", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_AppLicense",
                columns: table => new
                {
                    CompanyIndex = table.Column<int>(nullable: false),
                    PublicKey = table.Column<string>(maxLength: 200, nullable: true),
                    Data = table.Column<string>(maxLength: 2000, nullable: true),
                    Note = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_AppLicense", x => x.CompanyIndex);
                });

            migrationBuilder.CreateTable(
                name: "IC_HardwareLicense",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyIndex = table.Column<int>(nullable: false),
                    Data = table.Column<string>(maxLength: 2000, nullable: true),
                    FileName = table.Column<string>(maxLength: 50, nullable: true),
                    Note = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_HardwareLicense", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_AccessToken");

            migrationBuilder.DropTable(
                name: "IC_AppLicense");

            migrationBuilder.DropTable(
                name: "IC_HardwareLicense");
        }
    }
}
