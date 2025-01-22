using Melodee.Common.Models.OpenSubsonic.Requests;

namespace Melodee.Common.MessageBus.Events;

public record UserStreamEvent(ApiRequest ApiRequest, StreamRequest Request);
