using System;
using System.Collections.Generic;
using EPAD_Backend_Core.Base;
using EPAD_Common;
using EPAD_Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Values")]
    [ApiController]
    public class IC_ValuesController : ApiControllerBase
    {
        private EPAD_Context context;
        private readonly ILogger _logger;
        private readonly RabbitMQHelper mRabbitMQ_Helper;
        public IC_ValuesController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            _logger = TryResolve<ILogger>();
            //mRabbitMQ_Helper = rabbitMQ_Helper;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<string> listData = new List<string>();
            listData.Add("abc");
            //mRabbitMQ_Helper.CreateMessageCommand(2, listData, "Command");
            //mRabbitMQ_Helper.CreateMessageInfo(4, "message4", "");


            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
