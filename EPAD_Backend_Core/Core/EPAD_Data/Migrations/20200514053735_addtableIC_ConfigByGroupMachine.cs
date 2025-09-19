using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addtableIC_ConfigByGroupMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_ConfigByGroupMachine",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GroupDeviceIndex = table.Column<int>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    TimePos = table.Column<string>(type: "ntext", nullable: true),
                    EventType = table.Column<string>(maxLength: 100, nullable: true),
                    PreviousDays = table.Column<int>(nullable: true),
                    ProceedAfterEvent = table.Column<string>(maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "ntext", nullable: true),
                    SendMailWhenError = table.Column<bool>(nullable: false),
                    AlwaysSend = table.Column<bool>(nullable: false),
                    TitleEmailSuccess = table.Column<string>(maxLength: 200, nullable: true),
                    BodyEmailSuccess = table.Column<string>(type: "ntext", nullable: true),
                    TitleEmailError = table.Column<string>(maxLength: 200, nullable: true),
                    BodyEmailError = table.Column<string>(type: "ntext", nullable: true),
                    CustomField = table.Column<string>(type: "ntext", nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_ConfigByGroupMachine", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_ConfigByGroupMachine");
        }
    }
}
