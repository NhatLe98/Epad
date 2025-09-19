using AutoMapper.Configuration;
using EPAD_Common.Types;
using EPAD_Data;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPAD_Common.Extensions;

namespace EPAD_Common.Utility
{
    public static class StringHelper
    {
        public static string UrnAuthInfoPrefix = "urn:authinfo-";
        public static string UrnAppLicensePrefix = "urn:license-key-company-";
        public static string UrnHWLicensePrefix = "urn:hardwarelicenseinfo-";
        public static string ComputerIdentifyKey = "urn:computeridentify";

        public static string EmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        public static string UserNameClaimType = "preferred_username";
        public static string SystemTypeClaimType = "system_type";
        public static string NameIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public static AppLicenseInfoPure GetLicenseInfoPure(string pKey, string pEncryptData)
        {
            try
            {
                string decryptStr = AbriLicenseDecryptor.AbriLicenseDecryptor.Decrypt(pEncryptData, pKey);
                AppLicenseInfoPure appLicenseInfo = JsonConvert.DeserializeObject<AppLicenseInfoPure>(decryptStr);
                return appLicenseInfo;
            }
            catch
            {
                return null;
            }
        }

        public static int GetDateOfBirth(string dayOrMonthOrYear, string dateOfBirth)
        {
            var result = 0;
            var defaultDate = DateTime.Parse("1900-01-01");
            if (!string.IsNullOrWhiteSpace(dateOfBirth))
                defaultDate = DateTime.Parse(dateOfBirth);
            switch (dayOrMonthOrYear)
            {
                case "Day":
                    result = defaultDate.Day;
                    break;
                case "Month":
                    result = defaultDate.Month;
                    break;
                case "Year":
                    result = defaultDate.Year;
                    break;
            }
            return result;
        }

        public static int GetDateOfBirthEmployee(string dayOrMonthOrYear, string dateOfBirth)
        {
            var result = 0;
            var defaultDate = DateTime.ParseExact("1900-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(dateOfBirth))
                defaultDate = DateTime.ParseExact(dateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            switch (dayOrMonthOrYear)
            {
                case "Day":
                    result = defaultDate.Day;
                    break;
                case "Month":
                    result = defaultDate.Month;
                    break;
                case "Year":
                    result = defaultDate.Year;
                    break;
            }
            return result;
        }


        public static HWLicenseInfo GetHWLicenseInfo(string pKey, string pEncryptData)
        {
            try
            {
                string decryptStr = AbriLicenseDecryptor.AbriLicenseDecryptor.Decrypt(pEncryptData, pKey);
                HWLicenseInfo lcInfo = JsonConvert.DeserializeObject<HWLicenseInfo>(decryptStr);
                return lcInfo;
            }
            catch
            {
                return null;
            }
        }

        public static string GetCommandName(string commandCode)
        {
            var result = "";
            switch (commandCode)
            {
                case GlobalParams.ValueFunction.UploadUsers:
                    result = "Cập nhật thông tin nhân viên";
                    break;
                case GlobalParams.ValueFunction.DownloadAllUser:
                    result = "Tải tất cả thông tin nhân viên";
                    break;
                case GlobalParams.ValueFunction.DownloadLogFromToTime:
                    result = "Tải log nhân viên từ ngày tới ngày";
                    break;
                case GlobalParams.ValueFunction.DeleteAllUser:
                    result = "Xóa tất cả thông tin nhân viên";
                    break;
                case GlobalParams.ValueFunction.DeleteAllFingerPrint:
                    result = "Xóa tất cả vân tay";
                    break;
                case GlobalParams.ValueFunction.DeleteAllLog:
                    result = "Xóa tất cả các log";
                    break;
                case GlobalParams.ValueFunction.DeleteLogFromToTime:
                    result = "Xóa log từ ngày tới ngày";
                    break;
                case GlobalParams.ValueFunction.DeleteUserById:
                    result = "Xóa nhân viên bằng ID";
                    break;
                case GlobalParams.ValueFunction.GetDeviceInfo:
                    result = "Tải thông tin thiết bị";
                    break;
                case GlobalParams.ValueFunction.RestartDevice:
                    result = "Khởi động lại thiết bị";
                    break;
                case GlobalParams.ValueFunction.RESTART_SERVICE:
                    result = "Khởi động service";
                    break;
            }
            return result;
        }

        public static string GetCommandType(int employetype)
        {
            switch (employetype)
            {
                case 1:
                    return CommandName.UploadUsers.ToString();
                case 2:
                    return CommandName.UploadCustomers.ToString();
                case 3:
                    return CommandName.UploadParents.ToString();
                case 4:
                    return CommandName.UploadStudents.ToString();
                default:
                    return null;
            }
        }

        public static T RemoveWhiteSpace<T>(T pObj)
        {
            PropertyInfo[] arrProperty = pObj.GetType().GetProperties();
            foreach (PropertyInfo item in arrProperty)
            {
                if (item.PropertyType == typeof(string))
                {
                    item.SetValue(pObj, item.GetValue(pObj, null)?.ToString().Trim());
                }
            }
            return pObj;
        }

        public static string MD5(string pPass)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hashData = md5.ComputeHash(Encoding.UTF8.GetBytes(pPass));
            var sb = new StringBuilder();
            foreach (var hashByte in hashData)
            {
                sb.AppendFormat("{0:x2}", hashByte);
            }
            return sb.ToString();
        }

        public static string SHA1(string pPass)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] hashData = sha1.ComputeHash(Encoding.UTF8.GetBytes(pPass));
            var sb = new StringBuilder();
            foreach (var hashByte in hashData)
            {
                sb.AppendFormat("{0:x2}", hashByte);
            }
            return sb.ToString();
        }

