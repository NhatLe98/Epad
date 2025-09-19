using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using static EPAD_Data.Models.CommonUtils;

namespace EPAD_Logic
{
    public class HR_WorkingInfoLogic : IHR_WorkingInfoLogic
    {
        private ezHR_Context _dbIntergrateContext;
        public HR_WorkingInfoLogic(ezHR_Context dbIntergrateContext)
        {
            _dbIntergrateContext = dbIntergrateContext;
        }
        public List<HR_WorkingInfo> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<HR_WorkingInfo>();

            var query = _dbIntergrateContext.HR_WorkingInfo.AsQueryable();

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                query = query.Where(u => u.DepartmentIndex.HasValue && departments.Contains(u.DepartmentIndex.Value));
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null)
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID));
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {
                                int departmentIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.DepartmentIndex == departmentIndex);
                            }
                            break;
                        case "IsSync":
                            if (param.Value != null)
                            {
                                bool isSync = Convert.ToBoolean(param.Value);
                                query = query.Where(u => isSync == true ? u.Synched == 1 : u.Synched == 2);
                            }
                            break;
                        case "IsCurrentWorking":
                            if (param.Value != null)
                            {
                                bool isCurrentWorking = Convert.ToBoolean(param.Value);
                                if (isCurrentWorking)
                                {
                                    query = query.Where(u => (u.ToDate == null && DateTime.Today.Date >= u.FromDate.Value.Date)
                                                         || (u.ToDate != null && DateTime.Today.Date >= u.FromDate.Value.Date && DateTime.Today.Date <= u.ToDate.Value.Date));
                                }
                            }
                            break;
                    }
                }
            }
            // var totalcount = query.Count();
            var data = query.ToList();
            return data;
        }
        public IC_WorkingInfoDTO Update(List<AddedParam> addParams)
        {
            AddedParam addParam = addParams.FirstOrDefault(a => a.Key == "WorkingInfoIndex");
            if (addParam == null) return null;
            addParams.Remove(addParam);
            int workingInfoIndex = Convert.ToInt32(addParam.Value);
            HR_WorkingInfo dataItem = _dbIntergrateContext.HR_WorkingInfo.FirstOrDefault(u => u.Index == workingInfoIndex);
            if (dataItem == null) return null;
            //update field
            foreach (AddedParam p in addParams)
            {
                switch (p.Key)
                {
                    case "IsSync":
                        short isSync = 0;
                        if (p.Value != null)
                        {
                            isSync = Convert.ToInt16(p.Value);
                        }
                        dataItem.Synched = isSync;
                        break;
                }
            }
            _dbIntergrateContext.HR_WorkingInfo.Update(dataItem);
            _dbIntergrateContext.SaveChanges();

            var item = (IC_WorkingInfoDTO)ConvertObject(dataItem, new IC_WorkingInfoDTO());
            return item;
        }

        public List<HR_WorkingInfo> UpdateList(List<HR_WorkingInfo> listItem, List<AddedParam> addParams)
        {
            if (listItem != null && addParams != null)
            {

                var listUpdate = _dbIntergrateContext.HR_WorkingInfo.Where(u => listItem.Where(y => y.Index == u.Index).Count() > 0).ToList();

                foreach (AddedParam p in addParams)
                {
                    switch (p.Key)
                    {
                        case "IsSync":
                            bool updateIsSync = Convert.ToBoolean(p.Value);
                            foreach (var item in listItem)
                            {
                                var updateItem = listUpdate.FirstOrDefault(u => u.Index == item.Index);
                                if (updateItem != null)
                                {
                                    updateItem.Synched = short.Parse(updateIsSync == false ? "0" : "1"); ;
                                }
                            }
                            break;
                        case "UpdatedDate":
                            DateTime updatedDate = Convert.ToDateTime(p.Value);
                            foreach (var item in listItem)
                            {
                                var updateItem = listUpdate.FirstOrDefault(u => u.Index == item.Index);
                                if (updateItem != null)
                                {
                                    updateItem.UpdatedDate = updatedDate;
                                }
                            }
                            break;
                        case "UpdatedUser":
                            string updateUser = p.Value.ToString();
                            foreach (var item in listItem)
                            {
                                var updateItem = listUpdate.FirstOrDefault(u => u.Index == item.Index);
                                if (updateItem != null)
                                {
                                    updateItem.UpdatedUser = updateUser;
                                }
                            }
                            break;
                    }
                }

                _dbIntergrateContext.HR_WorkingInfo.UpdateRange(listUpdate);
                _dbIntergrateContext.SaveChanges();
                return listUpdate;
            }
            return null;

        }
    }

    public interface IHR_WorkingInfoLogic {
        List<HR_WorkingInfo> GetMany(List<AddedParam> addedParams);
        List<HR_WorkingInfo> UpdateList(List<HR_WorkingInfo> listItem, List<AddedParam> addParams);
        IC_WorkingInfoDTO Update(List<AddedParam> addParams);
    }
}
