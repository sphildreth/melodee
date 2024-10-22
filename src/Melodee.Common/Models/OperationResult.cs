using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Melodee.Common.Exceptions;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models;

[Serializable]
public record OperationResult<T>
{
    private List<Exception> _errors = [];

    private List<string> _messages = [];

    public OperationResult()
    {
    }

    public OperationResult(IEnumerable<string>? messages = null)
    {
        if (messages?.Any() == true)
        {
            AdditionalData = new Dictionary<string, object>();
            messages.ToList().ForEach(AddMessage);
        }
    }

    public OperationResult(string? message = null)
    {
        AdditionalData = new Dictionary<string, object>();
        AddMessage(message);
    }

    public OperationResult(Exception? error = null)
    {
        AddError(error);
    }

    public OperationResult(OperationResponseType type, IEnumerable<string>? messages = null)
    {
        Type = type;
        if (messages?.Any() == true)
        {
            AdditionalData = new Dictionary<string, object>();
            messages.ToList().ForEach(AddMessage);
        }
    }

    public OperationResult(OperationResponseType type, string message)
    {
        Type = type;
        AddMessage(message);
    }

    public OperationResult(string? message = null, Exception? error = null)
    {
        AddMessage(message);
        AddError(error);
    }

    [XmlIgnore] public Dictionary<string, object> AdditionalClientData { get; set; } = new();

    [JsonIgnore] [XmlIgnore] public Dictionary<string, object> AdditionalData { get; set; } = new();

    /// <summary>
    ///     Client friendly exceptions
    /// </summary>
    [JsonPropertyName("errors")]
    public IEnumerable<MelodeeException> AppExceptions
    {
        get
        {
            if (Errors.Any() != true)
            {
                return [];
            }

            return Errors.Select(x => new MelodeeException(x.Message));
        }
    }

    public required T Data { get; set; }

    /// <summary>
    ///     Server side visible exceptions
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Exception> Errors { get; set; } = [];

    public bool IsSuccess => Type == OperationResponseType.Ok &&
                             !Errors.Any() &&
                             !Data.IsNullOrDefault();

    public OperationResponseType Type { get; set; } = OperationResponseType.Ok;

    public IEnumerable<string> Messages => _messages;

    public long OperationTime { get; set; }

    public void AddError(Exception? exception)
    {
        if (exception != null)
        {
            _errors.Add(exception);
        }
    }

    public void AddMessage(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            _messages.Add(message);
        }
    }
}
