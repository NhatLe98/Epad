using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_PrinterService : BaseServices<IC_Printer, EPAD_Context>, IIC_PrinterService
    {
        public IC_PrinterService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public List<IC_Printer> GetAllPrinterInfo()
        {
            return DbContext.IC_Printer.ToList();
        }

        public IC_Printer CreatePrinter(IC_Printer printer)
        {
            var entity = Repository.Insert(printer);
            UnitOfWork.SaveChanges();
            return entity;
        }

        public (List<IC_Printer>, int) GetPrinters(string searchValue, int page, int pageSize)
        {
            IQueryable<IC_Printer> printers = Repository.GetAll().AsNoTracking();
            if (!string.IsNullOrEmpty(searchValue?.Trim()))
            {
                searchValue = searchValue.ToLower().Trim();
                printers = printers.Where(x => searchValue.Contains(x.Name.ToLower()) 
                    || searchValue.Contains(x.SerialNumber.ToLower()) || searchValue.Contains(x.IPAddress)
                    || x.Name.ToLower().Contains(searchValue) || x.SerialNumber.ToLower().Contains(searchValue) 
                    || x.IPAddress.Contains(searchValue));
            }

            int totalItem = printers.Count();
            printers = printers.Skip((page - 1) * pageSize).Take(pageSize);

            return (printers.ToList(), totalItem);
        }

        public IC_Printer GetPrinterBySerialNumber(string serialNumber)
        {
            return DbContext.IC_Printer.FirstOrDefault(e => e.SerialNumber == serialNumber);
        }

        public string GetPrinterNameBySerialNumber(string serialNumber)
        {
            return DbContext.IC_Printer.FirstOrDefault(e => e.SerialNumber == serialNumber).Name;
        }
    }
}
