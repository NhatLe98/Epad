using System.Collections.Generic;
using System.Data;

namespace EPAD_Common.Types
{
    public class StoreProcedureInfo
    {
        public string Name { get; set; }
        public ICollection<Parameter> Params { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public object Value { get; set; }
    }
}
