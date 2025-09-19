using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_CompanyService : BaseServices<IC_Company, EPAD_Context>, IIC_CompanyService
    {
        public IC_CompanyService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
