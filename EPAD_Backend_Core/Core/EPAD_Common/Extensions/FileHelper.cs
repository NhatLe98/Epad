using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EPAD_Common.Extensions
{
    public static class FileHelper
    {
        private static string encodeStr = "THS";
        private static byte[] encodeBytes = Encoding.ASCII.GetBytes(encodeStr);
        public static byte[] ReadAndDecodeToBytes(string path)
        {
            return File.ReadAllBytes(path)
                .Skip(encodeBytes.Length)
                .Reverse()
                .ToArray();
        }
        public static void CreateForderIfNotExists(string path)
        {
            var exists = Directory.Exists(path);
            if (!exists)
                Directory.CreateDirectory(path);
        }

        public static long FileUniqueIndex(string path, string fileName, string spriter)
        {
            var arr = GetAllFileName(path).Select(e => e.Split('\\').Last())
                .Where(e => e.StartsWith(fileName)).ToArray();
            if (arr.Length == 0)
            {
                return 0;
            }
            return arr.Select(e =>
            {
                var temp = e.Split(new string[] { spriter }, StringSplitOptions.None);
                var index = Int64.Parse(temp[temp.Length - 2]);
                return index;
            }).Max() + 1;
        }

        public static void WriteAndEncodeFileFromBase64(string path, string fileName, string base64)
        {
            CreateForderIfNotExists(path);
            File.WriteAllBytes(path + @"\" + fileName,
                Convert.FromBase64String(base64)
                    .Concat(encodeBytes)
                    .Reverse()
                    .ToArray()
                );
        }

        public static string WriteAndEncodeFileFromByte(string path, string fileName, byte[] data)
        {
            CreateForderIfNotExists(path);
            File.WriteAllBytes(path + @"\" + fileName, data);
            return path + @"\" + fileName;
        }

        public static void DeleteFile(string path, string fileName)
        {
            var fullName = path + @"\" + fileName;
            if (File.Exists(fullName))
            {
                File.Delete(fullName);
            }
        }

        public static string[] GetAllFileName(string path)
        {
            if (!Directory.Exists(path))
            {
                return new string[] { };
            }
            return Directory.GetFiles(path);
        }
    }
}
