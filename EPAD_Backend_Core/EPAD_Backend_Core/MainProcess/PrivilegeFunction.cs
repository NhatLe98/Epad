using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.MainProcess
{
    public class PrivilegeFunction
    {
        public static bool CheckPrivilegeForAction(UserInfo user, Microsoft.AspNetCore.Http.HttpContext context)
        {
            // nếu là tk service --> pass
            if (user.Index > 0)
                return true;
            
            string formName = "";
            bool pass = false;
            
            for (int i = 0; i < context.Request.Headers.Keys.Count; i++)
            {
                if (context.Request.Headers.Keys.ElementAt(i).ToLower() == "form-name")
                {
                    formName = context.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            // request ko có formname --> pass
            if (formName == "" || formName == "hanwhaCameraTest")
                return true;
           
            var listPrivilege = user.ListPrivilege;
            string method = context.Request.Method.ToUpper();
            for (int i = 0; i < listPrivilege.Count; i++)
            {
                if (listPrivilege[i].FormName == formName)
                {
                    if (method == "GET")
                    {
                        if(listPrivilege[i].Roles.Contains(FormRole.ReadOnly) || listPrivilege[i].Roles.Contains(FormRole.Full) || listPrivilege[i].Roles.Contains(FormRole.Edit))
                        {
                            pass = true;
                        }
                    }
                    else
                    {
                        if (listPrivilege[i].Roles.Contains(FormRole.Full) || listPrivilege[i].Roles.Contains(FormRole.Edit))
                        {
                            pass = true;
                            if (listPrivilege[i].Roles.Contains(FormRole.Edit))
                            {
                                if (context.Request.Path.ToString().Contains("Delete") || context.Request.Path.ToString().Contains("Remove"))
                                {
                                    pass = false;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return pass;
        }
    }
}