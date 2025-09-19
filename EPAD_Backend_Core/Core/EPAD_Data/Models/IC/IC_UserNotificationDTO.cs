using Newtonsoft.Json;
using System;

namespace EPAD_Data.Models
{
    public class IC_UserNotificationDTO
    {
        public int Index { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public short Type { get; set; }
        public short Status { get; set; }

        public MessageBodyDTO Data { get; set; }
        public IC_UserNotificationDTO(){
            Data = string.IsNullOrWhiteSpace(this.Message) ? null: JsonConvert.DeserializeObject<MessageBodyDTO>(this.Message);
        }
    }

    public class MessageBodyDTO
    {
        public string Message { get; set; }
        public string FromUser { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string Approver { get; set; }

    }
}
