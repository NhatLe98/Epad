using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.MainProcess.ImportProcess
{
    interface IImportProcess
    {
        string Process(EPAD_Context context,List<string> listFilePath, int companyIndex, string userName);
    }
}
