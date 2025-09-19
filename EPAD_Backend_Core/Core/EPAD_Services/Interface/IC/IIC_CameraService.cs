using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_CameraService : IBaseServices<IC_Camera, EPAD_Context>
    {
        Task<List<IC_Camera>> GetAllCamera(int pCompanyIndex);
       CameraPictureResult GetCameraPictureByCameraIndex(int cameraIndex, string channel, UserInfo user);
    }
}
