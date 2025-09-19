using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EzFile
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class EzFileRequestWithType
    {
        public int Index { get; set; }
        public List<EzFile> Attachments { get; set; }
        public int Type { get; set; }
    }
    public class EzFileRequestByListIndexWithType
    {
        public List<int> Indexs { get; set; }
        public List<EzFile> Attachments { get; set; }
        public int Type { get; set; }
    }

    public class UploadConfiguration
    {
        public string Root { get; set; }
    }

    public class EzFileRequest
    {
        public int Index { get; set; }
        public List<EzFileAttactment> Attachments { get; set; }
    }

    public class EzFileRequestSimple
    {
        public int Index { get; set; }
        public List<EzFile> Attachments { get; set; }
    }

    public class EzFileAttactment
    {
        public string Type { get; set; }
        public EzFile EzFile { get; set; }
    }
}
