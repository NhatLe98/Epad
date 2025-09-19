using MongoDB.Bson;
using System;

namespace EPAD_Data.Models
{
    public class UserAccountLog
    {
        public ObjectId _id { get; set; }
        public DateTime Time { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string FormName { get; set; }
        public string Detail { get; set; }
    }
}
