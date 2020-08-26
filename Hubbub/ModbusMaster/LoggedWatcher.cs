using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hubbub
{
    public class LoggedWatcher : Watcher
    {
        private readonly ILogger logger;
        public LoggedWatcher(ILogger logger)
        {
            this.logger = logger;
        }


        public override Task process(WatchedEvent @event)
        {
            
            switch (@event.getState())
            {
                case Event.KeeperState.AuthFailed:
                case Event.KeeperState.Disconnected:
                case Event.KeeperState.Expired:
                    logger.LogWarning($"[{DateTimeOffset.Now}] event type: {@event.get_Type()}, Path={@event.getPath()} State={@event.getState()}");
                    break;
                default:
                    logger.LogInformation($"[{DateTimeOffset.Now}] event type: {@event.get_Type()}, Path={@event.getPath()} State={@event.getState()}");
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
