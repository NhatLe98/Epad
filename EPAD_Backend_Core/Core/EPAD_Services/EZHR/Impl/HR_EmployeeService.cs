using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface1;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Impl1
{
    public class HR_EmployeeService : BaseServices<HR_Employee, ezHR_Context>, IHR_EmployeeService
    {
        public HR_EmployeeService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
       
    }
}
