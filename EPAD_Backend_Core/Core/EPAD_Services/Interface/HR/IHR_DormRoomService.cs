using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_DormRoomService : IBaseServices<HR_DormRoom, EPAD_Context>
    {
        Task<List<HR_FloorLevel>> GetAllFloorlevel(UserInfo user);
        Task<List<HR_DormRoom>> GetAllDormRoom(UserInfo user);
        Task<DataGridClass> GetDormRoom(UserInfo user, int page, int limit, string filter);
        Task<HR_DormRoom> GetByIndex(int index);
        Task<List<HR_DormRoom>> GetByCodeOrName(UserInfo user, string code, string name);
        Task<List<HR_DormRoom>> GetDormRoomUsingByIndexes(List<int> indexes);
        Task<bool> AddDormRoom(UserInfo user, HR_DormRoom data);
        Task<bool> UpdateDormRoom(UserInfo user, HR_DormRoom data);
        Task<bool> DeleteDormRoom(List<int> indexes);
    }
}
