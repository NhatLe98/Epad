using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_PrinterService : IBaseServices<IC_Printer, EPAD_Context>
    {
        List<IC_Printer> GetAllPrinterInfo();
        IC_Printer CreatePrinter(IC_Printer printer);
        (List<IC_Printer>, int) GetPrinters(string searchValue, int page, int pageSize);
        IC_Printer GetPrinterBySerialNumber(string serialNumber);
        string GetPrinterNameBySerialNumber(string serialNumber);
    }
}
