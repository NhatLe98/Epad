using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.FR05
{
    public class CommonModel
    {
        [JsonProperty("pass")]
        public string Pass { get; set; }
    }

    public class SetTimeParam : CommonModel
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }

    public class DeleteLogParam : CommonModel
    {
        [JsonProperty("time")]
        public string Time { get; set; }
    }

    public class DeleteLogByTime : CommonModel
    {
        [JsonProperty("personId")]
        public string PersonId { get; set; }
        [JsonProperty("startTime")]
        public string StartTime { get; set; }
        [JsonProperty("endTime")]
        public string EndTime { get; set; }
        [JsonProperty("model")]
        public int Model { get; set; }
    }

    public class FindRecordsParam : CommonModel
    {
        [JsonProperty("personId")]
        public string PersonId { get; set; }
        [JsonProperty("length")]
        public int Length { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("startTime")]
        public string StartTime { get; set; }
        [JsonProperty("endTime")]
        public string EndTime { get; set; }
    }

    public class NewFindRecordsParam : FindRecordsParam
    {
        [JsonProperty("model")]
        public int Model { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
    }

    public class FingerDeleteParam : CommonModel
    {
        [JsonProperty("fingerId")]
        public string FingerId { get; set; }
    }

    public class FaceDeleteParam : CommonModel
    {
        [JsonProperty("faceId")]
        public string FaceId { get; set; }
    }

    public class UserDeleteParam : CommonModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class UserFindParam : CommonModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class UserFingerParam : CommonModel
    {
        [JsonProperty("personId")]
        public string PersonId { get; set; }
    }

    public class UserUploadParam : CommonModel
    {
        [JsonProperty("person")]
        public string Person { get; set; }
    }

    public class UserFaceCreate : CommonModel
    {
        [JsonProperty("personId")]
        public string PersonId { get; set; }
        [JsonProperty("imgBase64")]
        public string ImgBase64 { get; set; }
    }

    public class UserFingerUploadParam : CommonModel
    {
        [JsonProperty("data")]
        public string Data { get; set; }
    }

}
