using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EPAD_Services.Impl
{
    public class AC_TimezoneService : BaseServices<AC_TimeZone, EPAD_Context>, IAC_TimezoneService
    {
        public AC_TimezoneService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;

            var timezoneLst = DbContext.AC_TimeZone.Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrEmpty(filter) && t.Name.Contains(filter) || string.IsNullOrEmpty(filter))).ToList();
            var result = timezoneLst.Select(x => new AC_TimezoneReturn
            {
                Name = x.Name,
                Monday = x.MonStart1.ConvertACTimeToString() + " - " + x.MonEnd1.ConvertACTimeToString(),
                Tuesday = x.TuesStart1.ConvertACTimeToString() + " - " + x.TuesEnd1.ConvertACTimeToString(),
                Wednesday = x.WedStart1.ConvertACTimeToString() + " - " + x.WedEnd1.ConvertACTimeToString(),
                Thursday = x.ThursStart1.ConvertACTimeToString() + " - " + x.ThursEnd1.ConvertACTimeToString(),
                Friday = x.FriStart1.ConvertACTimeToString() + " - " + x.FriEnd1.ConvertACTimeToString(),
                Saturday = x.SatStart1.ConvertACTimeToString() + " - " + x.SatEnd1.ConvertACTimeToString(),
                Sunday = x.SunStart1.ConvertACTimeToString() + " - " + x.SunEnd1.ConvertACTimeToString(),
                Description = x.Description,
                UID = x.UID,
                UIDIndex = x.UIDIndex,
                UIDIndex2 = x.UIDIndex != null ? x.UIDIndex + 1 : null,
                UIDIndex3 = x.UIDIndex != null ? x.UIDIndex + 2 : null,
            }).ToList();

            countPage = result.Count();
            dataGrid = new DataGridClass(countPage, result);
            if (pPage <= 1)
            {
                var lsDevice = result.OrderBy(t => t.Name).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsDevice = result.OrderBy(t => t.Name).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }

        public List<AC_TimeZone> GetAllDataTimezoneReturn(int pCompanyIndex)
        {
            var listName = new List<string>() { "Name", "UID", "UpdatedDate", "CompanyIndex", "UIDIndex", "Description" };
            var timezoneLst = DbContext.AC_TimeZone.AsNoTracking().Where(t => t.CompanyIndex == pCompanyIndex).ToList();
            foreach(var result in timezoneLst)
            {
                foreach (var prop in result.GetType().GetProperties())
                {
                    if (!listName.Contains(prop.Name))
                    {
                        var value = prop.GetValue(result, null);
                        if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        {
                            SetObjectProperty(prop.Name, value.ToString().ConvertACTimeToString(), result);

                        }
                    }
                }
              
            }
            return timezoneLst;
        }

        private void SetObjectProperty(string propertyName, string value, object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            // make sure object has the property we are after
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        public AC_TimeZone GetTimezoneByID(int UID)
        {
            var listName = new List<string>() { "Name", "UID", "UpdatedDate", "CompanyIndex", "UIDIndex" };
            var result = DbContext.AC_TimeZone.AsNoTracking().FirstOrDefault(x => x.UID == UID);

            foreach (var prop in result.GetType().GetProperties())
            {
                if (!listName.Contains(prop.Name))
                {
                    var value = prop.GetValue(result, null);
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        SetObjectProperty(prop.Name, value.ToString().ConvertACTimeToString(), result);

                    }
                }
            }
            return result;
        }

        public class AC_TimezoneReturn
        {
            public int UID { get; set; }
            public string Sunday { get; set; }
            public string Monday { get; set; }
            public string Tuesday { get; set; }
            public string Wednesday { get; set; }
            public string Thursday { get; set; }
            public string Friday { get; set; }
            public string Saturday { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UIDIndex { get; set; }
            public string UIDIndex2 { get; set; }
            public string UIDIndex3 { get; set; }
        }
    }
}
