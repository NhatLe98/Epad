using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class ListDTOModel<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Data { get; set; }
    }
}
