using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Lines_CheckInCameraService : BaseServices<GC_Lines_CheckInCamera, EPAD_Context>, IGC_Lines_CheckInCameraService
    {
        public GC_Lines_CheckInCameraService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<int>> GetAllCameraInAndOutByCompanyIndexAndIgnoreLineAndInOutMode(int companyIndex, int lineIndex, bool lineIn)
        {
            List<int> listSerial = new List<int>();
            List<int> listTemp = new List<int>();
            // get all serials from other lines
            listSerial = await DbContext.GC_Lines_CheckInCamera.Where(x => x.CompanyIndex == companyIndex && x.LineIndex != lineIndex)
                    .Select(x => x.CameraIndex).ToListAsync();
            listTemp = await DbContext.GC_Lines_CheckOutCamera.Where(x => x.CompanyIndex == companyIndex && x.LineIndex != lineIndex)
                    .Select(x => x.CameraIndex).ToListAsync();
            listSerial.AddRange(listTemp);

            // get all serials from reserve line
            if (lineIn == true)
            {
                listTemp = await DbContext.GC_Lines_CheckOutCamera.Where(x => x.CompanyIndex == companyIndex && x.LineIndex == lineIndex)
                       .Select(x => x.CameraIndex).ToListAsync();
            }
            else
            {
                listTemp = await DbContext.GC_Lines_CheckInCamera.Where(x => x.CompanyIndex == companyIndex && x.LineIndex == lineIndex)
                      .Select(x => x.CameraIndex).ToListAsync();
            }
            listSerial.AddRange(listTemp);

            return listSerial.Distinct().ToList();
        }

        public async Task<List<GC_Lines_CheckInCamera>> GetDataByLineIndex(int lineIndex, int companyIndex)
        {
            return await DbContext.GC_Lines_CheckInCamera.Where(x => x.CompanyIndex == companyIndex && x.LineIndex == lineIndex).ToListAsync();
        }
    }
}
