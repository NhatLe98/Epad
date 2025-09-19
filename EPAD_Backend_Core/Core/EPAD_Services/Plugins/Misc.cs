using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Services.Plugins
{
    public class Misc
    {
        public static void LoadMonitoringDeviceList(EPAD_Context _context, IMemoryCache _cache)
        {
            var listCheckInDevice = _context.GC_Lines_CheckInDevice.ToList();
            var listCheckOutDevice = _context.GC_Lines_CheckOutDevice.ToList();
            //var listCheckInputDevice = _dbContext.GC_Lines_CheckInputDevice.ToList();
            var listParking = _context.GC_ParkingLotDetail.ToList();
            var listData = new List<MonitoringDevice>();

            for (int i = 0; i < listCheckInDevice.Count; i++)
            {
                var data = listData.Where(t => t.CompanyIndex == listCheckInDevice[i].CompanyIndex).FirstOrDefault();
                if (data == null)
                {
                    data = new MonitoringDevice();
                    data.CompanyIndex = listCheckInDevice[i].CompanyIndex;
                    data.LineInDeviceSerialList.Add(listCheckInDevice[i].CheckInDeviceSerial, listCheckInDevice[i].LineIndex);
                    listData.Add(data);
                }
                else
                {
                    if (data.LineInDeviceSerialList.ContainsKey(listCheckInDevice[i].CheckInDeviceSerial) == false)
                    {
                        data.LineInDeviceSerialList.Add(listCheckInDevice[i].CheckInDeviceSerial, listCheckInDevice[i].LineIndex);
                    }
                }
                var parkingData = listParking.Where(t => t.CompanyIndex == data.CompanyIndex
                      && t.LineIndex == listCheckInDevice[i].LineIndex).FirstOrDefault();
                if (parkingData != null && data.ListLineParking.Contains(listCheckInDevice[i].LineIndex) == false)
                {
                    data.ListLineParking.Add(listCheckInDevice[i].LineIndex);
                }
            }
            for (int i = 0; i < listCheckOutDevice.Count; i++)
            {
                var data = listData.Where(t => t.CompanyIndex == listCheckOutDevice[i].CompanyIndex).FirstOrDefault();
                if (data == null)
                {
                    data = new MonitoringDevice();
                    data.CompanyIndex = listCheckOutDevice[i].CompanyIndex;
                    data.LineOutDeviceSerialList.Add(listCheckOutDevice[i].CheckOutDeviceSerial, listCheckOutDevice[i].LineIndex);
                    listData.Add(data);
                }
                else
                {
                    if (data.LineOutDeviceSerialList.ContainsKey(listCheckOutDevice[i].CheckOutDeviceSerial) == false)
                    {
                        data.LineOutDeviceSerialList.Add(listCheckOutDevice[i].CheckOutDeviceSerial, listCheckOutDevice[i].LineIndex);
                    }
                }

                var parkingData = listParking.Where(t => t.CompanyIndex == data.CompanyIndex
                      && t.LineIndex == listCheckOutDevice[i].LineIndex).FirstOrDefault();
                if (parkingData != null && data.ListLineParking.Contains(listCheckOutDevice[i].LineIndex) == false)
                {
                    data.ListLineParking.Add(listCheckOutDevice[i].LineIndex);
                }
            }
            //for (int i = 0; i < listCheckInputDevice.Count; i++)
            //{
            //    var data = listData.Where(t => t.CompanyIndex == listCheckInputDevice[i].CompanyIndex).FirstOrDefault();
            //    if (data == null)
            //    {
            //        data = new MonitoringDevice();
            //        data.CompanyIndex = listCheckInputDevice[i].CompanyIndex;
            //        data.LineInputDeviceSerialList.Add(listCheckInputDevice[i].CheckInputDeviceSerial, listCheckInputDevice[i].LineIndex);
            //        listData.Add(data);
            //    }
            //    else
            //    {
            //        if (data.LineInputDeviceSerialList.ContainsKey(listCheckInputDevice[i].CheckInputDeviceSerial) == false)
            //        {
            //            data.LineInputDeviceSerialList.Add(listCheckInputDevice[i].CheckInputDeviceSerial, listCheckInputDevice[i].LineIndex);
            //        }
            //    }

            //    var parkingData = listParking.FirstOrDefault(t => t.CompanyIndex == data.CompanyIndex
            //          && t.LineIndex == listCheckInputDevice[i].LineIndex);
            //    if (parkingData != null && data.ListLineParking.Contains(listCheckInputDevice[i].LineIndex) == false)
            //    {
            //        data.ListLineParking.Add(listCheckInputDevice[i].LineIndex);
            //    }
            //}
            _cache.Set("MonitoringDeviceList", listData, new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromDays(4) });
        }
    }
}
