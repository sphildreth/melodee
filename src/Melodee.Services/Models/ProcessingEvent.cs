namespace Melodee.Services.Models;

public record ProcessingEvent(ProcessingEventType Type, string EventName, int Max, int Current, string Message);
