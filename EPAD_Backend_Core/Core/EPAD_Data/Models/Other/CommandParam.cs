using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CommandParam
    {
        public string ID { get; set; }
        public string SDKFuntion { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        /// <summary>
        ///     Danh sách userId command thực hiện thành công, phân tách nhau bởi dấu phẩy (,)
        /// </summary>
        public string DataSuccess { get; set; } = string.Empty;
        /// <summary>
        ///     Danh sách userId command thực hiện xảy ra exception, phân tách nhau bởi dấu phẩy (,)
        /// </summary>
        public string DataFailure { get; set; } = string.Empty;
    }
}