        public static string SHA256(string src)
        {
            using (SHA512 hash = SHA512.Create())
            {
                byte[] hashData = hash.ComputeHash(Encoding.UTF8.GetBytes(src));
                var sb = new StringBuilder();
                foreach (var hashByte in hashData)
                {
                    sb.AppendFormat("{0:x2}", hashByte);
                }
                return sb.ToString();
            }
        }

        public static string GetInOutModeString(short inOutMode)
        {
            string value = "Other";
            switch (inOutMode)
            {
                case 0:
                    value = "In"; break;
                case 1:
                    value = "Out"; break;
                case 2:
                    value = "BreakOut"; break;
                case 3:
                    value = "BreakIn"; break;
            }
            return value;
        }

        public static string ConvertToString(string inOutMode)
        {
            string value = "5";
            switch (inOutMode.ToLower())
            {
                case "in":
                    value = "0"; break;
                case "out":
                    value = "1"; break;
                case "breakout":
                    value = "2"; break;
                case "breakin":
                    value = "3"; break;
                case "0":
                case "1":
                case "2":
                case "3":
                    value = inOutMode; break;
            }
            return value;
        }

        public static string GetVerifyModeString(short? verifyMode)
        {
            string value = "Other";
            switch (verifyMode)
            {
                case 1:
                    value = "Finger"; break;
                case 2:
                    value = "PIN"; break;
                case 3:
                    value = "Password"; break;
                case 4:
                    value = "Card"; break;
                case 15:
                    value = "FaceTemplate"; break;
                default: value = ""; break;
            }
            return value;
        }

        public static string GetFaceMaskString(int? faceMask)
        {
            string value = "";
            switch (faceMask)
            {
                case 0:
                    value = "NoFaceMask"; break;
                case 1:
                    value = "HaveFaceMask"; break;
                case 255:
                    value = ""; break;
            }
            return value;
        }

        public static string GetBodyTemperatureString(double? bodyTemperature)
        {
            string value = "";
            switch (bodyTemperature)
            {
                case 255:
                    value = ""; break;
                default:
                    value = bodyTemperature.ToString(); break;
            }
            return value;
        }

