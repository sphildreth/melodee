using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.MessageBus.Events;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Serilog;

namespace Melodee.Common.MessageBus.EventHandlers;

public sealed class UserStreamEventHandler(
    ILogger logger)  : IHandleMessages<UserStreamEvent>
{
    public Task Handle(UserStreamEvent message)
    {
        if (message.Request.IsDownloadingRequest)
        {
            logger.Information("User [{User}] downloaded song [{SongId}]", message.ApiRequest.Username, message.Request.Id);
        }
        logger.Debug("User [{User}] stream song [{SongId}]", message.ApiRequest.Username, message.Request.Id);
        
        return Task.CompletedTask;
    }
}
