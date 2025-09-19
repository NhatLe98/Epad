using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Constants
{
    public class FR05ApiConst
    {
        public const string RestartDevice = "restartDevice";
        public const string SetTime = "setTime";
        public const string DeleteAllLog = "deleteRecords";
        public const string DownloadAllLog = "findRecords";
        public const string DeleteLogByTime = "newDeleteRecords";
        public const string DeleteAllUser = "person/delete";
        public const string DeleteFingerData = "api/v2/finger/delete";
        public const string DeleteFaceData = "face/delete";
        public const string FindFingerPrint = "api/v2/finger/find";
        public const string FindUser = "person/find";
        public const string FindFace = "face/find";
        public const string CreateUser = "person/create";
        public const string CreateFace = "face/create";
        public const string NewDownloadAllLog = "newFindRecords";
        public const string CreateFinger = "api/v2/finger/create";
    }
}
