using EPAD_Data;
using EPAD_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class Employee_Shift_IntegrateLogic : IEmployee_Shift_IntegrateLogic
    {
        private string mLinkECMSApi;
        private string mCommunicateToken;
        private readonly ILogger _logger;
        private IConfiguration _Configuration;
        private readonly EPAD_Context _context;
        private IIC_HistoryTrackingIntegrateLogic _iC_HistoryTrackingIntegrateLogic;

        public Employee_Shift_IntegrateLogic(EPAD_Context context, ILogger<Employee_Shift_IntegrateLogic> logger, IConfiguration configuration,
              IIC_HistoryTrackingIntegrateLogic iC_HistoryTrackingIntegrateLogic)
        {
            _context = context;
            _logger = logger;
            _Configuration = configuration;
            mLinkECMSApi = _Configuration.GetValue<string>("ECMSApi");
            mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            _iC_HistoryTrackingIntegrateLogic = iC_HistoryTrackingIntegrateLogic;
        }

        public async Task<List<IC_Shift>> GetAllShiftsAsync()
        {
            return await _context.IC_Shift.ToListAsync();
        }

        public async Task UpdateEmployeeShiftsAsync(List<IC_Employee_Shift> request, List<IC_Shift> shifts)
        {
            try
            {
                var history = new IC_HistoryTrackingIntegrate();
                history.JobName = "AutoGetOVNEmployeeShift IC_Employee_Shift";

                var listEmployeeShifts = await _context.IC_Employee_Shift.ToListAsync();

                // Delete Shifts are not available
                var deletedEmployeeShifts = listEmployeeShifts.Except(request);//, new EmployeeATIDEqualityComparer());
                if (deletedEmployeeShifts != null && deletedEmployeeShifts.Any())
                {
                    _context.IC_Employee_Shift.RemoveRange(deletedEmployeeShifts);
                    history.DataDelete = (short)deletedEmployeeShifts.Count();
                }

                // Insert new shifts
                var newEmployeeShifts = request.Except(listEmployeeShifts);//, new EmployeeATIDEqualityComparer());
                if (newEmployeeShifts != null && newEmployeeShifts.Any())
                {
                    await _context.IC_Employee_Shift.AddRangeAsync(newEmployeeShifts);
                    history.DataNew = (short)newEmployeeShifts.Count();
                }

                // Update shifts have existed
                var modifiedEmployeeShifts = request.Except(newEmployeeShifts);//, new EmployeeATIDEqualityComparer());
                if (modifiedEmployeeShifts != null && modifiedEmployeeShifts.Any())
                {
                    foreach (var es in modifiedEmployeeShifts)
                    {
                        var existedES = listEmployeeShifts.FirstOrDefault(s => s.EmployeeATID.Equals(es.EmployeeATID, StringComparison.OrdinalIgnoreCase));
                        if (existedES != null)
                        {
                            existedES.IC_ShiftId = es.IC_ShiftId;
                            existedES.UpdatedDate = DateTime.Now;
                            _context.Update(existedES);
                            history.DataUpdate = history.DataUpdate++;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                await _iC_HistoryTrackingIntegrateLogic.AddHistoryTrackingIntegrate(history);
                //Send Employee Shift to ECMS
                if (!string.IsNullOrEmpty(mLinkECMSApi))
                {
                    var tasks = new List<Task>
                    {
                        SendShiftToECMSAPIAsync(shifts)
                    };
                    if (request.Any())
                    {
                        _logger.LogError($"request.Count() {DateTime.Now} {request.Count()}");
                        if (request.Count() > 1000)
                        {
                            //var childTask = new List<Task>();
                            for (int i = 0; i < request.Count; i++)
                            {
                                var newRequest = request.Skip(i).Take(2000).ToList();
                                var task = new Task(async () =>
                                {
                                    await SendEmployeeShiftToECMSAPIAsync(newRequest);
                                });
                                task.Start();
                                _logger.LogError($"newRequest {i} {DateTime.Now} {newRequest.Count()}");
                                await Task.Delay(100000);
                                i += 1000;
                            }
                        }
                        else
                        {
                            new List<Task>().Add(SendEmployeeShiftToECMSAPIAsync(request));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateEmployeeShiftsAsync {DateTime.Now} {ex}");
            }
        }

        private async Task SendShiftToECMSAPIAsync(List<IC_Shift> requestData)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/TA_Shift/ShiftIntegrateFromePAD");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendShiftToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
        }

        private async Task SendEmployeeShiftToECMSAPIAsync(List<IC_Employee_Shift> requestData)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/CM_EmployeeShift/SyncEmployeeShiftFromePAD");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                await client.SendAsync(request);
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendEmployeeShiftToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
        }

        private class ShiftNameEqualityComparer : IEqualityComparer<IC_Shift>
        {
            public bool Equals(IC_Shift x, IC_Shift y)
            {
                return string.Equals
                (
                    x.Name,
                    y.Name,
                    StringComparison.OrdinalIgnoreCase
                );
                //&& Equals
                //(
                //    x.ShiftDate,
                //    y.ShiftDate
                //);
            }

            public int GetHashCode(IC_Shift obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private class EmployeeATIDEqualityComparer : IEqualityComparer<IC_Employee_Shift>
        {
            public bool Equals(IC_Employee_Shift x, IC_Employee_Shift y)
            {
                return string.Equals
                (
                    x.EmployeeATID,
                    y.EmployeeATID,
                    StringComparison.OrdinalIgnoreCase
                )
                && Equals
                (
                    x.IC_Shift.ShiftDate,
                    y.IC_Shift.ShiftDate
                );
            }

            public int GetHashCode(IC_Employee_Shift obj)
            {
                return obj.EmployeeATID.GetHashCode();
            }
        }
    }

    public interface IEmployee_Shift_IntegrateLogic
    {
        Task<List<IC_Shift>> GetAllShiftsAsync();
        Task UpdateEmployeeShiftsAsync(List<IC_Employee_Shift> request, List<IC_Shift> shifts);
    }
}