        public static bool GetIsOverBodyTemperature(double? bodyTemperature, double defaultBodyTemperature)
        {
            if (bodyTemperature.HasValue)
            {
                switch (bodyTemperature)
                {
                    case 255:
                        return false;
                    default:
                        if (bodyTemperature > defaultBodyTemperature)
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        public static bool IsValidEmail(string pEmail)
        {
            try
            {
                var email = new System.Net.Mail.MailAddress(pEmail);
                return email.Address == pEmail;
            }
            catch
            {
                return false;
            }
        }

        public static string ConvertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string ConvertByteArrayToBaseString(byte[] arr)
        {
            string data = "";
            try
            {
                var stream = new MemoryStream(arr);
                var img = Image.FromStream(stream);

                if (arr.Length > 0)
                {
                    data = Convert.ToBase64String(arr);
                    data = $"data:image/{img.RawFormat.ToString().ToLower()};base64," + data;
                }
            }
            catch
            {
                return "";
            }
            return data;
        }

        public static string ConvertToUnSign2(string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }
        public static byte ToByte(this object value)
        {
            byte result;
            byte.TryParse(value.ToString(), out result);
            return result;
        }

        public static short ToShort(this object value)
        {
            short result;
            short.TryParse(value.ToString(), out result);
            return result;
        }

        public static int ToInt(this object value)
        {
            int result;
            int.TryParse(value.ToString(), out result);
            return result;
        }

        public static long ToLong(this object value)
        {
            long result;
            long.TryParse(value.ToString(), out result);
            return result;
        }

        public static decimal ToDecimal(this object value)
        {
            decimal result;
            decimal.TryParse(value.ToString(), out result);
            return result;
        }

        public static float ToFloat(this object value)
        {
            float result;
            float.TryParse(value.ToString(), out result);
            return result;
        }

        public static double ToDouble(this object value)
        {
            double result;
            double.TryParse(value.ToString(), out result);
            return result;
        }

        public static bool TryGetBoolValue(this string pValue)
        {
            pValue = pValue.Trim().Replace("\'", "");
            bool data = false;
            if (pValue == "1" || pValue.ToLower() == "true")
            {
                data = true;
            }
            return data;
        }

        public static bool? TryGetBoolValueWithNull(this string pValue)
        {
            bool? data = null;
            if (pValue == "1" || pValue.ToLower() == "true")
            {
                data = true;
            }
            else if (pValue == "0" || pValue.ToLower() == "false")
            {
                data = false;
            }

            return data;
        }

        public static double TryGetDoubleValue(this string pValue)
        {
            double data = 0;
            double.TryParse(pValue, out data);
            return data;
        }
        public static int TryGetIntValue(this string pValue)
        {
            int data = 0;
            int.TryParse(pValue, out data);
            return data;
        }

        public static int TryGetInOutMode(this string pValue)
        {
            int data = 1;
            int.TryParse(pValue, out data);
            return data;
        }

        public static long TryGetLongValue(this string pValue)
        {
            long data = 0;
            long.TryParse(pValue, out data);
            return data;
        }

        public static string TryGetTimeSpanString(this string pvalue)
        {
            double dMinutes = pvalue.TryGetDoubleValue();
            TimeSpan ts = TimeSpan.FromMinutes(dMinutes);
            return ts.ToString();
        }

        public static List<string> RemoveEmptyString(this string[] pSource)
        {
            return pSource.Where(x => x != "").ToList();
        }

        public static string RemoveAccents(this string input)
        {
            return new string(input
                .Normalize(System.Text.NormalizationForm.FormD)
                .ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }

        public static string TryGetManagedOtherDepartment(this string value)
        {
            List<string> result = new List<string>();
            var lstDept = value.Split(';');
            for (int i = 0; i < lstDept.Length; i++)
            {
                var dept = lstDept[i].Split('|');
                if (dept.Length > 1)
                {
                    result.Add(dept[1]);
                }
            }

            return string.Join(", ", result);
        }

        public static int TryGetScaleValue(this string source, char splitChar, int index)
        {
            int output = 0;
            var arr = source.Split(splitChar);
            try
            {
                int.TryParse(arr[index], out output);
            }
            catch (Exception)
            {
            }

            return output;
        }

        public static string TryGetEvaluatePeriodName(this short? period)
        {
            if (period.HasValue == false || period.Value == 1) return "Monthly";
            if (period.Value == 2) return "Quaterly";
            if (period.Value == 3) return "Yearly";
            return "Monthly";
        }

        public static bool SplitAndCheckContain(this string src, char delimiter, string key)
        {
            var lst = src.Split(delimiter);
            var rs = lst.Any(x => x.Trim() == key);
            return rs;
        }

        #region Encrypt/ Decrypt Data
        // Encrypt a byte array into a byte array using a key and an IV 

        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {

            // Create a MemoryStream that is going to accept the encrypted bytes 

            MemoryStream ms = new MemoryStream();



            // Create a symmetric algorithm. 

            // We are going to use Rijndael because it is strong and available on all platforms. 

            // You can use other algorithms, to do so substitute the next line with something like 

            //                      TripleDES alg = TripleDES.Create(); 

            Rijndael alg = Rijndael.Create();



            // Now set the key and the IV. 

            // We need the IV (Initialization Vector) because the algorithm is operating in its default 

            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte) 

            // of the data before it is encrypted, and then each encrypted block is XORed with the 

            // following block of plaintext. This is done to make encryption more secure. 

            // There is also a mode called ECB which does not need an IV, but it is much less secure. 

            alg.Key = Key;

            alg.IV = IV;



            // Create a CryptoStream through which we are going to be pumping our data. 

            // CryptoStreamMode.Write means that we are going to be writing data to the stream 

            // and the output will be written in the MemoryStream we have provided. 

            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);



            // Write the data and make it do the encryption 

            cs.Write(clearData, 0, clearData.Length);



            // Close the crypto stream (or do FlushFinalBlock). 

            // This will tell it that we have done our encryption and there is no more data coming in, 

            // and it is now a good time to apply the padding and finalize the encryption process. 

            cs.Close();



            // Now get the encrypted data from the MemoryStream. 

            // Some people make a mistake of using GetBuffer() here, which is not the right way. 

            byte[] encryptedData = ms.ToArray();



            return encryptedData;

        }


        // Encrypt a string into a string using a password 

        //    Uses Encrypt(byte[], byte[], byte[]) 

        public static string Encrypt(string clearText, string Password)
        {

            // First we need to turn the input string into a byte array. 

            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);



            // Then, we need to turn the password into Key and IV 

            // We are using salt to make it harder to guess our key using a dictionary attack - 

            // trying to guess a password by enumerating all possible words. 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            // Now get the key/IV and do the encryption using the function that accepts byte arrays. 

            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 

            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 

            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 

            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 

            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 

            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));



