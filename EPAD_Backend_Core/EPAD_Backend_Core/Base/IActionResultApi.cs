using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Base
{
    public class EResult<T>
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public T Data { get; set; }
    }
}
