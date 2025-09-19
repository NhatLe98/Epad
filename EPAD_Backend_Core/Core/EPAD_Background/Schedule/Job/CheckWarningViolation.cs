using EPAD_Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using EPAD_Common.Extensions;
using EPAD_Common.Utility;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;
using EPAD_Data.Entities;
using System.Linq;
using EPAD_Common.FileProvider;
using EPAD_Data.Models;
using EPAD_Common.EmailProvider;
using EPAD_Data;
using EPAD_Common.Enums;
using EPAD_Common.Locales;
using System.Net.Http;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using Microsoft.Extensions.Configuration;

namespace EPAD_Background.Schedule.Job
{
    public class CheckWarningViolation : BaseJob
    {
        private readonly ILogger Logger;
        private readonly ILoggerFactory LoggerFactory;

        IMemoryCache _Cache;
        ConfigObject _Config;

        protected readonly EPAD_Context _dbContext;
        private bool isUsingOnGcsEpadRealTime;

        private readonly IGC_Rules_WarningService _GC_Rules_WarningService;
        private readonly IGC_Rules_Warning_EmailScheduleService _GC_Rules_Warning_EmailScheduleService;
        private readonly IGC_TimeLogService _GC_TimeLogService;
        private readonly IGC_CustomerService _GC_CustomerService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IEmailSender _EmailSender;
        private readonly IStoreFileProvider _FileHandler;
        private readonly ILocales i18n;
        private IConfiguration _Configuration;
        public CheckWarningViolation(IServiceScopeFactory provider, ILoggerFactory loggerFactory, IConfiguration configuration) : base(provider)
        {
            _GC_Rules_WarningService = TryRessolve<IGC_Rules_WarningService>();
            _GC_Rules_Warning_EmailScheduleService = TryRessolve<IGC_Rules_Warning_EmailScheduleService>();
            _GC_TimeLogService = TryRessolve<IGC_TimeLogService>();
            _GC_CustomerService = TryRessolve<IGC_CustomerService>();
            _HR_UserService = TryRessolve<IHR_UserService>();
            _EmailSender = TryRessolve<IEmailSender>();
            _FileHandler = TryRessolve<IStoreFileProvider>();

            _dbContext = TryRessolve<EPAD_Context>();
            i18n = TryRessolve<ILocales>();
            _Configuration = configuration;

            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<CheckWarningViolation>();
            _Cache = TryRessolve<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            isUsingOnGcsEpadRealTime = Convert.ToBoolean(_Configuration.GetValue<string>("IsUsingOnEpad"));
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await DoWorkAsync();
        }

        private async Task DoWorkAsync()
        {
            if (isUsingOnGcsEpadRealTime)
            {
                var toDate = DateTime.Now;
                var schedules = await _GC_Rules_Warning_EmailScheduleService.GetSchedulesByTimeAndCompanyIndex(toDate.TimeOfDay,
                    (int)toDate.DayOfWeek, _Config.CompanyIndex);
                var listVipCustomers = await _GC_CustomerService.GetVipCustomers();
                if (schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        var rulesWarning = await _GC_Rules_WarningService.GetDataByIndex(schedule.RulesWarningIndex);
                        if (rulesWarning != null && (rulesWarning.UseEmail ?? false) && rulesWarning.EmailSendType == 1)
                        {
                            var group = _GC_Rules_WarningService.GetRulesWarningGroupsByGroupIndex(rulesWarning.RulesWarningGroupIndex);
                            if (group != null)
                            {
                                var preSchedule = _GC_Rules_Warning_EmailScheduleService.GetFromDateSendMail(schedule);
                                var fromDate = preSchedule.LatestDateSendMail;
                                IEnumerable<GC_TimeLog> timeLogs;
                                if (fromDate == null)
                                {
                                    if (group.Code == "IsVipCustomer")
                                    {
                                        timeLogs = _GC_TimeLogService.Where(e => e.CompanyIndex == _Config.CompanyIndex
                                    && e.Time <= toDate && e.CustomerIndex != 0);
                                    }
                                    else
                                    {
                                        timeLogs = _GC_TimeLogService.Where(e => e.CompanyIndex == _Config.CompanyIndex
                                    && e.Time <= toDate && e.Error == group.Code);
                                    }
                                }
                                else
                                {
                                    if (group.Code == "IsVipCustomer")
                                    {
                                        timeLogs = _GC_TimeLogService.Where(e => e.CompanyIndex == _Config.CompanyIndex
                                   && e.Time <= toDate && e.Time > fromDate
                                   && e.CustomerIndex != 0);
                                    }
                                    else
                                    {
                                        timeLogs = _GC_TimeLogService.Where(e => e.CompanyIndex == _Config.CompanyIndex
                                   && e.Time <= toDate && e.Time > fromDate
                                   && e.Error == group.Code);
                                    }
                                }

                                if (group.Code == "IsVipCustomer")
                                {
                                    timeLogs = timeLogs.Where(e => listVipCustomers.Any(y => y.Index == e.CustomerIndex));
                                }
                                if (timeLogs != null && timeLogs.Count() > 0)
                                {
                                    await CreateAndSendMailBySchedule(rulesWarning.Email, fromDate, toDate, timeLogs);
                                    schedule.LatestDateSendMail = toDate;
                                    _dbContext.GC_Rules_Warning_EmailSchedules.Update(schedule);
                                }
                            }
                        }
                    }
                    _dbContext.SaveChanges();
                }
            }
        }

