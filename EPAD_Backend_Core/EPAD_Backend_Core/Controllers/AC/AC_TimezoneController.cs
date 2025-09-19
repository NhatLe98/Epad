using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Extensions;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_Timezone/[action]")]
    [ApiController]
    public class AC_TimezoneController : ApiControllerBase
    {
        private readonly IAC_TimezoneService _IAC_TimezoneService;
        private IMemoryCache cache;
        private EPAD_Context context;
        private readonly IHostingEnvironment _hostingEnvironment;
        public AC_TimezoneController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _IAC_TimezoneService = TryResolve<IAC_TimezoneService>();
            cache = TryResolve<IMemoryCache>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
        }

        [Authorize]
        [ActionName("GetAllTimezone")]
        [HttpGet]
        public IActionResult GetAllTimezone()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            var areaList = context.AC_TimeZone.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            dep = from area in areaList
                  orderby area.Name
                  select new
                  {
                      value = area.UID,
                      label = area.Name
                  };
            result = Ok(dep);

            return result;
        }


        [Authorize]
        [ActionName("GetAllTimezoneUID")]
        [HttpGet]
        public IActionResult GetAllTimezoneUID()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            var areaList = context.AC_TimeZone.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            dep = from area in areaList
                  orderby area.Name
                  select new
                  {
                      UID = area.UIDIndex,
                      Name = area.Name
                  };
            result = Ok(dep);

            return result;
        }


        [Authorize]
        [ActionName("GetTimezoneAtPage")]
        [HttpGet]
        public IActionResult GetTimezoneAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_TimezoneService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetAllTimezoneAtPage")]
        [HttpGet]
        public IActionResult GetAllTimezoneAtPage()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_TimezoneService.GetAllDataTimezoneReturn(user.CompanyIndex);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetTimezoneByID")]
        [HttpGet]
        public IActionResult GetTimezoneByID(int UID)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var checkData = _IAC_TimezoneService.GetTimezoneByID(UID);
            return ApiOk(checkData);
        }

        [Authorize]
        [ActionName("AddTimezone")]
        [HttpPost]
        public IActionResult AddTimezone([FromBody] AC_TimeZone param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var checkName = context.AC_TimeZone.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null)
            {
                return BadRequest("ExistTimezoneName");
            }

            var timezone = new AC_TimeZone();
            timezone.Name = param.Name;
            timezone.UpdatedDate = DateTime.Now;
            timezone.CompanyIndex = user.CompanyIndex;
            timezone.Description = param.Description;

            var listName = new List<string>() { "Name", "UID", "UpdatedDate", "CompanyIndex", "UIDIndex", "Description" };
            var checkNull = false;
            var checkValue = false;
            var propertyName = String.Empty;

            var characterRemove = new List<string>() { "Start1", "End1", "Start2", "End2", "Start3", "End3", };
            var dayOfWeekMinus = new List<string>() { "Mon", "Sun", "Tues", "Wed", "Thurs", "Fri", "Sat", "Holiday1", "Holiday2", "Holiday3" };
            var dayError = new List<Error>()
            {
                new Error(){Name = "Timezoneisnotvalid", DayOfWeek = new List<string>() },
                new Error(){Name = "Timezoneisnull", DayOfWeek = new List<string>() },
                new Error(){Name = "TimezoneRangeisinvalid", DayOfWeek = new List<string>() }
            };
             
            foreach (var prop in param.GetType().GetProperties())
            {
                if (!listName.Contains(prop.Name))
                {
                    var value = prop.GetValue(param, null);
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        SetObjectProperty(prop.Name, value.ToString().Replace(":", ""), timezone);

                        if (prop.Name.Contains("Start"))
                        {
                            var proCompare = prop.Name.Replace("Start", "End");
                            var pi = param.GetType().GetProperty(proCompare);
                            var check = pi.GetValue(param, null);
                            if (check == null)
                            {
                                checkNull = true;
                                propertyName = prop.Name;
                                var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnull");
                                err.DayOfWeek.Add(prop.Name);
                                //break;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(check.ToString()))
                                {
                                    if (DateTime.Parse(value.ToString()) >= DateTime.Parse(check.ToString()))
                                    {
                                        checkValue = true;
                                        propertyName = prop.Name;
                                        var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnotvalid");
                                        err.DayOfWeek.Add(prop.Name);
                                       
                                        //break;
                                    }
                                }
                                else
                                {
                                    checkNull = true;
                                    propertyName = prop.Name;
                                    var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnull");
                                    err.DayOfWeek.Add(prop.Name);
                                    //break;
                                }
                            }
                        }
                        else if (prop.Name.Contains("End"))
                        {
                            var proCompare = prop.Name.Replace("End", "Start");
                            var pi = param.GetType().GetProperty(proCompare);
                            var check = pi.GetValue(param, null);
                            if (check == null)
                            {
                                checkNull = true;
                                propertyName = prop.Name;
                                var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnull");
                                err.DayOfWeek.Add(prop.Name);
                                //break;
                            }
                        }
                    }
                }
            }
            var lst = new List<DayOfWeekGroup>();

            if (!checkNull && !checkValue)
            {
                foreach (var item in dayOfWeekMinus)
                {
                    lst.Add(new DayOfWeekGroup { Name = item, DayGroups = new List<DayGroup>(), TimeRanges = new List<TimeRangeParam>() });
                }

                foreach (var prop in param.GetType().GetProperties())
                {
                    if (!listName.Contains(prop.Name))
                    {
                        var value = prop.GetValue(param, null);
                        if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        {
                            var getLst = lst.FirstOrDefault(x => prop.Name.Contains(x.Name));
                            var daygr = new DayGroup()
                            {
                                Name = prop.Name,
                                Value = value.ToString()
                            };
                            getLst.DayGroups.Add(daygr);
                        }
                    }
                }
                lst = lst.Where(x => x.DayGroups.Count() > 1).ToList();

                foreach (var item in lst)
                {
                    var group1 = item.DayGroups.Where(x => x.Name.Contains("1") && (!x.Name.Contains("Holiday") || x.Name.Contains("Start1") || x.Name.Contains("End1"))).ToList();
                    var group2 = item.DayGroups.Where(x => x.Name.Contains("2") && (!x.Name.Contains("Holiday") || x.Name.Contains("Start2") || x.Name.Contains("End2"))).ToList();
                    var group3 = item.DayGroups.Where(x => x.Name.Contains("3") && (!x.Name.Contains("Holiday") || x.Name.Contains("Start3") || x.Name.Contains("End3"))).ToList();

                    if (group1 != null && group1.Count > 0)
                    {
                        var start = group1.FirstOrDefault(x => x.Name.Contains("Start"));
                        var end = group1.FirstOrDefault(x => x.Name.Contains("End"));
                        item.TimeRanges.Add(new TimeRangeParam
                        {
                            DateStart = DateTime.Parse(start.Value),
                            DateEnd = DateTime.Parse(end.Value)
                        });
                    }
                    if (group2 != null && group2.Count > 0)
                    {
                        var start = group2.FirstOrDefault(x => x.Name.Contains("Start"));
                        var end = group2.FirstOrDefault(x => x.Name.Contains("End"));
                        item.TimeRanges.Add(new TimeRangeParam
                        {
                            DateStart = DateTime.Parse(start.Value),
                            DateEnd = DateTime.Parse(end.Value)
                        });
                    }
                    if (group3 != null && group3.Count > 0)
                    {
                        var start = group3.FirstOrDefault(x => x.Name.Contains("Start"));
                        var end = group3.FirstOrDefault(x => x.Name.Contains("End"));
                        item.TimeRanges.Add(new TimeRangeParam
                        {
                            DateStart = DateTime.Parse(start.Value),
                            DateEnd = DateTime.Parse(end.Value)
                        });
                    }
                }
            }

            lst = lst.Where(x => x.TimeRanges.Count > 1).ToList();
            var checkRange = false;

            foreach (var item in lst)
            {
                if (item.TimeRanges.Count == 2)
                {
                    var s = item.TimeRanges[0];
                    var check = s.AreTwoDateTimeRangesOverlapping(item.TimeRanges[1]);
                    if (check)
                    {
                        propertyName = item.Name;
                        checkRange = true;
                        var err = dayError.FirstOrDefault(d => d.Name == "TimezoneRangeisinvalid");
                        err.DayOfWeek.Add(propertyName);
                    }
                }
                else
                {
                    var s = item.TimeRanges[0];
                    var s1 = item.TimeRanges[1];
                    var s2 = item.TimeRanges[2];

                    var lsts = new List<TimeRangeParam>();
                    lsts.Add(s1);
                    lsts.Add(s2);
                    var check = s.AreManyDateTimeRangesOverlapping(lsts);
                    if (check)
                    {
                        propertyName = item.Name;
                        checkRange = true;
                        var err = dayError.FirstOrDefault(d => d.Name == "TimezoneRangeisinvalid");
                        err.DayOfWeek.Add(propertyName);
                    }
                }
            }

            if (checkNull || checkValue || checkRange)
            {

                var dayOfWeek = new List<string>() { "Monday", "Sunday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Holiday1", "Holiday2", "Holiday3" };
                var dayReturn = string.Empty;
                dayError = dayError.Where(x => x.DayOfWeek.Count > 0).ToList();
                for(var i= 0; i < dayError.Count; i++)
                {
                    for (var j = 0; j < dayError[i].DayOfWeek.Count; j++)
                    {
                        foreach (var item in characterRemove)
                        {
                            dayError[i].DayOfWeek[j] = dayError[i].DayOfWeek[j].Replace(item, "");
                            
                        }
                        dayError[i].DayOfWeek[j] = dayOfWeek.FirstOrDefault(x => x.Contains(dayError[i].DayOfWeek[j]));

                    }

                    dayError[i].DayOfWeek = dayError[i].DayOfWeek.Distinct().ToList();
                }
               


                //if (checkRange)
                //{
                //    return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "TimezoneRangeisinvalid", responseData = dayReturn });
                //}

                //if (checkNull)
                //{
                //    return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "Timezoneisnull", responseData = dayReturn });
                //}
                //else
                //{
                    return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "Timezoneisnotvalid", responseData = dayError });
                //}
            }

            var allTimezone = _DbContext.AC_TimeZone.Where(x => x.UIDIndex != null).Select(x => int.Parse(x.UIDIndex)).ToList();
            var uid = 2;
            do
            {

                if(uid > 50) { break; }
                if (!allTimezone.Contains(uid))
                {
                    timezone.UIDIndex = uid.ToString();
                    break;
                }

                uid += 3;
            } while (true);
            context.AC_TimeZone.Add(timezone);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateTimezone")]
        [HttpPost]
        public IActionResult UpdateTimezone([FromBody] AC_TimeZone param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var timezone = context.AC_TimeZone.Where(t => t.UID == param.UID).FirstOrDefault();
            var checkName = context.AC_TimeZone.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null && checkName.UID != timezone.UID)
            {
                return BadRequest("ExistTimezoneName");
            }


            timezone.Name = param.Name;
            timezone.UpdatedDate = DateTime.Now;
            timezone.CompanyIndex = user.CompanyIndex;
            timezone.Description = param.Description;

            var listName = new List<string>() { "Name", "UID", "UpdatedDate", "CompanyIndex", "UIDIndex", "Description" };
            var checkNull = false;
            var checkValue = false;
            var propertyName = String.Empty;

            var characterRemove = new List<string>() { "Start1", "End1", "Start2", "End2", "Start3", "End3", };
            var dayOfWeekMinus = new List<string>() { "Mon", "Sun", "Tues", "Wed", "Thurs", "Fri", "Sat", "Holiday1", "Holiday2", "Holiday3" };
            var dayError = new List<Error>()
            {
                new Error(){Name = "Timezoneisnotvalid", DayOfWeek = new List<string>() },
                new Error(){Name = "Timezoneisnull", DayOfWeek = new List<string>() },
                new Error(){Name = "TimezoneRangeisinvalid", DayOfWeek = new List<string>() }
            };
            foreach (var prop in param.GetType().GetProperties())
            {
                if (!listName.Contains(prop.Name))
                {
                    var value = prop.GetValue(param, null);
                    SetObjectProperty(prop.Name, "", timezone);
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        SetObjectProperty(prop.Name, value.ToString().Replace(":", ""), timezone);

                        if (prop.Name.Contains("Start"))
                        {
                            var proCompare = prop.Name.Replace("Start", "End");
                            var pi = param.GetType().GetProperty(proCompare);
                            var check = pi.GetValue(param, null);
                            if (check == null)
                            {
                                checkNull = true;
                                propertyName = prop.Name;
                                var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnull");
                                err.DayOfWeek.Add(prop.Name);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(check.ToString()))
                                {
                                    if (DateTime.Parse(value.ToString()) >= DateTime.Parse(check.ToString()))
                                    {
                                        checkValue = true;
                                        propertyName = prop.Name;
                                        var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnotvalid");
                                        err.DayOfWeek.Add(prop.Name);
                                    }
                                }
                                else
                                {
                                    checkNull = true;
                                    propertyName = prop.Name;
                                    var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnull");
                                    err.DayOfWeek.Add(prop.Name);
                                }
                            }
                        }
                        else if (prop.Name.Contains("End"))
                        {
                            var proCompare = prop.Name.Replace("End", "Start");
                            var pi = param.GetType().GetProperty(proCompare);
                            var check = pi.GetValue(param, null);
                            if (check == null || string.IsNullOrEmpty(check.ToString()))
                            {
                                checkNull = true;
                                propertyName = prop.Name;
                                var err = dayError.FirstOrDefault(d => d.Name == "Timezoneisnull");
                                err.DayOfWeek.Add(prop.Name);
                            }
                        }



                    }
                }
            }
            var lst = new List<DayOfWeekGroup>();

            if (!checkNull && !checkValue)
            {
                foreach (var item in dayOfWeekMinus)
                {
                    lst.Add(new DayOfWeekGroup { Name = item, DayGroups = new List<DayGroup>(), TimeRanges = new List<TimeRangeParam>() });
                }

                foreach (var prop in param.GetType().GetProperties())
                {
                    if (!listName.Contains(prop.Name))
                    {
                        var value = prop.GetValue(param, null);
                        if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        {
                            var getLst = lst.FirstOrDefault(x => prop.Name.Contains(x.Name));
                            var daygr = new DayGroup()
                            {
                                Name = prop.Name,
                                Value = value.ToString()
                            };
                            getLst.DayGroups.Add(daygr);
                        }
                    }
                }
                lst = lst.Where(x => x.DayGroups.Count() > 1).ToList();

                foreach (var item in lst)
                {
                    var group1 = item.DayGroups.Where(x => x.Name.Contains("1") && (!x.Name.Contains("Holiday") || x.Name.Contains("Start1") || x.Name.Contains("End1"))).ToList();
                    var group2 = item.DayGroups.Where(x => x.Name.Contains("2") && (!x.Name.Contains("Holiday") || x.Name.Contains("Start2") || x.Name.Contains("End2"))).ToList();
                    var group3 = item.DayGroups.Where(x => x.Name.Contains("3") && (!x.Name.Contains("Holiday") || x.Name.Contains("Start3") || x.Name.Contains("End3"))).ToList();
                    if (group1 != null && group1.Count > 0)
                    {
                        var start = group1.FirstOrDefault(x => x.Name.Contains("Start"));
                        var end = group1.FirstOrDefault(x => x.Name.Contains("End"));
                        item.TimeRanges.Add(new TimeRangeParam
                        {
                            DateStart = DateTime.Parse(start.Value),
                            DateEnd = DateTime.Parse(end.Value)
                        });
                    }
                    if (group2 != null && group2.Count > 0)
                    {
                        var start = group2.FirstOrDefault(x => x.Name.Contains("Start"));
                        var end = group2.FirstOrDefault(x => x.Name.Contains("End"));
                        item.TimeRanges.Add(new TimeRangeParam
                        {
                            DateStart = DateTime.Parse(start.Value),
                            DateEnd = DateTime.Parse(end.Value)
                        });
                    }
                    if (group3 != null && group3.Count > 0)
                    {
                        var start = group3.FirstOrDefault(x => x.Name.Contains("Start"));
                        var end = group3.FirstOrDefault(x => x.Name.Contains("End"));
                        item.TimeRanges.Add(new TimeRangeParam
                        {
                            DateStart = DateTime.Parse(start.Value),
                            DateEnd = DateTime.Parse(end.Value)
                        });
                    }
                }
            }

            lst = lst.Where(x => x.TimeRanges.Count > 1).ToList();
            var checkRange = false;

            foreach (var item in lst)
            {
                if (item.TimeRanges.Count == 2)
                {
                    var s = item.TimeRanges[0];
                    var check = s.AreTwoDateTimeRangesOverlapping(item.TimeRanges[1]);
                    if (check)
                    {
                        propertyName = item.Name;
                        checkRange = true;
                        var err = dayError.FirstOrDefault(d => d.Name == "TimezoneRangeisinvalid");
                        err.DayOfWeek.Add(propertyName);
                    }
                }
                else
                {
                    var s = item.TimeRanges[0];
                    var s1 = item.TimeRanges[1];
                    var s2 = item.TimeRanges[2];

                    var lsts = new List<TimeRangeParam>();
                    lsts.Add(s1);
                    lsts.Add(s2);
                    var check = s.AreManyDateTimeRangesOverlapping(lsts);
                    if (check)
                    {
                        propertyName = item.Name;
                        checkRange = true;
                        var err = dayError.FirstOrDefault(d => d.Name == "TimezoneRangeisinvalid");
                        err.DayOfWeek.Add(propertyName);
                    }
                }
            }

            if (checkNull || checkValue || checkRange)
            {

                var dayOfWeek = new List<string>() { "Monday", "Sunday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Holiday1", "Holiday2", "Holiday3" };
                var dayReturn = string.Empty;
                dayError = dayError.Where(x => x.DayOfWeek.Count > 0).ToList();
                for (var i = 0; i < dayError.Count; i++)
                {
                    for (var j = 0; j < dayError[i].DayOfWeek.Count; j++)
                    {
                        foreach (var item in characterRemove)
                        {
                            dayError[i].DayOfWeek[j] = dayError[i].DayOfWeek[j].Replace(item, "");

                        }
                        dayError[i].DayOfWeek[j] = dayOfWeek.FirstOrDefault(x => x.Contains(dayError[i].DayOfWeek[j]));
                    }
                    dayError[i].DayOfWeek = dayError[i].DayOfWeek.Distinct().ToList();
                }

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "Timezoneisnotvalid", responseData = dayError });
            }

            if (string.IsNullOrEmpty(timezone.UIDIndex))
            {
                var allTimezone = _DbContext.AC_TimeZone.Where(x => x.UIDIndex != null).Select(x => int.Parse(x.UIDIndex)).ToList();
                var uid = 2;
                do
                {

                    if (uid > 50) { break; }
                    if (!allTimezone.Contains(uid))
                    {
                        timezone.UIDIndex = uid.ToString();
                        break;
                    }

                    uid += 3;
                } while (true);
            }
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("ExportInfoSyncAcUser")]
        [HttpGet]
        public IActionResult ExportInfoSyncAcUser()
        {
#if !DEBUG
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_EmployeeSync.xlsx");
            var timezoneLst = _DbContext.AC_TimeZone.Select(x => x.Name).OrderByDescending(x => x).ToList();
            var areaLst = _DbContext.AC_Area.Select(x => x.Name).OrderByDescending(x => x).ToList();
            var doorLst = _DbContext.AC_Door.Select(x => x.Name).OrderByDescending(x => x).ToList();

            using (var workbook = new XLWorkbook(folderDetails))
            {
                var worksheet = workbook.Worksheets;
                IXLWorksheet worksheet1;
                IXLWorksheet worksheet2;
                IXLWorksheet worksheet3;
                IXLWorksheet worksheet4;
                IXLWorksheet worksheet5;

                var w1 = worksheet.TryGetWorksheet("TimezoneData", out worksheet1);
                worksheet1.Cells().Clear();
                string startTimezoneCell = "A1";
                string endTimezoneCell = string.Empty;
                for (int i = 0; i < timezoneLst.Count; i++)
                {
                    if (i == (timezoneLst.Count - 1))
                    {
                        endTimezoneCell = "A" + (i + 1);
                    }
                    worksheet1.Cell("A" + (i + 1)).Value = timezoneLst[i];
                }

                var w2 = worksheet.TryGetWorksheet("AreaData", out worksheet2);
                worksheet2.Cells().Clear();
                string startAreaCell = "A1";
                string endAreaCell = string.Empty;
                for (int i = 0; i < areaLst.Count; i++)
                {
                    if (i == (areaLst.Count - 1))
                    {
                        endAreaCell = "A" + (i + 1);
                    }
                    worksheet2.Cell("A" + (i + 1)).Value = areaLst[i];
                }

                var w3 = worksheet.TryGetWorksheet("DoorData", out worksheet3);
                worksheet3.Cells().Clear();
                string startDoorCell = "A1";
                string endDoorCell = string.Empty;
                for (int i = 0; i < doorLst.Count; i++)
                {
                    if (i == (doorLst.Count - 1))
                    {
                        endDoorCell = "A" + (i + 1);
                    }
                    worksheet3.Cell("A" + (i + 1)).Value = doorLst[i];
                }

                var w = worksheet.TryGetWorksheet("Data", out worksheet5);
                if (!string.IsNullOrWhiteSpace(startTimezoneCell) && !string.IsNullOrWhiteSpace(endTimezoneCell))
                    worksheet5.Range("E2:E10003").SetDataValidation().List(worksheet1.Range(startTimezoneCell + ":" + endTimezoneCell), true);
                if (!string.IsNullOrWhiteSpace(startAreaCell) && !string.IsNullOrWhiteSpace(endAreaCell))
                    worksheet5.Range("F2:F10003").SetDataValidation().List(worksheet2.Range(startAreaCell + ":" + endAreaCell), true);
                if (!string.IsNullOrWhiteSpace(startDoorCell) && !string.IsNullOrWhiteSpace(endDoorCell))
                    worksheet5.Range("G2:G10003").SetDataValidation().List(worksheet3.Range(startDoorCell + ":" + endDoorCell), true);
                workbook.Save();
            }
#endif
            return ApiOk();
        }

        [Authorize]
        [ActionName("DeleteTimezone")]
        [HttpPost]
        public IActionResult DeleteTimezone([FromBody] List<AC_TimeZone> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var deleteData = new AC_TimeZone();
            foreach (var param in lsparam)
            {
                 deleteData = context.AC_TimeZone.Where(t => t.CompanyIndex == user.CompanyIndex && t.UID == param.UID).FirstOrDefault();

                if (deleteData == null)
                {
                    return NotFound("TimezoneNotExist");
                }
                else
                {
                    context.AC_TimeZone.Remove(deleteData);
                }
            }
            context.SaveChanges();

            result = Ok(deleteData);
            return result;
        }

        private void SetObjectProperty(string propertyName, string value, object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            // make sure object has the property we are after
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        public class DayOfWeekGroup
        {
            public string Name { get; set; }
            public List<DayGroup> DayGroups { get; set; }
            public List<TimeRangeParam> TimeRanges { get; set; }
        }

        public class DayGroup
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public class Error
        {
            public string Name { get; set;}
            public List<string> DayOfWeek { get; set; }
        }

    }

}
