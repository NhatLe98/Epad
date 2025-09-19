using EPAD_Backend_Core.Protos;
using EPAD_Services.Interface;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace EPAD_Backend_Core.Grpc.Services
{
    public class EpadService : UserNamagement.UserNamagementBase
    {
        private readonly ILogger<EpadService> _logger;
        private readonly IHR_EmployeeInfoService _HR_EmployeeInfoService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IMapper _Mapper;

        public EpadService(ILogger<EpadService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _HR_EmployeeInfoService = serviceProvider.GetService<IHR_EmployeeInfoService>();
            _Mapper = serviceProvider.GetService<IMapper>();
        }

        public override async Task<EmployeeInfoResults> GetAllEmployeeInfo(ListEmployeeReq request, ServerCallContext context)
        {
            var allEmployee = await _HR_EmployeeInfoService.GetAllEmployeeInfo(request.EmployeeATID.ToArray(), request.CompanyIndex.Value);

            var rs = new EmployeeInfoResults();
            rs.EmployeeInfoResults_.AddRange(allEmployee.Select(x => _Mapper.Map<EmployeeInfoResult>(x)));
            return rs;
        }

        public override async Task<EmployeeInfoResult> GetEmployeeInfo(EmployeeReq request, ServerCallContext context)
        {
            var employee = await _HR_EmployeeInfoService.GetEmployeeInfo(request.EmployeeATID, request.CompanyIndex.Value);
            if(employee != null)
            {
                return _Mapper.Map<EmployeeInfoResult>(employee);
            }

            return null;
        }
    }
}