        private async Task<bool> CheckIsVIPCustomer(int customerIndex)
        {
            var customer = await _GC_CustomerService.GetDataByIndex(customerIndex);
            return (customer != null && customer.IsVip);
        }

        private async Task CreateAndSendMailBySchedule(string pEmail, DateTime? pFromDate, DateTime pToDate, IEnumerable<GC_TimeLog> pTimeLogs)
        {
            EASMailInfo mailInfo = new EASMailInfo();
            mailInfo.IsSingleToDate = (pFromDate == null);
            mailInfo.StartDate = pFromDate;
            mailInfo.EndDate = pToDate;

            await SendMailForCurrentUser(pEmail, mailInfo, pTimeLogs);
        }

        public async Task CreateAndSendMail(string pEmail, GC_TimeLog pTimeLog)
        {
            EASMailInfo mailInfo = new EASMailInfo();
            var timeLogs = new List<GC_TimeLog>();
            var timeLog = _GC_TimeLogService.FirstOrDefault(e => e.Index == pTimeLog.Index);
            if (timeLog != null)
            {
                timeLogs.Add(timeLog);
                mailInfo.StartDate = timeLog.Time;
                mailInfo.IsSendItNow = true;
                await SendMailForCurrentUser(pEmail, mailInfo, timeLogs);
            }
        }

        async Task SendMailForCurrentUser(string pEmail, EASMailInfo pMailInfo, IEnumerable<GC_TimeLog> pTimeLogs)
        {
            var filePath = await CreateExcelByTimeLogs(pTimeLogs);
            pMailInfo.FilePath = filePath;
            pMailInfo.ListEmail.Add(pEmail);

            SendMailReminder(pMailInfo);
        }

        private void SendMailReminder(EASMailInfo pMailInfo)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"StaticFiles/EmailTemplate";
            string title = System.IO.File.ReadAllText(path + "/RulesWarningTitle.txt");

