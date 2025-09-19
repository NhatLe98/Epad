using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers.IC
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Printer/[action]")]
    [ApiController]
    public class IC_PrinterController : ApiControllerBase
    {
        private EPAD_Context _context;
        private IMemoryCache _cache;
        private readonly IIC_PrinterService _IC_PrinterService;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public IC_PrinterController(IServiceProvider provider, ILoggerFactory loggerFactory) : base(provider)
        {
            _context = TryResolve<EPAD_Context>();
            _cache = TryResolve<IMemoryCache>();
            _IC_PrinterService = TryResolve<IIC_PrinterService>();
            _webHostEnvironment = TryResolve<IWebHostEnvironment>();
            _logger = loggerFactory.CreateLogger<IC_PrinterController>();
        }

        [Authorize]
        [ActionName("GetAllPrinter")]
        [HttpGet]
        public IActionResult GetAllPrinter()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var listPrinter = _IC_PrinterService.GetAllPrinterInfo();
            return ApiOk(listPrinter);
        }

        [Authorize]
        [ActionName("GetPrinters")]
        [HttpGet]
        public IActionResult GetPrinters(string searchValue, int page = 1, int pageSize = 1000)
        {
            var data = _IC_PrinterService.GetPrinters(searchValue, page, pageSize);
            return Ok(new
            {
                Data = data.Item1,
                Total = data.Item2,
            });
        }


        [Authorize]
        [ActionName("Create")]
        [HttpPost]
        public IActionResult CreatePrinter([FromBody] IC_Printer printer)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            if (_IC_PrinterService.Any(x => x.Name == printer.Name && x.SerialNumber == printer.SerialNumber))
            {
                return BadRequest("DeviceNameIsExisted");
            }
            if (_IC_PrinterService.Any(x => x.SerialNumber == printer.SerialNumber))
            {
                return BadRequest("SerialNumberIsExisted");
            }

            printer.CreatedDate = DateTime.Now;
            printer.UpdatedUser = user.UserName;
            printer.CompanyIndex = user.CompanyIndex;
            var createdPrinter = _IC_PrinterService.CreatePrinter(printer);

            return StatusCode(StatusCodes.Status201Created, createdPrinter);
        }


        [Authorize]
        [ActionName("Delete")]
        [HttpDelete]
        public IActionResult DeletePrinter(int printerIndex)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            if (_IC_PrinterService.Any(x => x.Index == printerIndex && x.CompanyIndex == user.CompanyIndex) == false)
            {
                return NotFound();
            }

            _IC_PrinterService.Delete(printerIndex);
            _IC_PrinterService.UnitOfWork.SaveChanges();
            return NoContent();
        }

        [Authorize]
        [ActionName("DeleteMany")]
        [HttpDelete]
        public IActionResult DeleteManyPrinter([FromQuery] List<int> printerIndexes)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                var printerList = _IC_PrinterService
                    .Where(x => printerIndexes.Contains(x.Index) && x.CompanyIndex == user.CompanyIndex).ToList();
                //_IC_PrinterService.DbSet.RemoveRange(printerList);
                //_IC_PrinterService.UnitOfWork.SaveChanges();
                _context.IC_Printer.RemoveRange(printerList);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
            return NoContent();
        }

        [Authorize]
        [ActionName("Update")]
        [HttpPut]
        public IActionResult UpdatePrinter([FromBody] IC_Printer printer)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            IC_Printer entity = _IC_PrinterService.FirstOrDefault(x => x.Index == printer.Index && x.CompanyIndex == user.CompanyIndex);
            if (entity == null)
            {
                return NotFound();
            }

            if (_IC_PrinterService.Any(x => x.Name == printer.Name && x.SerialNumber != printer.SerialNumber))
            {
                return BadRequest("DeviceNameIsExisted");
            }

            //entity.SerialNumber = printer.SerialNumber;
            entity.IPAddress = printer.IPAddress;
            entity.PrinterModel = printer.PrinterModel;
            entity.Port = printer.Port;
            entity.Name = printer.Name;

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = user.UserName;

            _IC_PrinterService.UnitOfWork.SaveChanges();
            return Ok(entity);
        }


        /// <summary>
        ///     Prerequisites: Install printer's driver & Connect printer with LAN.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
#pragma warning disable CA1416 // Validate platform compatibility
        [ActionName("PrintFromExcel")]
        [HttpPost]
        public IActionResult PrintFromExcel([FromForm] PrintingPayload payload)
        {
            if (string.IsNullOrEmpty(payload.SerialNumber))
                return BadRequest("serialNumber cannot empty!");
            
            try
            {
                Workbook wb = new();
                wb.LoadFromStream(payload.File.OpenReadStream());
                using System.Drawing.Printing.PrintDocument printDocument = wb.PrintDocument;

                var printerName = _IC_PrinterService.GetPrinterNameBySerialNumber(payload.SerialNumber);
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("ZPrinter Paper", 1180, 1180);
                //printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(10, 10, 10, 10);

                printDocument.Print();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
            return NoContent();
        }

        [ActionName("TestPrinter")]
        [HttpPost]
        public IActionResult TestPrinter(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
            {
                return BadRequest("printerName cannot empty!");
            }
            try
            {
                string sWebRootFolder = _webHostEnvironment.ContentRootPath;
                FileInfo file = new(Path.Combine(sWebRootFolder, @"Files/PrinterTestingTemplate.xlsx"));

                Workbook wb = new();
                wb.LoadFromStream(file.OpenRead());
                using System.Drawing.Printing.PrintDocument printDocument = wb.PrintDocument;

                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("ZPrinter Paper", 1180, 1180);

                printDocument.Print();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
            return NoContent();
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
