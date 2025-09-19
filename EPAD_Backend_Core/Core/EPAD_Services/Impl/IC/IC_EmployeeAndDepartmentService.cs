using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_EmployeeAndDepartmentService : BaseServices<IC_EmployeeAndDepartment, EPAD_Context>, IIC_EmployeeAndDepartmentService
    {
        public IC_EmployeeAndDepartmentService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
