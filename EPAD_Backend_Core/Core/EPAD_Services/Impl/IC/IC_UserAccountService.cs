using EPAD_Common.Services;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace EPAD_Services.Impl
{
    public class IC_UserAccountService : BaseServices<IC_UserAccount, EPAD_Context>, IIC_UserAccountService
    {
        private EPAD_Context _dbContext;
        private ConfigObject _config;
        private IMemoryCache _iCache;
        public IC_UserAccountService(IServiceProvider serviceProvider, EPAD_Context dbContext, IMemoryCache cache) : base(serviceProvider)
        {
            _dbContext = dbContext;
            _iCache = cache;
            _config = ConfigObject.GetConfig(_iCache);
        }

        public bool CheckAccountExisted(string username, string password)
        {
            string pass = StringHelper.SHA1(password);

            IC_UserAccount account = _dbContext.IC_UserAccount.Where(t => t.UserName == username && (t.Password == pass)).FirstOrDefault();
            if (account == null)
            {
                return false;
            }

            return true;
        }
    }
}
