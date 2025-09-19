using System;
using System.Collections.Generic;

namespace EPAD_Data.Entities
{
    public partial class HR_Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameInEng { get; set; }
        public string Address { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Mail { get; set; }
        public string Website { get; set; }
        public string TaxCode { get; set; }
        public string Slogan { get; set; }
        public string Description { get; set; }
        public bool? Headquaters { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public string AccountNo { get; set; }
        public string BankInfo { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public int Index { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; }
    }
}
