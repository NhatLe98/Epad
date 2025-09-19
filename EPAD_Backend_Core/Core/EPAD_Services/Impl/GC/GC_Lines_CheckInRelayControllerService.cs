using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Lines_CheckInRelayControllerService : BaseServices<GC_Lines_CheckInRelayController, EPAD_Context>, IGC_Lines_CheckInRelayControllerService
    {
        public GC_Lines_CheckInRelayControllerService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
    }
}
