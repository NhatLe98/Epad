using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Net;
using System.Drawing;
using EPAD_Data.Models;
using EPAD_Data.Entities;
using EPAD_Data;

namespace EPAD_Backend_Core.WebUtilitys
{
    public class PublicFunctions
    {
        static HashSet<Type> mListInt = new HashSet<Type>()
        {
            typeof(int),typeof(Int16),typeof(Int32),typeof(Int64)
        };

        public static T FillGeneralFields<T>(T pObj, UserInfo pUser,bool pEdit)
        {
            PropertyInfo[] arrProperty = pObj.GetType().GetProperties();
            foreach (PropertyInfo item in arrProperty)
            {
                if (item.Name == "CreatedDate" && pEdit==false)
                {
                    item.SetValue(pObj, DateTime.Now);
                }
                else if (item.Name == "UpdatedDate")
                {
                    item.SetValue(pObj, DateTime.Now);
                }
                else if (item.Name == "UpdatedUser")
                {
                    item.SetValue(pObj, pUser.UserName);
                }
                else if (item.Name == "CompanyIndex")
                {
                    item.SetValue(pObj, pUser.CompanyIndex);
                }
            }
            return pObj;
        }
        
        public static string CreateQueryFilterAllColumn<T>(T pObj,int pCompanyIndex,string pFilter)
        {
            pFilter = pFilter.Replace("'", "");

            PropertyInfo[] arrProperty = pObj.GetType().GetProperties();
            List<string> listColumnIgnore = new List<string>() { "CreatedDate", "UpdatedDate", "UpdatedUser", "CompanyIndex","Index" };
            string query = "";
            bool filterIsNumber = false;
            int number = 0;
            if(int.TryParse(pFilter,out number) == true)
            {
                filterIsNumber = true;
            }

            foreach (PropertyInfo item in arrProperty)
            {
                if (mListInt.Contains(item.PropertyType) && filterIsNumber == false)
                {
                    continue;
                }

                if (listColumnIgnore.Contains(item.Name) == false)
                {
                    query += $" [{item.Name}] like '%{pFilter}%' OR";
                }
            }
            if (query.EndsWith("OR"))
            {
                query = query.Substring(0, query.Length - 2);
            }
            if (query.Length > 0)
            {
                query= $"select * from {pObj.GetType().Name} where CompanyIndex={pCompanyIndex} and ({query})";
            }
            return query;
        }
        
        public static int GetDepartmentIndexFromString(string pDepartment, List<IC_Department> pListDepartment, int pCompanyIndex, DateTime now,
             EPAD_Context context, ref string error)
        {
            string[] arrDep = pDepartment.Split('/');
            int parentIndex = 0;
            List<int> listDepartmentIndexCreate = new List<int>();
            for (int i = 0; i < arrDep.Length; i++)
            {
                if (arrDep[i].Trim() == "")
                {
                    error = "DepartmentError";
                    break;
                }
                IC_Department department = pListDepartment.Where(t => t.Name == arrDep[i]).FirstOrDefault();
                if (department == null)
                {
                    department = new IC_Department();
                    department.Name = arrDep[i];
                    department.Location = "";
                    department.Description = "";

                    department.ParentIndex = parentIndex;
                    department.CompanyIndex = pCompanyIndex;
                    department.CreatedDate = now;

                    department.UpdatedDate = now;
                    department.UpdatedUser = UpdatedUser.IntegrateEmployee.ToString();

                    context.IC_Department.Add(department);
                    context.SaveChanges();
                    pListDepartment.Add(department);
                    listDepartmentIndexCreate.Add(department.Index);
                }
                parentIndex = department.Index;
            }
            //revert
            if (error != "")
            {
                context.IC_Department.RemoveRange(context.IC_Department.Where(t => listDepartmentIndexCreate.Contains(t.Index)));
                context.SaveChanges();
            }
            return parentIndex;
        }

        public static string GetImageFromCamera(string pIp,string pPort,string pUser,string pPass,string pChannel,int pCameraIndex,
            ref string error,int pTimeOut,UserInfo user=null)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(pUser, pPass);

            HttpClient client = new HttpClient(handler);
            
            if (pTimeOut > 0)
            {
                client.Timeout = TimeSpan.FromSeconds(pTimeOut);
            }
            string filePath = "";
            error = "";
            DateTime now = DateTime.Now;
            try
            {
                HttpResponseMessage result = client.GetAsync($"http://{pIp}:{pPort}/ISAPI/Streaming/channels/{pChannel}/picture").Result;
                result.EnsureSuccessStatusCode();
                
                HttpContent content = result.Content;
                var stream = result.Content.ReadAsStreamAsync().Result;
                using(MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);

                    Convert.ToBase64String(memoryStream.ToArray());
                }
                

