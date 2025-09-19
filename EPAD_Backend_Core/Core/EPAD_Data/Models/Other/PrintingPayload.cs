using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class PrintingPayload
    {
        public IFormFile File { get; set; }
        public string SerialNumber { get; set; }
    } 
}
