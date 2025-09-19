using EPAD_Backend_Core.WebUtilitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EPAD_Data.Models;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Logic.SendMail;
using EPAD_Common;
using EPAD_Common.Enums;
using EPAD_Data.Entities;
using EPAD_Logic;
using EPAD_Common.Types;
using static EPAD_Data.Models.IC_SignalRDTO;
using EPAD_Logic.MainProcess;
using EPAD_Backend_Core.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using EPAD_Backend_Core.ApiClient;
using EPAD_Services.Interface;
using DeviceType = EPAD_Common.Enums.DeviceType;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/SystemCommand/[action]")]
    [ApiController]
    public class IC_SystemCommandController : ApiControllerBase
    {
        private readonly EPAD_Context context;
        private IMemoryCache cache;
        private readonly IEmailProvider emailProvider;
        private ezHR_Context otherContext;
        private IIC_SystemCommandLogic _IIC_SystemCommandLogic;
        private IIC_SignalRLogic _iC_SignalRLogic;
        private IIC_CachingLogic _iC_CachingLogic;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IIC_AuditLogic _iC_AuditLogic;
        private readonly PushNotificationClient pushNotificationClient;
        private readonly IIC_SystemCommandService _IC_SystemCommandService;
        

        public IC_SystemCommandController(IServiceProvider provider,
            ILoggerFactory loggerFactory,
            IConfiguration configuration) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            otherContext = TryResolve<ezHR_Context>();
            cache = TryResolve<IMemoryCache>();
            emailProvider = TryResolve<IEmailProvider>();
            _IIC_SystemCommandLogic = TryResolve<IIC_SystemCommandLogic>();
            _iC_SignalRLogic = TryResolve<IIC_SignalRLogic>();
            _iC_CachingLogic = TryResolve<IIC_CachingLogic>();
            _iC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _logger = loggerFactory.CreateLogger<IC_SystemCommandController>();
            _configuration = configuration;
            pushNotificationClient = PushNotificationClient.GetInstance(cache);
            _IC_SystemCommandService = TryResolve<IIC_SystemCommandService>();
        }

        [Authorize]
        [ActionName("AddSystemCommand")]
        [HttpPost]
        public IActionResult AddSystemCommand([FromBody] SystemCommandParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (SystemCommandParam)StringHelper.RemoveWhiteSpace(param);
            if (param.SerialNumber == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            IC_SystemCommand command = new IC_SystemCommand();
            command.SerialNumber = param.SerialNumber;
            command.CommandName = param.CommandName;
            command.Command = param.Command;
            command.Params = param.Params;

            command.EmployeeATIDs = param.EmployeeATIDs;
            command.RequestedTime = param.RequestedTime;
            command.ExcutedTime = param.ExcutedTime;
            command.Excuted = false;

            command.CompanyIndex = user.CompanyIndex;
            command.CreatedDate = DateTime.Now;
            command.UpdatedDate = DateTime.Now;
            command.UpdatedUser = user.UserName;

            context.IC_SystemCommand.Add(command);
            context.SaveChanges();

            result = Ok();
            return result;
        }

        internal static void PostPushNotification()
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [ActionName("DeleteSystemCommand")]
        [HttpPost]
        public IActionResult DeleteSystemCommand([FromBody] SystemCommandParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_SystemCommand deleteData = context.IC_SystemCommand.Where(t => t.Index == param.Index).FirstOrDefault();
            if (deleteData == null)
            {
                return NotFound("ServiceNotExist");
            }

            context.IC_SystemCommand.Remove(deleteData);
            context.SaveChanges();


            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("GetSystemCommandNeedExecute")]
        [HttpPost]
        public IActionResult GetSystemCommandNeedExecute(SerialNumberInfos lsSerialNumber)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var commandsNeedExecute = new List<CommandResult>();
            var now = DateTime.Now;

            try
            {
                if (lsSerialNumber != null && lsSerialNumber?.ServiceType != GlobalParams.ServiceType.SDKInterfaceService)
                {
                    _IIC_SystemCommandLogic.UpdateLastConnection(lsSerialNumber.ListSerialNumber, user.CompanyIndex);
                }

                commandsNeedExecute = user.GetSomeCommandsUnExecute(lsSerialNumber).Where(t => t.ID != null).ToList();

                if (commandsNeedExecute.Count < 1 && user.NeedCheckCommandDB)
                {
                    var take = lsSerialNumber?.ServiceType == GlobalParams.ServiceType.SDKInterfaceService
                            ? GlobalParams.COMMAND_NUMBER_RETURN_SDK : GlobalParams.COMMAND_NUMBER_RETURN_PUSH;

                    commandsNeedExecute = context.IC_SystemCommand
                        .Where(x => (x.Excuted == false || x.ExcutedTime == null)
                            && x.IsActive
                            && (lsSerialNumber.ListSerialNumber == null || lsSerialNumber.ListSerialNumber.Contains(x.SerialNumber)))
                        .Take(take)
                        .Select(cmd => new CommandResult
                        {
                            ID = cmd.Index.ToString(),
                            SerialNumber = cmd.SerialNumber,
                            Command = cmd.Command,
                            CreatedTime = cmd.CreatedDate ?? DateTime.Now,
                            ListUsers = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).ListUsers,
                            TimeZones = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).TimeZones,
                            AccHolidays = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).AccHolidays,
                            AccGroups = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).AccGroups,
                            TimeZone = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).TimeZone,
                            Group = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).Group,
                            ConnectionCode = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).ConnectionCode,
                            AutoOffSecond = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).AutoOffSecond,
                            Error = cmd.Error,
                            ExcutingServiceIndex = cmd.ExcutingServiceIndex,
                            GroupIndex = cmd.GroupIndex + "",
                            Status = "UnExecute",
                            IsOverwriteData = cmd.IsOverwriteData,
                        })
                        .ToList();

                    var serialNumberList = commandsNeedExecute.Select(x => x.SerialNumber).ToList();
                    var devices = context.IC_Device.Where(x => serialNumberList.Contains(x.SerialNumber)).ToList();

                    for (int i = 0; i < commandsNeedExecute.Count; i++)
                    {
                        var cmd = commandsNeedExecute[i];
                        var device = devices.FirstOrDefault(x => x.SerialNumber == cmd.SerialNumber);
                        cmd.Port = device?.Port ?? 0;
                        cmd.IPAddress = device?.IPAddress;
                        cmd.ConnectionCode = device?.ConnectionCode;
                        if (!string.IsNullOrEmpty(device?.DeviceModel))
                        {
                            int.TryParse(device.DeviceModel, out int deviceModel);
                            cmd.DeviceModel = deviceModel;
                        }
                    }
                    if(commandsNeedExecute.Count > 0)
                    {
                        user.ListCommands.AddRange(commandsNeedExecute);
                    }
                   
                }
                if (commandsNeedExecute.Count > 0)
                {
                    var listLogs = new List<RemoteProcessLogObject>();
                    var serialNumberList = commandsNeedExecute.Select(x => x.SerialNumber).ToList();
                    var devices = context.IC_Device.Where(x => serialNumberList.Contains(x.SerialNumber)).ToList();
                    for (int i = 0; i < commandsNeedExecute.Count; i++)
                    {
                        var cmd = commandsNeedExecute[i];
                        var device = devices.FirstOrDefault(x => x.SerialNumber == cmd.SerialNumber);
                        cmd.ConnectionCode = device?.ConnectionCode;
                        if (!string.IsNullOrEmpty(device?.DeviceModel))
                        {
                            int.TryParse(device.DeviceModel, out int deviceModel);
                            cmd.DeviceModel = deviceModel;
                        }
                    }

                    result = Ok(commandsNeedExecute);
                    var listCmdIndex = new List<int>();
                    for (int i = 0; i < commandsNeedExecute.Count; i++)
                    {
                        listCmdIndex.Add(int.Parse(commandsNeedExecute[i].ID));
                        var log = new RemoteProcessLogObject(user, commandsNeedExecute[i], "", CommandStatus.UnExecute.ToString(), now);
                        log.Action = "GetSystemCommandNeedExecute";
                        listLogs.Add(log);
                    }

                    //ghi log 
                    if (listLogs.Count > 0)
                    {
                        var mongoObject = new MongoDBHelper<RemoteProcessLogObject>("process_log", cache);
                        mongoObject.AddListDataToCollection(listLogs, true);
                    }
                }

                if (lsSerialNumber != null && lsSerialNumber.ListSerialNumber != null && lsSerialNumber.ListSerialNumber.Count > 0)
                {
                    var existedSerialList = context.IC_Device.Where(x => lsSerialNumber.ListSerialNumber.Contains(x.SerialNumber))?.Select(x => x.SerialNumber).ToList();
                    var newSerialNumberList = lsSerialNumber.ListSerialNumber.Where(x => !existedSerialList.Contains(x)).ToList();
                    var defaultPort = 4370;
                    foreach (var serial in newSerialNumberList)
                    {
                        context.IC_Device.Add(new IC_Device
                        {
                            SerialNumber = serial,
                            IPAddress = "1.1.1.1",
                            Port = defaultPort,
                            DeviceType = (short)DeviceType.CardAndFinger,
                            CompanyIndex = user.CompanyIndex,
                            CreatedDate = DateTime.Now,
                            AliasName = serial + "_AutoCreate",
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        });
                        defaultPort++;
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetSystemCommandNeedExecute {ex}");
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
                commandsNeedExecute = new List<CommandResult>();
            }
            return result;
        }

        [Authorize]
        [ActionName("UpdateCommandStatus")]
        [HttpPost]
        public IActionResult UpdateSystemCommandStatus([FromBody] List<CommandParam> listParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
            _IC_SystemCommandService.UpdateSystemCommandStatus(listParams, user, config, companyInfo);
            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("GetDeviceInfo")]
        [HttpPost]
        public IActionResult GetDeviceInfo([FromBody] List<string> listParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            DateTime now = DateTime.Now;
            List<CommandResult> listCommandResult = CommandProcess.CreateListCommands(context, listParams, CommandAction.GetDeviceInfo, "", now, now, null, false, GlobalParams.DevicePrivilege.SDKStandardRole);
            CommandProcess.CreateGroupCommand(context, cache, user.CompanyIndex, user.UserName, "GetDeviceInfo", "", listCommandResult, "");

            result = Ok();
            return result;
        }

        /// <summary>
        ///     Update status of 1 system command.
        /// </summary>
        /// <param name="systemCommandIndex">Index of systemCommand</param>
        /// <param name="firstUpdate">firstUpdate = true => return error if systemCommand.ExecutingTime != null.</param>
        /// <returns></returns>
        [Authorize]
        [ActionName("UpdateCommandExecuting")]
        [HttpPost]
        public async Task<IActionResult> UpdateCommandExecuting([FromQuery] int systemCommandIndex, [FromQuery] bool firstUpdate)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            bool foundCommandInDatabase = await context.IC_SystemCommand.AnyAsync(t => t.Index == systemCommandIndex && !t.Excuted && t.IsActive);
            if (!foundCommandInDatabase)
            {
                user.ListCommands.RemoveAll(x => x?.ID == systemCommandIndex.ToString());
                return NotFound();
            }

            string conn = _configuration.GetConnectionString("connectionString");
            using (var connection = new SqlConnection(conn))
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                        UPDATE IC_SystemCommand
                        SET ExcutingServiceIndex = @executingServiceIndex
	                        , ExcutedTime = GETDATE()
                        WHERE [INDEX] = @systemCommandIndex AND ([ExcutedTime] IS NULL OR (@firstUpdate = 0))
                    ";

                command.Parameters.Add("@firstUpdate", System.Data.SqlDbType.Int);
                command.Parameters["@firstUpdate"].Value = firstUpdate ? 1 : 0;

                command.Parameters.Add("@executingServiceIndex", System.Data.SqlDbType.Int);
                command.Parameters["@executingServiceIndex"].Value = user.Index;

                command.Parameters.Add("@systemCommandIndex", System.Data.SqlDbType.Int);
                command.Parameters["@systemCommandIndex"].Value = systemCommandIndex;

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    #region Add command status to IC_Audit
                    if (rowsAffected > 0)
                    {
                        command.CommandText = @"
                                    INSERT INTO IC_Audit ([UserName]
                                         , [Name]
                                         , [Description]
                                         , [DescriptionEn]
                                         , [TableName]
                                         , [PageName]
                                         , [State]
                                         , [IC_SystemCommandIndex]
                                         , [CompanyIndex]
                                         , [DateTime]
                                         , [Status]
                                    )
                                    SELECT TOP (1) [UserName]
                                       , [Name]
                                       , [Description]
                                       , [DescriptionEn]
                                       , [TableName]
                                       , [PageName]
                                       , [State] 
                                       , @systemCommandIndex
                                       , @companyIndex
                                       , GETDATE()
                                       , @status
                                    FROM IC_Audit
                                    WHERE IC_SystemCommandIndex = @systemCommandIndex
                                    ORDER BY [Index] DESC
                                ";
                        command.Parameters.Add("@companyIndex", System.Data.SqlDbType.Int);
                        command.Parameters["@companyIndex"].Value = user.CompanyIndex;
                        command.Parameters.Add("@status", System.Data.SqlDbType.VarChar, 100);
                        command.Parameters["@status"].Value = AuditStatus.Processing.ToString();

                        command.ExecuteNonQuery();
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    result = StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
                }
            }
            return result;
        }

        [Authorize]
        [ActionName("UpdateLastConnectionBySDK")]
        [HttpPost]
        public IActionResult UpdateLastConnectionBySDK(SerialNumberInfos lsSerialNumber)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                if (lsSerialNumber != null)
                {
                    _IIC_SystemCommandLogic.UpdateLastConnection(lsSerialNumber.ListSerialNumber, user.CompanyIndex);
                }
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("PostPage")]
        [HttpPost]
        public IActionResult PostPage([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                List<IC_SystemCommandDTO> listCommand = new List<IC_SystemCommandDTO>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });

                ListDTOModel<IC_SystemCommandDTO> listData = _IIC_SystemCommandLogic.GetPage(addedParams);
                DataGridClass dataGrid = new DataGridClass(listData.TotalCount, listData.Data); ;

                return Ok(dataGrid);

            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }
            return result;
        }
        [Authorize]
        [ActionName("GetPage")]
        [HttpGet]
        public IActionResult GetPage(string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                var addedParams = JsonConvert.DeserializeObject<List<AddedParam>>(filter);
                List<IC_SystemCommandDTO> listCommand = new List<IC_SystemCommandDTO>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                ListDTOModel<IC_SystemCommandDTO> listData = _IIC_SystemCommandLogic.GetPage(addedParams);
                DataGridClass dataGrid = new DataGridClass(listData.TotalCount, listData.Data); ;

                return Ok(dataGrid);

            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("DeleteList")]
        [HttpPost]
        public IActionResult DeleteList([FromBody] List<IC_SystemCommandDTO> listSystemCommand)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                _IIC_SystemCommandLogic.DeleteSystemCommandCacheAndDataBase(listSystemCommand);
                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("DeleteByIds")]
        [HttpPost]
        public IActionResult DeleteByIds([FromBody] List<string> listSystemCommandIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                var idxInt = new List<int>();
                listSystemCommandIndex.ForEach(i => idxInt.Add(int.Parse(i)));
                var listAuditToDelete = context.IC_Audit.Where(x => idxInt.Contains(x.IC_SystemCommandIndex ?? 0));
                var listCmdForDelete = context.IC_SystemCommand.Where(x => idxInt.Contains(x.Index)).ToList();
                var listGroupIdxForDelete = listCmdForDelete.Select(x => x.GroupIndex).ToList();
                var listCommandResult = listCmdForDelete.Select(u => new CommandResult
                {
                    ID = u.Index.ToString(),
                    GroupIndex = u.GroupIndex.ToString(),
                    SerialNumber = u.SerialNumber
                }).ToList();

                context.IC_Audit.RemoveRange(listAuditToDelete);
                context.IC_SystemCommand.RemoveRange(listCmdForDelete);

                var groupCommandForDelete = context.IC_CommandSystemGroup.Where(x => listGroupIdxForDelete.Contains(x.Index)).ToList();
                context.IC_CommandSystemGroup.RemoveRange(groupCommandForDelete);

                context.SaveChanges();
                ReloadCaching();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            return Ok();
        }

        [Authorize]
        [ActionName("DeactivateSystemCommands")]
        [HttpPost]
        public async Task<IActionResult> DeactivateSystemCommands([FromBody] List<string> listSystemCommandIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                var commandForDeactive = context.IC_SystemCommand
                    .Where(x => listSystemCommandIndex.Select(idx => int.Parse(idx)).Contains(x.Index)).ToList();
                commandForDeactive.ForEach(cmd =>
                {
                    cmd.IsActive = false;
                });
                await context.SaveChangesAsync();
                this.ReloadCaching();

                var audit = _iC_AuditLogic.CreateDefaultAudit(user, "SystemCommand", AuditType.Deleted);
                _iC_AuditLogic.UpdateAuditStatusToCompleted(audit,
                    numSuccess: listSystemCommandIndex.Count,
                    numFailure: 0,
                    description: $"Xóa {listSystemCommandIndex.Count} lệnh",
                    descriptionEn: $"Delete {listSystemCommandIndex.Count} commands");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }


        [Authorize]
        [ActionName("ReloadCaching")]
        [HttpPost]
        public IActionResult ReloadCaching()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                _iC_CachingLogic.SyncSystemCommandCacheAndDatabase();
                _iC_CachingLogic.ResetCommandCacheForService();
                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("RenewSystemCommand")]
        [HttpPost]
        public async Task<IActionResult> RenewSystemCommand([FromQuery] int systemCommandIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                string serialNumber = context.IC_SystemCommand.FirstOrDefault(x => x.Index == systemCommandIndex)?.SerialNumber;
                bool renewSuccess = _IIC_SystemCommandLogic.RenewSystemCommand(systemCommandIndex, user);
                if (renewSuccess)
                {
                    _iC_CachingLogic.SyncSystemCommandCacheAndDatabase();
                    //Call method delete command one more time for chắc.
                    DeleteByIds(new List<string> { systemCommandIndex.ToString() });
                    await pushNotificationClient.SendNotificationToSDKInterfaceAsync(new List<string>() { serialNumber });
                    return Ok();
                }
            }
            catch (NullReferenceException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
            return BadRequest();
        }
      
        [NonAction]
        public void PostPushNotification(int CompanyIndex, List<SerialNumberCommandParam> pNotification)
        {
            var notification = new Notification()
            {
                CompanyIndex = CompanyIndex,
                Message = pNotification
            };
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(ConfigObject.GetConfig(cache).PushNotificatioinLink);
            var json = JsonConvert.SerializeObject(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("api/Notification", content).Result;
        }

    }
}
