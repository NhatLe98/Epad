using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_AttendanceLog",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    CheckTime = table.Column<DateTime>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    VerifyMode = table.Column<short>(nullable: true),
                    InOutMode = table.Column<short>(nullable: false),
                    WorkCode = table.Column<int>(nullable: true),
                    Reserve1 = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    Function = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_AttendanceLog", x => new { x.EmployeeATID, x.CompanyIndex, x.CheckTime, x.SerialNumber });
                });

            migrationBuilder.CreateTable(
                name: "IC_Command",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    CommandName = table.Column<string>(maxLength: 50, nullable: true),
                    Command = table.Column<string>(type: "varchar(max)", nullable: true),
                    RequestedTime = table.Column<DateTime>(nullable: true),
                    ExcutedTime = table.Column<DateTime>(nullable: true),
                    Excuted = table.Column<bool>(nullable: false),
                    CommandType = table.Column<int>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Command", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_CommandSystemGroup",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GroupName = table.Column<string>(maxLength: 50, nullable: true),
                    Excuted = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_CommandSystemGroup", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Company",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TaxCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Address = table.Column<string>(maxLength: 500, nullable: true),
                    Phone = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Password = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Company", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Config",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
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
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Config", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Department",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Code = table.Column<string>(maxLength: 50, nullable: true),
                    ParentIndex = table.Column<int>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Department", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_DepartmentAndDevice",
                columns: table => new
                {
                    DepartmentIndex = table.Column<int>(nullable: false),
                    SerialNumber = table.Column<string>(maxLength: 50, nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_DepartmentAndDevice", x => new { x.DepartmentIndex, x.SerialNumber, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_Device",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    AliasName = table.Column<string>(maxLength: 100, nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: true),
                    Port = table.Column<int>(nullable: true),
                    LastConnection = table.Column<DateTime>(nullable: true),
                    UserCount = table.Column<int>(nullable: true),
                    FingerCount = table.Column<int>(nullable: true),
                    AttendanceLogCount = table.Column<int>(nullable: true),
                    OperationLogCount = table.Column<int>(nullable: true),
                    FirmwareVersion = table.Column<string>(maxLength: 50, nullable: true),
                    Stamp = table.Column<string>(type: "varchar(15)", nullable: true),
                    OpStamp = table.Column<string>(type: "varchar(15)", nullable: true),
                    PhotoStamp = table.Column<string>(type: "varchar(15)", nullable: true),
                    ErrorDelay = table.Column<int>(nullable: true),
                    Delay = table.Column<int>(nullable: true),
                    TransTimes = table.Column<string>(type: "varchar(60)", nullable: true),
                    TransInterval = table.Column<int>(nullable: true),
                    TransFlag = table.Column<string>(type: "varchar(300)", nullable: true),
                    Realtime = table.Column<int>(nullable: true),
                    Encrypt = table.Column<short>(nullable: true),
                    TimeZoneclock = table.Column<short>(nullable: true),
                    Reserve1 = table.Column<string>(maxLength: 50, nullable: true),
                    Reserve2 = table.Column<string>(maxLength: 50, nullable: true),
                    Reserve3 = table.Column<int>(nullable: true),
                    ATTLOGStamp = table.Column<string>(maxLength: 15, nullable: true),
                    OPERLOGStamp = table.Column<string>(maxLength: 15, nullable: true),
                    ATTPHOTOStamp = table.Column<string>(maxLength: 15, nullable: true),
                    SMSStamp = table.Column<string>(maxLength: 15, nullable: true),
                    USER_SMSStamp = table.Column<string>(maxLength: 15, nullable: true),
                    USERINFOStamp = table.Column<string>(maxLength: 15, nullable: true),
                    FINGERTMPStamp = table.Column<string>(maxLength: 15, nullable: true),
                    DeviceNumber = table.Column<int>(nullable: true),
                    DeviceType = table.Column<int>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    UseSDK = table.Column<bool>(nullable: true),
                    UsePush = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Device", x => new { x.SerialNumber, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_Employee",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    EmployeeCode = table.Column<string>(maxLength: 50, nullable: true),
                    FullName = table.Column<string>(maxLength: 200, nullable: true),
                    Gender = table.Column<short>(nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    NameOnMachine = table.Column<string>(maxLength: 200, nullable: true),
                    DepartmentIndex = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Employee", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_EmployeeTransfer",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    NewDepartment = table.Column<int>(nullable: false),
                    FromTime = table.Column<DateTime>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    OldDepartment = table.Column<int>(nullable: true),
                    RemoveFromOldDepartment = table.Column<bool>(nullable: false),
                    AddOnNewDepartment = table.Column<bool>(nullable: false),
                    ToTime = table.Column<DateTime>(nullable: false),
                    IsSync = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_EmployeeTransfer", x => new { x.EmployeeATID, x.CompanyIndex, x.NewDepartment, x.FromTime });
                });

            migrationBuilder.CreateTable(
                name: "IC_OperationLog",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    OpTime = table.Column<DateTime>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    OpCode = table.Column<short>(nullable: false),
                    AdminID = table.Column<string>(maxLength: 10, nullable: true),
                    Reserve1 = table.Column<short>(nullable: true),
                    Reserve2 = table.Column<short>(nullable: true),
                    Reserve3 = table.Column<short>(nullable: true),
                    Reserve4 = table.Column<short>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_OperationLog", x => new { x.SerialNumber, x.CompanyIndex, x.OpTime });
                });

            migrationBuilder.CreateTable(
                name: "IC_PrivilegeDetails",
                columns: table => new
                {
                    PrivilegeIndex = table.Column<int>(nullable: false),
                    FormName = table.Column<string>(maxLength: 100, nullable: false),
                    Role = table.Column<string>(maxLength: 20, nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_PrivilegeDetails", x => new { x.PrivilegeIndex, x.CompanyIndex, x.FormName, x.Role });
                });

            migrationBuilder.CreateTable(
                name: "IC_Service",
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
                    table.PrimaryKey("PK_IC_Service", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_ServiceAndDevices",
                columns: table => new
                {
                    ServiceIndex = table.Column<int>(nullable: false),
                    SerialNumber = table.Column<string>(maxLength: 50, nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_ServiceAndDevices", x => new { x.ServiceIndex, x.CompanyIndex, x.SerialNumber });
                });

            migrationBuilder.CreateTable(
                name: "IC_SystemCommand",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    CommandName = table.Column<string>(maxLength: 50, nullable: true),
                    Command = table.Column<string>(type: "varchar(max)", nullable: true),
                    Params = table.Column<string>(nullable: true),
                    EmployeeATIDs = table.Column<string>(nullable: true),
                    RequestedTime = table.Column<DateTime>(nullable: true),
                    ExcutedTime = table.Column<DateTime>(nullable: true),
                    Excuted = table.Column<bool>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false),
                    GroupIndex = table.Column<int>(nullable: false),
                    ExcutingServiceIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_SystemCommand", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_UserAccount",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    Password = table.Column<string>(maxLength: 500, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    ResetPasswordCode = table.Column<string>(maxLength: 20, nullable: true),
                    Disabled = table.Column<bool>(nullable: false),
                    LockTo = table.Column<DateTime>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    AccountPrivilege = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserAccount", x => new { x.UserName, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_UserFaceTemplate",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    FaceIndex = table.Column<short>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    FaceTemplate = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserFaceTemplate", x => new { x.EmployeeATID, x.CompanyIndex, x.SerialNumber, x.FaceIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_UserFinger",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    FingerIndex = table.Column<short>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    FingerData = table.Column<string>(type: "varchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserFinger", x => new { x.EmployeeATID, x.CompanyIndex, x.SerialNumber, x.FingerIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_UserInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(type: "varchar(20)", nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    Privilege = table.Column<short>(nullable: true),
                    Password = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Reserve1 = table.Column<string>(maxLength: 50, nullable: true),
                    Reserve2 = table.Column<int>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserInfo", x => new { x.EmployeeATID, x.CompanyIndex, x.SerialNumber });
                    table.UniqueConstraint("AK_IC_UserInfo_CompanyIndex_EmployeeATID_SerialNumber", x => new { x.CompanyIndex, x.EmployeeATID, x.SerialNumber });
                });

            migrationBuilder.CreateTable(
                name: "IC_UserPrivilege",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    UseForDefault = table.Column<bool>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    CompanyIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserPrivilege", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_AttendanceLog");

            migrationBuilder.DropTable(
                name: "IC_Command");

            migrationBuilder.DropTable(
                name: "IC_CommandSystemGroup");

            migrationBuilder.DropTable(
                name: "IC_Company");

            migrationBuilder.DropTable(
                name: "IC_Config");

            migrationBuilder.DropTable(
                name: "IC_Department");

            migrationBuilder.DropTable(
                name: "IC_DepartmentAndDevice");

            migrationBuilder.DropTable(
                name: "IC_Device");

            migrationBuilder.DropTable(
                name: "IC_Employee");

            migrationBuilder.DropTable(
                name: "IC_EmployeeTransfer");

            migrationBuilder.DropTable(
                name: "IC_OperationLog");

            migrationBuilder.DropTable(
                name: "IC_PrivilegeDetails");

            migrationBuilder.DropTable(
                name: "IC_Service");

            migrationBuilder.DropTable(
                name: "IC_ServiceAndDevices");

            migrationBuilder.DropTable(
                name: "IC_SystemCommand");

            migrationBuilder.DropTable(
                name: "IC_UserAccount");

            migrationBuilder.DropTable(
                name: "IC_UserFaceTemplate");

            migrationBuilder.DropTable(
                name: "IC_UserFinger");

            migrationBuilder.DropTable(
                name: "IC_UserInfo");

            migrationBuilder.DropTable(
                name: "IC_UserPrivilege");
        }
    }
}
