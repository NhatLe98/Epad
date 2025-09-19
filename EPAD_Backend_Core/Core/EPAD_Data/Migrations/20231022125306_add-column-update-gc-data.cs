using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addcolumnupdategcdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeaveType",
                table: "GC_TimeLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GC_Customer",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    RegisterCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    CustomerID = table.Column<string>(type: "varchar(50)", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerNRIC = table.Column<string>(type: "varchar(50)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "varchar(50)", nullable: true),
                    CustomerEmail = table.Column<string>(type: "varchar(100)", nullable: true),
                    CustomerCompany = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsVip = table.Column<bool>(type: "bit", nullable: false),
                    DataStorageTime = table.Column<int>(type: "int", nullable: false, defaultValue: 18),
                    ContactPersonATIDs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccompanyingPersonList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfContactPerson = table.Column<short>(type: "smallint", nullable: false),
                    RegisterTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtensionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BikeType = table.Column<short>(type: "smallint", nullable: true),
                    BikeModel = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    BikePlate = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    BikeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerFaceImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NRICFrontImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NRICBackImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicensePlateFrontImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicensePlateBackImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoInSystem = table.Column<bool>(type: "bit", nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    RulesCustomerIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Customer", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_EmployeeVehicle",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    EmployeeATID = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    ToDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Plate = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    VehicleImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    RegistrationImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_EmployeeVehicle", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_ParkingLotAccessed",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingLotIndex = table.Column<int>(type: "int", nullable: false),
                    EmployeeATID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerIndex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessType = table.Column<short>(type: "smallint", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_ParkingLotAccessed", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_Customer",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameInEng = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumberOfConnect = table.Column<short>(type: "smallint", nullable: false),
                    NumberOfDaysForSaveData = table.Column<int>(type: "int", nullable: false),
                    UseDefault = table.Column<bool>(type: "bit", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_Customer", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_Customer_Gates",
                columns: table => new
                {
                    RulesCustomerIndex = table.Column<int>(type: "int", nullable: false),
                    GateIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    LineIndexs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_Customer_Gates", x => new { x.RulesCustomerIndex, x.GateIndex, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_GeneralAccess_Gates",
                columns: table => new
                {
                    RulesGeneralIndex = table.Column<int>(type: "int", nullable: false),
                    GateIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    LineIndexs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_GeneralAccess_Gates", x => new { x.RulesGeneralIndex, x.GateIndex, x.CompanyIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Customer");

            migrationBuilder.DropTable(
                name: "GC_EmployeeVehicle");

            migrationBuilder.DropTable(
                name: "GC_ParkingLotAccessed");

            migrationBuilder.DropTable(
                name: "GC_Rules_Customer");

            migrationBuilder.DropTable(
                name: "GC_Rules_Customer_Gates");

            migrationBuilder.DropTable(
                name: "GC_Rules_GeneralAccess_Gates");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "GC_TimeLog");
        }
    }
}