                Image image = Bitmap.FromStream(stream);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (user == null)
                {
                    filePath = $"Files/ImageFromCamera/{pCameraIndex }/{pChannel}/{now.ToString("yyyy-MM-dd")}/{now.ToString("HH")}";
                }
                else
                {
                    filePath = $"Files/ImageFromCamera/CheckImage/{user.UserName}";
                    Directory.Delete(path + filePath);
                }

                if (Directory.Exists(path + filePath) == false)
                {
                    Directory.CreateDirectory(path + filePath);
                }
                filePath += "/" + DateTime.Now.ToString("mmss_ffff") + ".jpg";
                try
                {
                    image.Save(path + filePath);
                }
                catch (Exception ex)
                {
                    filePath = "";
                    error = ex.Message;
                }
            }
            catch(Exception ex)
            {
                error = ex.Message;
                return filePath;
            }
           
            return filePath;
        }

        public static string CreateLinkImageCameraFromStream(Stream stream,int pCameraIndex,string pChannel,DateTime pNow,UserInfo pUser,ref string error)
        {
            if(stream != null)
            {
                var image = Bitmap.FromStream(stream);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = "";
                if (pUser == null)
                {
                    filePath = $"Files/ImageFromCamera/{pCameraIndex }/{pChannel}/{pNow.ToString("yyyy-MM-dd")}/{pNow.ToString("HH")}";
                }
                else
                {
                    filePath = $"Files/ImageFromCamera/CheckImage/{pUser.UserName}";
                    Directory.Delete(path + filePath);
                }

                if (Directory.Exists(path + filePath) == false)
                {
                    Directory.CreateDirectory(path + filePath);
                }
                filePath += "/" + DateTime.Now.ToString("mmss_ffff") + ".jpg";
                try
                {
                    image.Save(path + filePath);
                }
                catch (Exception ex)
                {
                    filePath = "";
                    error = ex.Message;
                }
                return filePath;
            }
            return "";
        }
        
        public static Stream GetStreamImageFromCamera(string pIp, string pPort, string pUser, string pPass, string pChannel, int pTimeOut,ref string error)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(pUser, pPass);

            HttpClient client = new HttpClient(handler);

            if (pTimeOut > 0)
            {
                client.Timeout = TimeSpan.FromSeconds(pTimeOut);
            }
            DateTime now = DateTime.Now;
            Stream stream = null;
            try
            {
                HttpResponseMessage result = client.GetAsync($"http://{pIp}:{pPort}/ISAPI/Streaming/channels/{pChannel}/picture").Result;
                result.EnsureSuccessStatusCode();

                HttpContent content = result.Content;
                stream = result.Content.ReadAsStreamAsync().Result;
                
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return stream;


        }

        public static void UpdateCommandForService(EPAD_Context context,List<UserInfo> pListUser, List<IC_SystemCommand> pListCommand, List<IC_Device> pListDevice)
        {
            if (pListUser.Count == 0)
            {
                return;
            }
            for (int i = 0; i < pListCommand.Count; i++)
            {
                CommandParamDB param = JsonConvert.DeserializeObject<CommandParamDB>(pListCommand[i].Params);

                IC_Device device = pListDevice.Find(t => t.IPAddress == param.IPAddress);
                // tìm service có quản lý device này
                UserInfo user = pListUser.Find(t => t.ListDevice.Find(s => s.SerialNumber == device.SerialNumber) != null);
                if (user != null)
                {
                    List<CommandResult> listCommandService = user.ListCommands;
                    // kiểm tra có command này ko
                    if (user.CheckCommandExists(pListCommand[i].Command, device.SerialNumber, device.IPAddress, pListCommand[i].CreatedDate.Value)==false)
                    {
                        CommandResult command = new CommandResult();
                        command.Command = pListCommand[i].Command;
                        command.IPAddress = param.IPAddress;
                        command.Port = param.Port;
                        
                        command.FromTime = param.FromTime;
                        command.ToTime = param.ToTime;
                        command.ListUsers = param.ListUsers;
                        command.SerialNumber = device.SerialNumber;

                        command.CreatedTime = pListCommand[i].RequestedTime.Value;
                        command.Status = CommandStatus.UnExecute.ToString();

                        user.ListCommands.Add(command);
                    }
                }
            }
        }
        public static void CreateCompanyCache(EPAD_Context pContext, IMemoryCache pCache)
        {
            List<IC_Company> listCompany = pContext.IC_Company.ToList();
            for (int i = 0; i < listCompany.Count; i++)
            {
                CompanyInfo companyInfo = CompanyInfo.GetFromCache(pCache, listCompany[i].Index.ToString());
                if (companyInfo == null)
                {
                    companyInfo = new CompanyInfo();
                    companyInfo.CompanyIndex = listCompany[i].Index;

                    companyInfo.AddToCache(pCache, listCompany[i].Index.ToString());
                }
                companyInfo.SetConfig(pContext.IC_Config.Where(t => t.CompanyIndex == listCompany[i].Index).ToList());
            }
        }

        public static string CreateHttpErrorContent(string pData)
        {
            return pData;
        }
    }
}