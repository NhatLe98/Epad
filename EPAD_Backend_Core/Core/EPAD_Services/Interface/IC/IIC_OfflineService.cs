﻿using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_OfflineService
    {
        Task IntegrateBlackList(List<GC_BlackList> blacklists);
        Task DeleteBlackList(List<string> employeeATIDs, List<string> nricLst);
        Task IntegrateCustomerCardToOffline(List<HR_CustomerCard> employeeATIDs);
        Task DeleteCustomerCardToOffline(List<string> employeeATIDs);
        Task IntegrateCardToOffline(List<HR_CardNumberInfo> employeeATIDs);
        Task DeleteCardToOffline(List<HR_CardNumberInfo> employeeATIDs);
        Task IntegrateDepartmentToOffline(List<IC_Department> departments);
        Task DeleteDepartmentToOffline(List<string> codes);
        Task IntegrateUserToOffline(List<HR_User> codes);
        Task DeleteUserToOffline(List<string> codes);
        Task IntegrateWorkingInfo(List<IC_WorkingInfoIntegrate> workingInfoList);
        Task IntegrateEmployeeToOffline(List<HR_EmployeeInfo> userList);
        Task IntegrateCustmerToOffline(List<HR_CustomerInfo> userList);
    }
}
