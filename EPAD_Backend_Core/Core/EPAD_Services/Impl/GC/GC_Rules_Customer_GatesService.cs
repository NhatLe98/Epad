using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class GC_Rules_Customer_GatesService : BaseServices<GC_Rules_Customer_Gates, EPAD_Context>, IGC_Rules_Customer_GatesService
    {
        public GC_Rules_Customer_GatesService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}

