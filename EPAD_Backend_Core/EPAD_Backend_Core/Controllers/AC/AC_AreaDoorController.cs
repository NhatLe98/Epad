using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using EPAD_Backend_Core.Base;
using System;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using EPAD_Data;

namespace EPAD_Backend_Core.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_AreaDoor/[action]")]
    [ApiController]
    public class AC_AreaDoorController : ApiControllerBase
    {
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_AreaDoorController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            context = TryResolve<EPAD_Context>();
        }

        [Authorize]
        [ActionName("GetDoorInOutArea")]
        [HttpGet]
        public IActionResult GetDoorInOutArea(int areaIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var areaDoor = context.AC_AreaAndDoor.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            var notInDoor = areaDoor.Where(x => x.AreaIndex != areaIndex).Select(x => x.DoorIndex).ToList();
            var inDoor = areaDoor.Where(x => x.AreaIndex == areaIndex).Select(x => x.DoorIndex).ToList();

            var listDoor = context.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex && !notInDoor.Contains(x.Index));

            var listDoorRe = listDoor.Select(x => new DoorByArea
            {
                DoorIndex = x.Index,
                DoorName = x.Name,
                InArea = inDoor.Contains(x.Index)
            });
            result = Ok(listDoorRe);
            return result;
        }


        [Authorize]
        [ActionName("UpdateAreaDoorDetail")]
        [HttpPost]
        public IActionResult UpdateAreaDoorDetail([FromBody] GroupAreaParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
          
            context.AC_AreaAndDoor.RemoveRange(context.AC_AreaAndDoor.Where(t => t.CompanyIndex == user.CompanyIndex && t.AreaIndex == param.AreaIndex));
            //insert dữ liệu mới
            DateTime now = DateTime.Now;
            for (int i = 0; i < param.ListDoor.Count; i++)
            {
                var dataInsert = new AC_AreaAndDoor();
                dataInsert.AreaIndex = param.AreaIndex;
                dataInsert.DoorIndex = param.ListDoor[i];
                dataInsert.CompanyIndex = user.CompanyIndex;
                dataInsert.UpdatedDate = now;
                dataInsert.UpdatedUser = user.UserName;

                context.AC_AreaAndDoor.Add(dataInsert);
            }

            context.SaveChanges();
            result = Ok();
            return result;
        }

        public class DoorByArea
        {
            public int DoorIndex { get; set; }
            public string DoorName { get; set; }
            public bool InArea { get; set; }
        }

        public class GroupAreaParam
        {
            public int AreaIndex { get; set; }
            public List<int> ListDoor { get; set; }
        }

    }
}
