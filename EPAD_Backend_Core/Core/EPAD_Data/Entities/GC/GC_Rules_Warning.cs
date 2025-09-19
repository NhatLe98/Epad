using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_Warning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        public bool? UseSpeaker { get; set; }

        public bool? UseSpeakerFocus { get; set; }
        public bool? UseSpeakerInPlace { get; set; }
        //public int? SpeakerController { get; set; }
        //public int? SpeakerChannel { get; set; }
        //[StringLength(3000)]
        //public string SpeakerDescription { get; set; }


        public bool? UseLed { get; set; }
        public bool? UseLedFocus { get; set; }
        public bool? UseLedInPlace { get; set; }
        //public int? LedController { get; set; }
        //public int? LedChannel { get; set; }
        //[StringLength(3000)]
        //public string LedDescription { get; set; }


        public bool? UseEmail { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; }
        public short? EmailSendType { get; set; } // 0: Gửi ngay, 1: Theo lịch
        //public string EmailSchedule { get; set; } // Json lịch gửi mail. format: [ {"DayOfWeekIndex hh:mm"}, {"DayOfWeekIndex hh:mm"}, ...  ]

        public bool? UseComputerSound { get; set; }
        public string ComputerSoundPath { get; set; }


        public bool? UseChangeColor { get; set; }




        [Required]
        public int CompanyIndex { get; set; }
        [Required]
        public int RulesWarningGroupIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
