using EPAD_Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.Locales
{
    public interface ILocales
    {
        void SetLanguage(Language pLang);
        string GetCheckedString(string pKey);
        string GetCheckedStringWithLang(string pKey, Language plang = Language.VI);
        bool AddResources(string pKey, string pVI, string pEN, out string pMessage);
        void InitLanguage();
    }
}
