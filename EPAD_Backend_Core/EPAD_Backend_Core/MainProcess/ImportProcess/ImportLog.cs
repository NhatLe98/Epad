using ClosedXML.Excel;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.MainProcess.ImportProcess
{
    public class ImportLog : IImportProcess
    {
        public string Process(EPAD_Context context, List<string> listFilePath,int companyIndex,string userName)
        {
            string result = $"Số file import {listFilePath.Count}.";
            string[] formatConvert = new string [1]{ "yyyy-MM-dd HH:mm:ss"};
            List<HR_User> listEmployees = context.HR_User.Where(t => t.CompanyIndex == companyIndex).ToList();
            for (int i = 0; i < listFilePath.Count; i++)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(listFilePath[i]);
                if (fileInfo == null)
                {
                    result += Environment.NewLine + "---- File không tồn tại " + listFilePath[i];
                    continue;
                }
                result += Environment.NewLine + "---- Xử lý file " + fileInfo.Name;
                if (fileInfo.Extension.Contains(".txt"))
                {
                    string fileNameNoExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
                    string[] arrInfo = fileNameNoExtension.Split('_');
                    if (arrInfo.Length != 2)
                    {
                        result += Environment.NewLine + "-ERR: Tên file không đúng định dạng.";
                        continue;
                    }

                    string serial = arrInfo[0];
                    IC_Device device = context.IC_Device.Where(t => t.CompanyIndex == companyIndex && t.SerialNumber == serial).FirstOrDefault();
                    if (device == null)
                    {
                        result += Environment.NewLine + $"-ERR: Serial number không tồn tại: {serial}.";
                        continue;
                    }

                    string data = System.IO.File.ReadAllText(listFilePath[i]);
                    string[] arrRow = data.Split("\r\n");
                    string errorDetail = "";
                    int rowSuccess = 0;
                    int rowError = 0;
                    DateTime now = DateTime.Now;
                    foreach (string item in arrRow)
                    {
                        if (item.Trim() == "")
                        {
                            continue;
                        }
                        string[] arrDetail = item.Split("\t");
                        string error = "";
                        bool success = AddAttendanceLog(context,listEmployees, arrDetail[0], serial, companyIndex, arrDetail[1], arrDetail[4], arrDetail[3], arrDetail[5], userName, now, formatConvert, ref error);
                        if (success == true)
                        {
                            rowSuccess++;
                        }
                        else
                        {
                            rowError++;
                            errorDetail += error;
                        }
                        
                    }
                    result += Environment.NewLine + $"- Số dòng thành công: {rowSuccess}";
                    result += Environment.NewLine + $"- Số dòng thất bại: {rowError}";
                    result += Environment.NewLine + (errorDetail == "" ? "" : ($"- Chi tiết lỗi: {errorDetail}"));

                }
                else if (fileInfo.Extension.Contains(".xlsx"))
                {
                    var workbook = new XLWorkbook(listFilePath[i]);
                    var wsEmp = workbook.Worksheet(1);
                    var tbl = wsEmp.RangeUsed().AsTable();
                    int firstRow = tbl.FirstCell().Address.RowNumber;
                    int firstColumn = tbl.FirstCell().Address.ColumnNumber;
                    int lastRow = tbl.LastCell().Address.RowNumber;
                    int lastColumn = tbl.LastCell().Address.ColumnNumber;
                    try
                    {
                        string format = tbl.Cell(1, 2).Value.ToString().ToLower().Replace("m", "M");
                        format += " HH:mm:ss";
                        string[] arrFormat = new string[1] { format };
                        string serial = tbl.Cell(2, 2).Value.ToString().Trim();
                        IC_Device device = context.IC_Device.Where(t => t.CompanyIndex == companyIndex && (t.SerialNumber == serial || t.IPAddress == serial)).FirstOrDefault();
                        if (device == null)
                        {
                            result += Environment.NewLine + $"-ERR: Serial number không tồn tại: {serial}.";
                            continue;
                        }
                        string errorDetail = "";
                        int rowSuccess = 0;
                        int rowError = 0;
                        DateTime now = DateTime.Now;
                        for (int rowIndex = 4; rowIndex <= lastRow; rowIndex++)
                        {
                            string empATID = tbl.Cell(rowIndex, 1).Value.ToString().Trim();
                            string date = tbl.Cell(rowIndex, 4).Value.ToString().Trim();
                            string timeIn = tbl.Cell(rowIndex, 5).Value.ToString().Trim();
                            string timeOut = tbl.Cell(rowIndex, 6).Value.ToString().Trim();
                            if (timeIn == timeOut)
                            {
                                rowError++;
                                errorDetail += Environment.NewLine + $"+ Giờ vào và giờ ra giống nhau: {timeIn} - {timeOut}.";
                            }
                            
                            string verifyMode = "4";
                            string workCode = "1";
                            string error = "";
                            bool success = false;
                            // add log in
                            if (timeIn != "")
                            {
                                string strTimeIn = date + " " + timeIn;
                                success = AddAttendanceLog(context, listEmployees, empATID, device.SerialNumber, companyIndex, strTimeIn, verifyMode, "0", workCode, userName, now, arrFormat, ref error);
                                if (success == true)
                                {
                                    rowSuccess++;
                                }
                                else
                                {
                                    rowError++;
                                    errorDetail += error;
                                }
                            }

                            if (timeOut != "")
                            {
                                string strTimeOut = date + " " + timeOut;
                                success = AddAttendanceLog(context, listEmployees, empATID, device.SerialNumber, companyIndex, strTimeOut, verifyMode, "1", workCode, userName, now, arrFormat, ref error);
                                if (success == true)
                                {
                                    rowSuccess++;
                                }
                                else
                                {
                                    rowError++;
                                    errorDetail += error;
                                }
                            }
                        }
                        result += Environment.NewLine + $"- Số dòng thành công: {rowSuccess}";
                        result += Environment.NewLine + $"- Số dòng thất bại: {rowError}";
                        result += Environment.NewLine + (errorDetail == "" ? "" : ($"- Chi tiết lỗi: {errorDetail}"));
                    }
                    catch(Exception ex)
                    {
                        result += Environment.NewLine + $"-Lỗi không xác định: {ex.Message}.";
                    }
                   
                }
                else
                {
                    result += Environment.NewLine + "-ERR: File không đúng định dạng.";
                    continue;
                }
            }

            return result;
        }

        private bool AddAttendanceLog(EPAD_Context pContext, List<HR_User> pListEmp, string pEmpATID, string pSerial, int pCompanyIndex, string pStrCheckTime, string pVerifyMode, string pInOutMode, string pWorkCode,
            string pUserName,DateTime pNow,string[] pFormatConvert,ref string error)
        {
            var employee = pListEmp.Where(t => t.EmployeeATID == pEmpATID).FirstOrDefault();
            if (employee == null)
            {
                error += Environment.NewLine + $"+ MCC không tồn tại: {pEmpATID}.";
                return false;
            }

            DateTime? checkTime = null;
            try
            {
                checkTime = DateTime.ParseExact(pStrCheckTime, pFormatConvert, new CultureInfo("en-US"), DateTimeStyles.AssumeLocal);
            }
            catch (Exception ex)
            {
                error += Environment.NewLine + $"+ Sai định dạng ngày tháng: {pStrCheckTime}.";
                return false;
            }
            short verifyMode = 0;
            short inOutMode = 0;
            short workCode = 0;
            if (short.TryParse(pVerifyMode, out verifyMode) == false)
            {
                error += Environment.NewLine + $"+ Verify mode sai định dạng: {pVerifyMode}.";
                return false;
            }
            if (short.TryParse(pInOutMode, out inOutMode) == false)
            {
                error += Environment.NewLine + $"+ In out mode sai định dạng: {pInOutMode}.";
                return false;
            }
            if (short.TryParse(pWorkCode, out workCode) == false)
            {
                error += Environment.NewLine + $"+ Work code sai định dạng: {pWorkCode}.";
                return false;
            }

            var device = pContext.IC_Device.FirstOrDefault(x => x.SerialNumber.ToLower() == pSerial.ToLower());
            var attendanceLog = new IC_AttendanceLog();
            try
            {
                attendanceLog.EmployeeATID = pEmpATID;
                attendanceLog.SerialNumber = pSerial;
                attendanceLog.DeviceId = device.DeviceId;
                attendanceLog.CompanyIndex = pCompanyIndex;
                attendanceLog.CheckTime = checkTime.Value;
                attendanceLog.VerifyMode = verifyMode;
                attendanceLog.InOutMode = inOutMode;
                attendanceLog.WorkCode = workCode;
                attendanceLog.Reserve1 = 0;
                attendanceLog.Function = "";
                attendanceLog.UpdatedDate = pNow;
                attendanceLog.UpdatedUser = pUserName;

                pContext.IC_AttendanceLog.Add(attendanceLog);
                try
                {
                    pContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    pContext.IC_AttendanceLog.Remove(attendanceLog);
                    if (ex.ToString().Contains("Violation of PRIMARY KEY constraint") == false)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                error += Environment.NewLine + $"+ {ex.ToString()}.";
                return false;
            }
            return true;
        }

    }
}
