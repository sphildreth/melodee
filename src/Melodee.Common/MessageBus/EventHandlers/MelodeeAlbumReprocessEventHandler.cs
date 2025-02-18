using System.Diagnostics;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Services.Scanning;
using Rebus.Handlers;
using Serilog;

namespace Melodee.Common.MessageBus.EventHandlers;

public sealed class MelodeeAlbumReprocessEventHandler(
    ILogger logger,
    DirectoryProcessorToStagingService directoryProcessorToStagingService) : IHandleMessages<MelodeeAlbumReprocessEvent>
{
    public async Task Handle(MelodeeAlbumReprocessEvent message)
    {
        var directoryToReProcess = new DirectoryInfo(message.Path);
        if (!directoryToReProcess.Exists)
        {
            logger.Warning("[{HandlerName}]: invalid metadata album directory [{DirName}]", nameof(MelodeeAlbumReprocessEventHandler), directoryToReProcess.FullName);
            return;
        }
        logger.Debug("[{HandlerName}]: Reprocessing metadata album directory [{DirName}]", nameof(MelodeeAlbumReprocessEventHandler), directoryToReProcess.FullName);
        await directoryProcessorToStagingService.InitializeAsync();
        var result = await directoryProcessorToStagingService.ProcessDirectoryAsync(directoryToReProcess.ToDirectorySystemInfo(), null, null);
        if (!result.IsSuccess)
        {
            logger.Warning("[{HandlerName}]: unable to process metadata album directory [{DirName}] Result [{Result}]", 
                nameof(MelodeeAlbumReprocessEventHandler), 
                directoryToReProcess.FullName,
                result.ToString());
            return;
        }
    }
}
