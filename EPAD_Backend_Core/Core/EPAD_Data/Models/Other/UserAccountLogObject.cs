using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class UserAccountLogObject
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public DateTime Time { get; set; }
        public string TimeString { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string FormName { get; set; }
        public string Detail { get; set; }
    }
}
