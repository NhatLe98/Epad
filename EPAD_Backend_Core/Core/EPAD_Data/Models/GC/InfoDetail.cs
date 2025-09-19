using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class InfoDetail
    {
        public string Title { get; set; }
        public string Data { get; set; }
        public InfoDetail(string pTitle, string pData)
        {
            Title = pTitle;
            Data = pData;
        }
    }
}
