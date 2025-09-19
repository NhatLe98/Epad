using EPAD_Common.HTTPClient;
using EPAD_Data.HTTPClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface.EZHR
{
    public interface IEzhrApiClient
    {
        Task<OAuth2ResponseEz> GetToken(OAuth2Request request);
        Task<ResultCheckHoursReponse> CheckMaximumWorkingHoursAndOT(string employeeATID, DateTime date);
        Task<LeaveDayBasicInfoReponse> GetListEmployeeLeaveDay(string employeeATID, DateTime date);
        Task<MissionApprovedResultReponse> GetListMissionApproved(string employeeATID, DateTime date);
        Task<LateInEarlyOutApprovedResultReponse> LateInEarlyOutApprovedResult(string employeeATID, DateTime date);
    }
}
