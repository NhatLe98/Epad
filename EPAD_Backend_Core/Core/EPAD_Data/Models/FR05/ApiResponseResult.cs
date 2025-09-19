using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.FR05
{
    public class ApiResponseResult
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
        [JsonProperty("result")]
        public int Result { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class DeleteRecordsResult : ApiResponseResult
    {
        [JsonProperty("data")]
        public string Data { get; set; }
    }

    public class FindRecordsResult : ApiResponseResult
    {
        [JsonProperty("data")]
        public FindRecordsParamResult Data { get; set; }
    }

    public class FindRecordsParamResult
    {
        [JsonProperty("records")]
        public List<FindRecordData> Records { get; set; }
    }

    public class FindUsersResult : ApiResponseResult
    {
        [JsonProperty("data")]
        public List<FindUserData> Data { get; set; }
    }

    public class UserFaceResult : ApiResponseResult
    {
        [JsonProperty("data")]
        public List<UserFaceData> Data { get; set; }
    }

    public class FindFingerResult : ApiResponseResult
    {
        [JsonProperty("data")]
        public List<FindFingerData> Data { get; set; }
    }

    public class FindRecordData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("personId")]
        public string PersonId { get; set; }
        [JsonProperty("state")]
        public int State { get; set; }
        [JsonProperty("temperature")]
        public string Temperature { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("direction")]
        public int Direction { get; set; }
    }

    public class FindUserData
    {

        [JsonProperty("createTime")]
        public long CreateTime { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("idcardNum")]
        public string IdCardNum { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("vaccination")]
        public int Vaccination { get; set; }
    }

    public class UserFaceData
    {
        [JsonProperty("faceId")]
        public string FaceId { get; set; }
        [JsonProperty("imgBase64")]
        public string ImgBase64 { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("personId")]
        public string PersonId { get; set; }
    }

    public class FindFingerData
    {
        [JsonProperty("feature")]
        public string Feature { get; set; }
        [JsonProperty("fingerId")]
        public string FingerId { get; set; }
        [JsonProperty("fingerNum")]
        public int FingerNum { get; set; }
        [JsonProperty("personId")]
        public string PersonId { get; set; }
    }
}
