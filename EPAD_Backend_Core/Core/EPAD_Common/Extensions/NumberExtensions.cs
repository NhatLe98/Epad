using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.Extensions
{
    public static class NumberExtensions
    {
        public static bool? GetIsync(this short? itemCheck)
        {
            if (itemCheck.HasValue)
            {
                return itemCheck.Value == 1 ? true : false;
            }
            return null;
        }

        public static bool IsNumber(string Value)
        {
            if (string.IsNullOrEmpty(Value)) return false;
            foreach (char c in Value)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
