using EPAD_Common.Extensions;
using EPAD_Common.Observables;
using EPAD_Data.Constants;
using EPAD_Data.Models;
using EPAD_Data.Models.FR05;
using EPAD_Services.FR05.Interface;
using EPAD_Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Impl
{
    public class IC_UserApiMachineService : IIC_UserApiMachineService
    {
        private readonly IIC_DeviceService _deviceService;
        ConfigObject _Config;
        IMemoryCache _Cache;
        public IC_UserApiMachineService(IServiceProvider serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _deviceService = serviceProvider.GetService<IIC_DeviceService>();
        }

        
        public Task DeleteAllFingerPrint(CommandResult param)
        {
            throw new NotImplementedException();
        }

        public async Task<string> DeleteAllUser(CommandResult param)
        {
            var err = "";
            var client = new HttpClient();
            var httpIp = "http://" + param.IPAddress + ":" + param.Port;
            try
            {
                var device = await _deviceService.GetBySerialNumber(param.SerialNumber, 2);
                if (!string.IsNullOrEmpty(param.ExternalData))
                {
                    var externalDataDefinition = new
                    {
                        AuthenModes = new string[] { },
                    };
                    var externalData = JsonConvert.DeserializeAnonymousType(param.ExternalData, externalDataDefinition);
                    string[] authModes = externalData?.AuthenModes;

                    if (authModes?.Length > 0)
                    {
                        if (authModes.Contains("Finger"))
                        {
                            await DeleteAllFingers(httpIp, device.ConnectionCode, null);
                        }

                        if (authModes.Contains("Face"))
                        {
                            await DeleteAllFaces(httpIp, device.ConnectionCode, null);
                        }
                    }
                    else
                    {
                        await DeleteAllUser(httpIp, device.ConnectionCode, null);
                    }

                }
                else
                {
                    await DeleteAllUser(httpIp, device.ConnectionCode, null);
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;
        }

        public async Task<UserInfoCommandResult> DeleteUserById(CommandResult param, DeviceProcessTracker tracker)
        {
            var result = new UserInfoCommandResult();
            var httpIp = "http://" + param.IPAddress + ":" + param.Port;
            try
            {
                var device = await _deviceService.GetBySerialNumber(param.SerialNumber, 2);
                var pUserIds = param.ListUsers;

                if (pUserIds?.Count > 0)
                {
                    if (!string.IsNullOrEmpty(param.ExternalData))
                    {
                        var externalDataDefinition = new
                        {
                            AuthenModes = new string[] { },
                        };
                        var externalData = JsonConvert.DeserializeAnonymousType(param.ExternalData, externalDataDefinition);
                        string[] authModes = externalData?.AuthenModes;

                        for (int i = 0; i < pUserIds.Count; i++)
                        {
                            if (NumberExtensions.IsNumber(pUserIds[i].UserID))
                            {
                                if (authModes?.Length > 0)
                                {
                                    bool check = false;
                                    if (authModes.Contains("Finger"))
                                    {
                                        check = await DeleteAllFingers(httpIp, device.ConnectionCode, null);
                                    }
                                    if (authModes.Contains("Face"))
                                    {
                                        check = await DeleteAllFaces(httpIp, device.ConnectionCode, null);
                                    }
                                    if (check)
                                    {
                                        result.UserIdsSuccess.Add(pUserIds[i].UserID);
                                    }
                                    else
                                    {
                                        result.UserIdsFailed.Add(pUserIds[i].UserID);
                                    }
                                }
                                else
                                {
                                    bool check = await DeleteAllUser(httpIp, device.ConnectionCode, pUserIds[i].UserID);
                                    if (check)
                                    {
                                        result.UserIdsSuccess.Add(pUserIds[i].UserID);
                                    }
                                    else
                                    {
                                        result.UserIdsFailed.Add(pUserIds[i].UserID);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await DeleteAllUser(httpIp, device.ConnectionCode, null);
                    }
                }
                else
                {
                    await DeleteAllUser(httpIp, device.ConnectionCode, null);
                }

            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }

        public async Task<UserInfoCommandResult> DownloadAllUser(CommandResult param, DeviceProcessTracker tracker)
        {
            var err = "";
            var UserIdsSuccess = new List<string>();
            var UserIdsFailed = new List<string>();
            var pUserInfos = new List<UserInfoOnMachine>(); ;
            var httpIp = "http://" + param.IPAddress + ":" + param.Port;
            try
            {
                var device = await _deviceService.GetBySerialNumber(param.SerialNumber,2);
                var users = await GetAllUser(httpIp, device.ConnectionCode, null);
                if (users != null && users.Count > 0)
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        var newUser = new UserInfoOnMachine(users[i].Id);
                        var templates = await GetFaceById(httpIp, device.ConnectionCode, users[i].Id);
                        FaceInfo info = null;
                        if (templates != null && templates.Count > 0)
                        {
                            var TmpDataFace = templates[0].ImgBase64;
                            info = new FaceInfo()
                            {
                                FaceTemplate = TmpDataFace
                            };
                            newUser.Face = info;
                        }

                        var fingerDatas = await GetFingerById(httpIp, users[i].Id, device.ConnectionCode);
                        List<FingerInfo> lstFinger = null;
                        if (fingerDatas != null && fingerDatas.Count > 0)
                        {
                            lstFinger = ConvertFingerIndex(fingerDatas);
                        }

                        pUserInfos.Add(new UserInfoOnMachine()
                        {
                            UserID = users[i].Id,
                            NameOnDevice = users[i].Name,
                            CardNumber = users[i].IdCardNum,
                            Enable = true,
                            Face = info,
                            FingerPrints = lstFinger
                        });

                        UserIdsSuccess.Add(users[i].Id);
                    }
                }

            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return new UserInfoCommandResult()
            {
                Error = err,
                UserInfos = pUserInfos,
                UserIdsFailed = UserIdsFailed,
                UserIdsSuccess = UserIdsSuccess
            };

        }

        public async Task<UserInfoCommandResult> DownloadUserById(CommandResult param, DeviceProcessTracker tracker)
        {
            var client = new HttpClient();
            var pUserInfos = new List<UserInfoOnMachine>();
            var err = "";
            var httpIp = "http://" + param.IPAddress + ":" + param.Port;
            try
            {
                var device = await _deviceService.GetBySerialNumber(param.SerialNumber, 2);
                var users = await GetAllUser(httpIp, device.ConnectionCode, null);
                var usersListUpl = param.ListUsers.Where(x => NumberExtensions.IsNumber(x.UserID)).Select(x => x.UserID).ToList();
                users = users.Where(x => usersListUpl.Contains(x.Id)).ToList();

                if (users != null && users.Count > 0)
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        var newUser = new UserInfoOnMachine(users[i].Id);
                        var templates = await GetFaceById(httpIp, device.ConnectionCode, users[i].Id);
                        FaceInfo info = null;
                        if (templates != null && templates.Count > 0)
                        {
                            var TmpDataFace = templates[0].ImgBase64;
                            info = new FaceInfo()
                            {
                                FaceTemplate = TmpDataFace
                            };
                            newUser.Face = info;
                        }


                        var fingerDatas = await GetFingerById(httpIp, users[i].Id, device.ConnectionCode);
                        List<FingerInfo> lstFinger = null;
                        if (fingerDatas != null && fingerDatas.Count > 0)
                        {
                            lstFinger = ConvertFingerIndex(fingerDatas);
                        }

                        pUserInfos.Add(new UserInfoOnMachine()
                        {
                            UserID = users[i].Id,
                            NameOnDevice = users[i].Name,
                            CardNumber = users[i].IdCardNum,
                            Enable = true,
                            Face = info
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            var userInfo = new UserInfoCommandResult();
            userInfo.UserInfos = pUserInfos;
            userInfo.UserIdsSuccess = pUserInfos.Select(x => x.UserID).ToList();
            userInfo.UserIdsFailed = param.ListUsers.Select(x => x.UserID).Where(x => pUserInfos.Select(x => x.UserID).Contains(x)).ToList();
            return userInfo;
        }

        public async Task<UserInfoCommandResult> UploadUsers(CommandResult param, DeviceProcessTracker tracker)
        {
            var device = await _deviceService.GetBySerialNumber(param.SerialNumber, 2);
            string[] authModes = null;
            var pUserIds = param.ListUsers;
            var userIdSuccess = new List<string>();
            var userIdFail = new List<string>();
            var err = "";
            var httpIp = "http://" + param.IPAddress + ":" + param.Port;
            try
            {
                if (!string.IsNullOrEmpty(param.ExternalData))
                {
                    var externalDataDefinition = new
                    {
                        AuthenModes = new string[] { },
                    };
                    var externalData = JsonConvert.DeserializeAnonymousType(param.ExternalData, externalDataDefinition);
                    authModes = externalData?.AuthenModes;


                    foreach (var userInfo in pUserIds)
                    {
                        if (NumberExtensions.IsNumber(userInfo.UserID))
                        {
                            var userUpl = new FindUserData()
                            {
                                Id = userInfo.UserID,
                                CreateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                                IdCardNum = userInfo.CardNumber,
                                Name = !string.IsNullOrEmpty(userInfo.NameOnDevice) ? userInfo.NameOnDevice : userInfo.UserID,
                                Vaccination = 0
                            };

                            var result = await UploadUser(httpIp, userUpl, device.ConnectionCode);

                            if (authModes.Contains("Face") && userInfo.Face != null && !string.IsNullOrEmpty(userInfo.Face.FaceTemplate))
                            {
                                var faceUpload = new UserFaceCreate()
                                {
                                    PersonId = userInfo.UserID,
                                    ImgBase64 = userInfo.Face.FaceTemplate,
                                    Pass = device.ConnectionCode
                                };

                                await UploadFace(httpIp, faceUpload, device.ConnectionCode);
                            }

                            if (authModes.Contains("Finger") && userInfo.FingerPrints != null && userInfo.FingerPrints.Count() > 0)
                            {
                                var userFingerParam = ConvertFingerUpload(userInfo.FingerPrints, userInfo.UserID);

                                var fingerUpload = new UserFingerUploadParam()
                                {
                                    Pass = device.ConnectionCode,
                                    Data = JsonConvert.SerializeObject(userFingerParam)
                                };

                                await FingerUpload(httpIp, fingerUpload);
                            }

                            if (result)
                            {
                                userIdSuccess.Add(userInfo.UserID);
                            }
                            else
                            {
                                userIdFail.Add(userInfo.UserID);
                            }
                        }
                        else
                        {
                            userIdFail.Add(userInfo.UserID);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return new UserInfoCommandResult()
            {
                UserIdsFailed = userIdFail,
                UserIdsSuccess = userIdSuccess,
                Error = err
            };
        }

        private async Task<bool> DeleteAllFingers(string ipAddress, string pass, string id)
        {
            var client = new HttpClient();
            var sendData = new FingerDeleteParam()
            {
                Pass = pass,
                FingerId = string.IsNullOrEmpty(id) ? "-1" : id
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.DeleteFingerData, new FormUrlEncodedContent(values));
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> DeleteAllFaces(string ipAddress, string pass, string id)
        {
            var client = new HttpClient();
            var sendData = new FaceDeleteParam()
            {
                Pass = pass,
                FaceId = string.IsNullOrEmpty(id) ? "-1" : id
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.DeleteFaceData, new FormUrlEncodedContent(values));
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> DeleteAllUser(string ipAddress, string pass, string id)
        {
            var client = new HttpClient();
            var sendData = new UserDeleteParam()
            {
                Pass = pass,
                Id = string.IsNullOrEmpty(id) ? "-1" : id
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.DeleteAllUser, new FormUrlEncodedContent(values));
            return response.IsSuccessStatusCode;
        }


        private async Task<List<FindUserData>> GetAllUser(string ipAddress, string pass, string id)
        {
            var client = new HttpClient();
            var sendData = new UserFindParam()
            {
                Pass = pass,
                Id = !string.IsNullOrEmpty(id) ? id : "-1"
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.FindUser, new FormUrlEncodedContent(values));

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<FindUsersResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return item.Data;
        }

        private async Task<List<UserFaceData>> GetFaceById(string ipAddress, string pass, string id)
        {
            var client = new HttpClient();
            var sendData = new UserFingerParam()
            {
                Pass = pass,
                PersonId = !string.IsNullOrEmpty(id) ? id : "-1"
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.FindFace, new FormUrlEncodedContent(values));

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<UserFaceResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return item.Data;
        }

        private async Task<bool> UploadUser(string ipAddress, FindUserData user, string pass)
        {
            var client = new HttpClient();
            var jsonPerson = JsonConvert.SerializeObject(user);
            var sendData = new UserUploadParam()
            {
                Pass = pass,
                Person = jsonPerson
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.CreateUser, new FormUrlEncodedContent(values));
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> UploadFace(string ipAddress, UserFaceCreate faceCreate, string pass)
        {
            var client = new HttpClient();
            var json = JsonConvert.SerializeObject(faceCreate);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.CreateFace, new FormUrlEncodedContent(values));
            return response.IsSuccessStatusCode;
        }


        private async Task<List<FindFingerData>> GetFingerById(string ipAddress, string personId, string pass)
        {
            var client = new HttpClient();
            var fingerParam = new UserFingerParam()
            {
                PersonId = personId,
                Pass = pass
            };
            var json = JsonConvert.SerializeObject(fingerParam);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.FindFingerPrint, new FormUrlEncodedContent(values));
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<FindFingerResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return item.Data;
        }

        private async Task<bool> FingerUpload(string ipAddress, UserFingerUploadParam fingerParam)
        {
            var client = new HttpClient();

            var json = JsonConvert.SerializeObject(fingerParam);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(ipAddress + "/" + FR05ApiConst.CreateFinger, new FormUrlEncodedContent(values));
            return response.IsSuccessStatusCode;

        }

        private List<FingerInfo> ConvertFingerIndex(List<FindFingerData> findFingerDatas)
        {
            var lsFingerInfos = new List<FingerInfo>();
            foreach (var item in findFingerDatas)
            {
                if (item.FingerNum > 0 && item.FingerNum < 16)
                {
                    item.FingerNum = item.FingerNum - 11;
                }
                else if (item.FingerNum > 20 && item.FingerNum < 26)
                {
                    item.FingerNum = item.FingerNum - 16;
                }

                lsFingerInfos.Add(new FingerInfo() { FingerIndex = item.FingerNum, FingerTemplate = item.Feature });
            }

            return lsFingerInfos;
        }

        private List<FindFingerData> ConvertFingerUpload(List<FingerInfo> findFingerDatas, string personId)
        {
            var lsFingerInfos = new List<FindFingerData>();
            foreach (var item in findFingerDatas)
            {
                if (item.FingerIndex < 5)
                {
                    item.FingerIndex += 11;
                }
                else if (item.FingerIndex >= 5)
                {
                    item.FingerIndex += 16;
                }


                lsFingerInfos.Add(new FindFingerData() { Feature = item.FingerTemplate, FingerNum = item.FingerIndex, PersonId = personId});
            }

            return lsFingerInfos;
        }



    }

}
