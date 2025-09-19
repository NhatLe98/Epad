using EPAD_Data.Entities;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class SystemCommandInfoMongo
    {
        public IC_CommandSystemGroup Group { get; set; }
        public List<IC_SystemCommand> ListCommand { get; set; }
    }
}
