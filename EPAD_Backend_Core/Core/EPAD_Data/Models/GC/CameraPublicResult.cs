using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CameraPublicResult
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Serial { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
    }

    public class CameraPictureResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Link { get; set; }
    }

    public class CameraStreamLinkResult
    {
        public int Index { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
    }

    public class CameraANPRResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Picture { get; set; }
        public string LCR_Picture { get; set; }
        public string LCR { get; set; }
        public CameraANPRResult(bool pSuccess, string pError, string pPicture, string pLCR_Picture, string pLCR)
        {
            Success = pSuccess;
            Error = pError;
            Picture = pPicture;
            LCR_Picture = pLCR_Picture;
            LCR = pLCR;
        }
    }
}