            // Now we need to turn the resulting byte array into a string. 

            // A common mistake would be to use an Encoding class for that. It does not work 

            // because not all byte values can be represented by characters. 

            // We are going to be using Base64 encoding that is designed exactly for what we are 

            // trying to do. 

            return Convert.ToBase64String(encryptedData);



        }


        // Encrypt bytes into bytes using a password 

        //    Uses Encrypt(byte[], byte[], byte[]) 

        public static byte[] Encrypt(byte[] clearData, string Password)
        {

            // We need to turn the password into Key and IV. 

            // We are using salt to make it harder to guess our key using a dictionary attack - 

            // trying to guess a password by enumerating all possible words. 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            // Now get the key/IV and do the encryption using the function that accepts byte arrays. 

            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 

            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 

            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 

            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 

            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 

            return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16));



        }


        // Encrypt a file into another file using a password 

        public static void Encrypt(string fileIn, string fileOut, string Password)
        {

            // First we are going to open the file streams 

            FileStream fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);

            FileStream fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);



            // Then we are going to derive a Key and an IV from the Password and create an algorithm 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            Rijndael alg = Rijndael.Create();



            alg.Key = pdb.GetBytes(32);

            alg.IV = pdb.GetBytes(16);



            // Now create a crypto stream through which we are going to be pumping data. 

            // Our fileOut is going to be receiving the encrypted bytes. 

            CryptoStream cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write);



            // Now will will initialize a buffer and will be processing the input file in chunks. 

            // This is done to avoid reading the whole file (which can be huge) into memory. 

            int bufferLen = 4096;

            byte[] buffer = new byte[bufferLen];

            int bytesRead;



            do
            {

                // read a chunk of data from the input file 

                bytesRead = fsIn.Read(buffer, 0, bufferLen);



                // encrypt it 

                cs.Write(buffer, 0, bytesRead);



            } while (bytesRead != 0);



            // close everything 

            cs.Close(); // this will also close the unrelying fsOut stream 

            fsIn.Close();

        }


        // Decrypt a byte array into a byte array using a key and an IV 

        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {

            // Create a MemoryStream that is going to accept the decrypted bytes 

            MemoryStream ms = new MemoryStream();



            // Create a symmetric algorithm. 

            // We are going to use Rijndael because it is strong and available on all platforms. 

            // You can use other algorithms, to do so substitute the next line with something like 

            //                      TripleDES alg = TripleDES.Create(); 

            Rijndael alg = Rijndael.Create();



            // Now set the key and the IV. 

            // We need the IV (Initialization Vector) because the algorithm is operating in its default 

            // mode called CBC (Cipher Block Chaining). The IV is XORed with the first block (8 byte) 

            // of the data after it is decrypted, and then each decrypted block is XORed with the previous 

            // cipher block. This is done to make encryption more secure. 

            // There is also a mode called ECB which does not need an IV, but it is much less secure. 

            alg.Key = Key;

            alg.IV = IV;



            // Create a CryptoStream through which we are going to be pumping our data. 

            // CryptoStreamMode.Write means that we are going to be writing data to the stream 

            // and the output will be written in the MemoryStream we have provided. 

            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);



            // Write the data and make it do the decryption 

            cs.Write(cipherData, 0, cipherData.Length);



            // Close the crypto stream (or do FlushFinalBlock). 

            // This will tell it that we have done our decryption and there is no more data coming in, 

            // and it is now a good time to remove the padding and finalize the decryption process. 

            cs.Close();



            // Now get the decrypted data from the MemoryStream. 

            // Some people make a mistake of using GetBuffer() here, which is not the right way. 

            byte[] decryptedData = ms.ToArray();



            return decryptedData;

        }


        // Decrypt a string into a string using a password 

        //    Uses Decrypt(byte[], byte[], byte[]) 

        public static string Decrypt(string cipherText, string Password)
        {

            // First we need to turn the input string into a byte array. 

            // We presume that Base64 encoding was used 

            byte[] cipherBytes = Convert.FromBase64String(cipherText);



            // Then, we need to turn the password into Key and IV 

            // We are using salt to make it harder to guess our key using a dictionary attack - 

            // trying to guess a password by enumerating all possible words. 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            // Now get the key/IV and do the decryption using the function that accepts byte arrays. 

            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 

            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 

            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 

            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 

            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 

            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));



            // Now we need to turn the resulting byte array into a string. 

            // A common mistake would be to use an Encoding class for that. It does not work 

            // because not all byte values can be represented by characters. 

            // We are going to be using Base64 encoding that is designed exactly for what we are 

            // trying to do. 

            return System.Text.Encoding.Unicode.GetString(decryptedData);



        }


        // Decrypt bytes into bytes using a password 

        //    Uses Decrypt(byte[], byte[], byte[]) 

        public static byte[] Decrypt(byte[] cipherData, string Password)
        {

            // We need to turn the password into Key and IV. 

            // We are using salt to make it harder to guess our key using a dictionary attack - 

            // trying to guess a password by enumerating all possible words. 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            // Now get the key/IV and do the Decryption using the function that accepts byte arrays. 

            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key 

            // (the default Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV. 

            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael. 

            // If you are using DES/TripleDES/RC2 the block size is 8 bytes and so should be the IV size. 

            // You can also read KeySize/BlockSize properties off the algorithm to find out the sizes. 

            return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16));



        }


        // Decrypt a file into another file using a password 

        public static void Decrypt(string fileIn, string fileOut, string Password)
        {

            // First we are going to open the file streams 

            FileStream fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);

            FileStream fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);



            // Then we are going to derive a Key and an IV from the Password and create an algorithm 

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });



            Rijndael alg = Rijndael.Create();



            alg.Key = pdb.GetBytes(32);

            alg.IV = pdb.GetBytes(16);



            // Now create a crypto stream through which we are going to be pumping data. 

            // Our fileOut is going to be receiving the Decrypted bytes. 

            CryptoStream cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write);



            // Now will will initialize a buffer and will be processing the input file in chunks. 

            // This is done to avoid reading the whole file (which can be huge) into memory. 

            int bufferLen = 4096;

            byte[] buffer = new byte[bufferLen];

            int bytesRead;



            do
            {

                // read a chunk of data from the input file 

                bytesRead = fsIn.Read(buffer, 0, bufferLen);



                // Decrypt it 

                cs.Write(buffer, 0, bytesRead);



            } while (bytesRead != 0);



            // close everything 

            cs.Close(); // this will also close the unrelying fsOut stream 

            fsIn.Close();

        }

        #endregion
    }
}