            var curBody = System.IO.File.ReadAllText(path + "/RulesWarningBody.txt");
            curBody = curBody.Replace("<endDate>", pMailInfo.EndDate.ToString("dd/MM/yyyy HH:mm"));
            if (pMailInfo.IsSendItNow)
            {
                curBody = System.IO.File.ReadAllText(path + "/RulesWarningBody_SendItNow.txt");
                curBody = curBody.Replace("<startDate>", pMailInfo.StartDate.Value.ToString("dd/MM/yyyy HH:mm"));
            }
            else if (pMailInfo.IsSingleToDate)
            {
                curBody = curBody.Replace("<startDate>", "trước");
            }
            else
            {
                curBody = curBody.Replace("<startDate>", pMailInfo.StartDate.Value.ToString("dd/MM/yyyy HH:mm"));
            }
            var filePaths = new List<string>() { pMailInfo.FilePath };
            _EmailSender.SendEmailToMulti("", title, curBody, string.Join(',', pMailInfo.ListEmail), filePaths.ToArray());
        }

        private async Task<string> CreateExcelByTimeLogs(IEnumerable<GC_TimeLog> pTimeLogs)
        {
            try
            {

                XLWorkbook workbook = new XLWorkbook(); // Load existed AINF template
                var worksheet = workbook.AddWorksheet("DanhSachCacTruongHopCanhBao", 0);
                worksheet.RowHeight = 50;
                int startRow = 1;
                int No = 1;

                worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("A" + startRow).Value = "STT";

                worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("B" + startRow).Value = "Họ tên";

                worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("C" + startRow).Value = "Đối tượng";

                worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("D" + startRow).Value = "Mã chấm công";

                //worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("E" + startRow).Value = "CMND";

                worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("E" + startRow).Value = "Chức vụ";

                //worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("G" + startRow).Value = "Người liên hệ làm việc";



                worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("F" + startRow).Value = "Sự kiện cảnh báo";

                worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("G" + startRow).Value = "Thời gian";
                startRow++;

                var timeLogs = new List<GC_TimeLog>();
                if (pTimeLogs != null && pTimeLogs.Count() > 0)
                {
                    timeLogs = pTimeLogs.ToList();
                }
                foreach (var timeLog in timeLogs)
                {
                    string fullName = "", employeeATID = "", nric = "", position = "", contactPerson = "";
                    if (timeLog.CustomerIndex == 0)
                    {
                        var currentUser = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(new List<string> { timeLog.EmployeeATID },
                            DateTime.Now, timeLog.CompanyIndex);
                        if (currentUser != null && currentUser.Count > 0)
                        {
                            employeeATID = timeLog.EmployeeATID;
                            fullName = currentUser[0].FullName;
                            position = currentUser[0].Position;
                        }
                    }
                    else
                    {
                        var customer = await _GC_CustomerService.GetDataByIndex(timeLog.CustomerIndex);
                        if (customer != null)
                        {
                            var contactUser = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(
                                new List<string> { customer.ContactPersonATIDs }, 
                                DateTime.Now, timeLog.CompanyIndex);
                            nric = customer.CustomerNRIC;
                            fullName = customer.CustomerName;
                            if (contactUser != null && contactUser.Count > 0)
                                contactPerson = contactUser[0].FullName;
                            if (customer.IsVip)
                            {
                                timeLog.ObjectAccessType = "VipCustomer";
                            }
                        }
                    }

                    worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("A" + startRow).Value = No;

                    worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("B" + startRow).Value = fullName;

                    worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("C" + startRow).Value = GetCheckedString(timeLog.CustomerIndex == 0 ? "Employee" : "Customer");

                    worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("D" + startRow).Value = "'" + employeeATID;

                    //worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("E" + startRow).Value = nric;

                    worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("E" + startRow).Value = position;

                    //worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("G" + startRow).Value = contactPerson;

                    worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("F" + startRow).Value = GetCheckedString(timeLog.Error);

                    worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("G" + startRow).Value = "'" + timeLog.Time.ToString("dd/MM/yyyy HH:mm");
                    //var image = GetStream("http://localhost:5000/Files/ImageFromCamera/7/101/2021-03-29/13/4211_9399.jpg");
                    //worksheet.AddPicture(image).MoveTo(worksheet.Cell("I" + startRow)).WithSize(50, 50);

                    No++;
                    startRow++;
                }

                using (MemoryStream stream = new MemoryStream())
                {

                    var path = "RulesWarningExcel/DanhSachCacTruongHopCanhBao/";
                    string filename = "DanhSachCacTruongHopCanhBao_" + DateTime.Now.ToFileTime() + ".xlsx";
                    workbook.SaveAs(stream);

                    var parentPath = _FileHandler.uploadAndUpdateUrlBase(stream.ToArray(), path, filename);
                    return parentPath;
                }


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<MemoryStream> CreateExcelByVehicleMOnitoringHistory(List<MonitoringVehicleHistoryModel> pData, string pLogType, int pStatus)
        {
            try
            {
                XLWorkbook workbook = new XLWorkbook(); // Load existed AINF template
                var worksheet = workbook.AddWorksheet("VehicleMonitoringHistory", 0);
                worksheet.RowHeight = 50;
                int startRow = 1;
                if (pLogType == "Employee")
                {
                    worksheet = SetWorkSheetHeaderEmployee(worksheet, startRow, pStatus);

                    startRow++;
                    worksheet = await SetWorkSheetDataEmployee(worksheet, startRow, pData, pStatus);
                }
                else
                {
                    worksheet = SetWorkSheetHeaderCustomer(worksheet, startRow, pStatus);
                    startRow++;
                    worksheet = await SetWorkSheetDataCustomer(worksheet, startRow, pData, pStatus);
                }

                worksheet.Row(1).Style.Fill.BackgroundColor = GetHeaderColor();
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Font.FontColor = XLColor.White;
                //worksheet.Row(1).Height = 10;
                worksheet.Columns().AdjustToContents();

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return stream;
                }
            }
            catch
            {
                return null;
            }
        }

        private XLColor GetHeaderColor()
        {
            return XLColor.FromTheme(XLThemeColor.Accent1);
        }

        protected async Task<Stream> GetStream(string url)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage result = await client.GetAsync(url);
                result.EnsureSuccessStatusCode();

                HttpContent content = result.Content;
                var stream = await result.Content.ReadAsStreamAsync();

                return stream;
            }
            catch
            {
                return null;
            }
        }

        public string getPath(string internalPath, int pCompanyIndex)
        {
            return _Config.WarningSoundRoot + "/" + pCompanyIndex + "/" + internalPath;
        }

        private IXLWorksheet SetWorkSheetHeaderEmployee(IXLWorksheet worksheet, int startRow, int pStatus)
        {
            worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("A" + startRow).Value = "STT";

            worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("B" + startRow).Value = "Biển số";

            worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("C" + startRow).Value = "Thời gian vào";

            worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("D" + startRow).Value = "Cổng vào";

            worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("E" + startRow).Value = "Nhân viên vào";

            worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("F" + startRow).Value = "Họ tên";

            worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("G" + startRow).Value = "Phòng ban";

            worksheet.Cell("H" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("H" + startRow).Value = "Ảnh biển số vào";

            worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("I" + startRow).Value = "Ảnh mặt vào";

            if (pStatus == 1) //Đang hiện diện
            {
                worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("J" + startRow).Value = "Người cập nhật vào";

                worksheet.Cell("K" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("K" + startRow).Value = "Ghi chú vào";
            }
            else
            {
                worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("J" + startRow).Value = "Thời gian ra";

                worksheet.Cell("K" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("K" + startRow).Value = "Cổng ra";

                worksheet.Cell("L" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("L" + startRow).Value = "Nhân viên ra";

                worksheet.Cell("M" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("M" + startRow).Value = "Họ tên";

                worksheet.Cell("N" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("N" + startRow).Value = "Phòng ban";

                worksheet.Cell("O" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("O" + startRow).Value = "Ảnh biển số ra";

                worksheet.Cell("P" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("P" + startRow).Value = "Ảnh mặt ra";

                worksheet.Cell("Q" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("Q" + startRow).Value = "Người cập nhật vào";

                worksheet.Cell("R" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("R" + startRow).Value = "Ghi chú vào";

                worksheet.Cell("S" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("S" + startRow).Value = "Người cập nhật ra";

                worksheet.Cell("T" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("T" + startRow).Value = "Ghi chú ra";
            }
            return worksheet;
        }
        private IXLWorksheet SetWorkSheetHeaderCustomer(IXLWorksheet worksheet, int startRow, int pStatus)
        {
            worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("A" + startRow).Value = "STT";

            worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("B" + startRow).Value = "Biển số";

            worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("C" + startRow).Value = "CMND";

            worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("D" + startRow).Value = "Họ tên";

            worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("E" + startRow).Value = "Thời gian vào";

            worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
            worksheet.Cell("F" + startRow).Value = "Cổng vào";


            if (pStatus == 1) //Đang hiện diện
            {
                worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("G" + startRow).Value = "Ảnh biển số vào";

                worksheet.Cell("H" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("H" + startRow).Value = "Ảnh mặt vào";

                worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("I" + startRow).Value = "Người cập nhật vào";

                worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("J" + startRow).Value = "Ghi chú vào";
            }
            else
            {

                worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("G" + startRow).Value = "Thời gian ra";

                worksheet.Cell("H" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("H" + startRow).Value = "Cổng ra";

                worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("I" + startRow).Value = "Ảnh biển số vào";

                worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("J" + startRow).Value = "Ảnh mặt vào";

                worksheet.Cell("K" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("K" + startRow).Value = "Ảnh biển số ra";

                worksheet.Cell("L" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("L" + startRow).Value = "Ảnh mặt ra";

                worksheet.Cell("M" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("M" + startRow).Value = "Người cập nhật vào";

                worksheet.Cell("N" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("N" + startRow).Value = "Ghi chú vào";

                worksheet.Cell("O" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("O" + startRow).Value = "Người cập nhật ra";

                worksheet.Cell("P" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("P" + startRow).Value = "Ghi chú ra";
            }
            return worksheet;
        }
        private async Task<IXLWorksheet> SetWorkSheetDataEmployee(IXLWorksheet worksheet, int startRow, List<MonitoringVehicleHistoryModel> pData, int pStatus)
        {
            foreach (var item in pData)
            {
                worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("A" + startRow).Value = startRow - 1;

                worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("B" + startRow).Value = item.VehiclePlate;

                worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("C" + startRow).Value = item.TimeIn.ToString("dd/MM/yyyy HH:mm");

                worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("D" + startRow).Value = item.GateNameIn;

                worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("E" + startRow).Value = item.EmployeeCodeIn;

                worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("F" + startRow).Value = item.EmployeeNameIn;

                worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("G" + startRow).Value = item.DepartmentNameIn;

                if (!string.IsNullOrEmpty(item.ImagePlateIn))
                {
                    var image = await GetStream(item.ImagePlateIn);
                    worksheet.AddPicture(image).MoveTo(worksheet.Cell("H" + startRow)).WithSize(50, 50);
                }

                if (!string.IsNullOrEmpty(item.ImageFaceIn))
                {
                    var image = await GetStream(item.ImageFaceIn);
                    worksheet.AddPicture(image).MoveTo(worksheet.Cell("I" + startRow)).WithSize(50, 50);
                }


                //worksheet.Cell("H" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("H" + startRow).Value = item.ImagePlateIn;

                //worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("I" + startRow).Value = item.ImageFaceIn;
                /*item.Time.ToString("dd/MM/yyyy HH:mm");*/

                if (pStatus == 1) //Đang hiện diện
                {
                    worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("J" + startRow).Value = item.UpdatedUserIn;

                    worksheet.Cell("K" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("K" + startRow).Value = item.NoteIn;
                }
                else
                {
                    worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("J" + startRow).Value = item.TimeOut.HasValue ? item.TimeOut.Value.ToString("dd/MM/yyyy HH:mm") : "";

                    worksheet.Cell("K" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("K" + startRow).Value = item.GateNameOut;

                    worksheet.Cell("L" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("L" + startRow).Value = item.EmployeeCodeOut;

                    worksheet.Cell("M" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("M" + startRow).Value = item.EmployeeNameOut;

                    worksheet.Cell("N" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("N" + startRow).Value = item.DepartmentNameOut;


                    if (!string.IsNullOrEmpty(item.ImagePlateOut))
                    {
                        var image = await GetStream(item.ImagePlateOut);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("O" + startRow)).WithSize(50, 50);
                    }

                    if (!string.IsNullOrEmpty(item.ImageFaceOut))
                    {
                        var image = await GetStream(item.ImageFaceOut);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("P" + startRow)).WithSize(50, 50);
                    }

                    //worksheet.Cell("O" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("O" + startRow).Value = item.ImagePlateOut;

                    //worksheet.Cell("P" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("P" + startRow).Value = item.ImageFaceOut;

                    worksheet.Cell("Q" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("Q" + startRow).Value = item.UpdatedUserIn;

                    worksheet.Cell("R" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("R" + startRow).Value = item.NoteIn;

                    worksheet.Cell("S" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("S" + startRow).Value = item.UpdatedUserOut;

                    worksheet.Cell("T" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("T" + startRow).Value = item.NoteOut;
                }

                startRow++;
            }
            return worksheet;
        }

        protected string GetCheckedString(string key)
        {
            return i18n.GetCheckedStringWithLang(key, Language.VI);
        }

        private async Task<IXLWorksheet> SetWorkSheetDataCustomer(IXLWorksheet worksheet, int startRow, List<MonitoringVehicleHistoryModel> pData, int pStatus)
        {
            foreach (var item in pData)
            {
                worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("A" + startRow).Value = startRow - 1;

                worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("B" + startRow).Value = item.VehiclePlate;

                worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("C" + startRow).Value = item.CustomerNRIC;

                worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("D" + startRow).Value = item.CustomerName;

                worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("E" + startRow).Value = item.TimeIn.ToString("dd/MM/yyyy HH:mm");

                worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("F" + startRow).Value = item.GateNameIn;

                //worksheet.Cell("H" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("H" + startRow).Value = item.ImagePlateIn;

                //worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("I" + startRow).Value = item.ImageFaceIn;
                /*item.Time.ToString("dd/MM/yyyy HH:mm");*/

                if (pStatus == 1) //Đang hiện diện
                {
                    if (!string.IsNullOrEmpty(item.ImagePlateIn))
                    {
                        var image = await GetStream(item.ImagePlateIn);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("G" + startRow)).WithSize(50, 50);
                    }

                    if (!string.IsNullOrEmpty(item.ImageFaceIn))
                    {
                        var image = await GetStream(item.ImageFaceIn);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("H" + startRow)).WithSize(50, 50);
                    }

                    worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("I" + startRow).Value = item.UpdatedUserIn;

                    worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("J" + startRow).Value = item.NoteIn;
                }
                else
                {
                    worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("G" + startRow).Value = item.TimeOut;

                    worksheet.Cell("H" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("H" + startRow).Value = item.GateNameOut;

                    worksheet.Cell("I" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("I" + startRow).Value = item.EmployeeCodeOut;

                    worksheet.Cell("J" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("J" + startRow).Value = item.EmployeeNameOut;

                    worksheet.Cell("K" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("K" + startRow).Value = item.DepartmentNameOut;


                    if (!string.IsNullOrEmpty(item.ImagePlateIn))
                    {
                        var image = await GetStream(item.ImagePlateIn);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("L" + startRow)).WithSize(50, 50);
                    }

                    if (!string.IsNullOrEmpty(item.ImageFaceIn))
                    {
                        var image = await GetStream(item.ImageFaceIn);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("M" + startRow)).WithSize(50, 50);
                    }

                    if (!string.IsNullOrEmpty(item.ImagePlateOut))
                    {
                        var image = await GetStream(item.ImagePlateOut);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("N" + startRow)).WithSize(50, 50);
                    }

                    if (!string.IsNullOrEmpty(item.ImageFaceOut))
                    {
                        var image = await GetStream(item.ImageFaceOut);
                        worksheet.AddPicture(image).MoveTo(worksheet.Cell("O" + startRow)).WithSize(50, 50);
                    }

                    //worksheet.Cell("O" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("O" + startRow).Value = item.ImagePlateOut;

                    //worksheet.Cell("P" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("P" + startRow).Value = item.ImageFaceOut;

                    worksheet.Cell("P" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("P" + startRow).Value = item.UpdatedUserIn;

                    worksheet.Cell("Q" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("Q" + startRow).Value = item.NoteIn;

                    worksheet.Cell("R" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("R" + startRow).Value = item.UpdatedUserOut;

                    worksheet.Cell("S" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("S" + startRow).Value = item.NoteOut;
                }

                startRow++;
            }
            return worksheet;
        }
    }

    public class EASMailInfo
    {
        public bool IsSendItNow { get; set; }
        public bool IsSingleToDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FilePath { get; set; }
        public List<string> ListEmail { get; set; }

        public EASMailInfo()
        {
            ListEmail = new List<string>();
        }
    }
}
