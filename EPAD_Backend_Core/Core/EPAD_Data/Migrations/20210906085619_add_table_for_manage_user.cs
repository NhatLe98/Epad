using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class add_table_for_manage_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HR_CardNumberInfo",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_CardNumberInfo", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_CustomerInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    NRIC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVIP = table.Column<bool>(type: "bit", nullable: true),
                    IdentityImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    RegisterCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    CustomerID = table.Column<string>(type: "varchar(50)", nullable: true),
                    DataStorageTime = table.Column<int>(type: "int", nullable: true),
                    ContactPersonATIDs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccompanyingPersonList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfContactPerson = table.Column<short>(type: "smallint", nullable: false),
                    RegisterTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtensionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BikeType = table.Column<short>(type: "smallint", nullable: false),
                    BikeModel = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    BikePlate = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    BikeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NRICFrontImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NRICBackImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicensePlateFrontImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicensePlateBackImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoInSystem = table.Column<bool>(type: "bit", nullable: true),
                    RulesCustomerIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_CustomerInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_EmployeeInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_EmployeeInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_ParentInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Students = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_ParentInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_StudentInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_StudentInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_User",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Avatar = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Gender = table.Column<short>(type: "smallint", nullable: true),
                    DayOfBirth = table.Column<int>(type: "int", nullable: true),
                    MonthOfBirth = table.Column<int>(type: "int", nullable: true),
                    YearOfBirth = table.Column<int>(type: "int", nullable: true),
                    NameOnMachine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmployeeType = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_User", x => new { x.EmployeeATID, x.CompanyIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_CardNumberInfo");

            migrationBuilder.DropTable(
                name: "HR_CustomerInfo");

            migrationBuilder.DropTable(
                name: "HR_EmployeeInfo");

            migrationBuilder.DropTable(
                name: "HR_ParentInfo");

            migrationBuilder.DropTable(
                name: "HR_StudentInfo");

            migrationBuilder.DropTable(
                name: "HR_User");
        }
    }
}
