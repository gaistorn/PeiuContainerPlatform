using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace PeiuPlatform.App.Controllers
{
    [Route("api/pcscontrol/v1")]
    [Authorize]
    [ApiController]
    public class PcsControlController : ControllerBase
    {
        private readonly IMqttPusher mqttPusher;
        private readonly ILogger<PcsControlController> logger;
        public PcsControlController(ILogger<PcsControlController> logger, IMqttPusher pusher)
        {
            mqttPusher = pusher;
            this.logger = logger;
        }

        [HttpGet, Route("run")]
        public async Task<IActionResult> run(int siteid, int deviceindex)
        {
            PcsControlModel model = new PcsControlModel();
            model.StopRun = true;
            string topic = $"hubbub/{siteid}/0/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }
    }
}