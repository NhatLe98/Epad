using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.FileProvider
{
    public interface IStoreFileProvider
    {
        bool SaveImageBase64(string imageBase64, string folder, string fileName, ref string error);
        string getPath(string internalPath);
        string getPath(string internalPath, int pCompanyIndex);
        EzFile uploadAndUpdateUrl(EzFile ezFile, string parentPath);
        string uploadAndUpdateUrlBase(byte[] data, string parentPath, string fileName);
    }
}
