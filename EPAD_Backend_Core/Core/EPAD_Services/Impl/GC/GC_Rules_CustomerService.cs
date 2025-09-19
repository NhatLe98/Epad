using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class GC_Rules_CustomerService : BaseServices<GC_Rules_Customer, EPAD_Context>, IGC_Rules_CustomerService
    {
        public GC_Rules_CustomerService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
