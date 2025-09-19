using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_Audit
    {
        [Key]
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string UserName { get; set; }
        [StringLength(100)]
        public string TableName { get; set; }
        public DateTime DateTime { get; set; }
        [StringLength(200)]
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string AffectedColumns { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string State { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [MaxLength(1000)]
        public string DescriptionEn { get; set; }
        /// <summary>
        ///     FullName của người thực hiện lệnh, khi refactor sẽ bỏ cột này dùng tham chiếu qua UserName đến bảng IC_User
        /// </summary>
        [MaxLength(250)]
        public string Name { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }
        /// <summary>
        ///     Màn hình thực hiện hành động, giá trị bằng property list_menu[i].name 
        ///     trong file epad/static/variables/group-menu.json
        /// </summary>
        [Column(TypeName = "varchar(250)")]
        public string PageName { get; set; }
        /// <summary>
        ///     Số lượng record insert/update/delete thành công
        /// </summary>
        public int? NumSuccess { get; set; }
        /// <summary>
        ///     Số lượng record insert/update/delete thất bại
        /// </summary>
        public int? NumFailure { get; set; }
        [ForeignKey(nameof(IC_SystemCommand))]
        public int? IC_SystemCommandIndex { get; set; }

        public virtual IC_SystemCommand IC_SystemCommand { get; set; }

    }
}
