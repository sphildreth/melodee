using Quartz;
using Quartz.Impl;

namespace Melodee.Common.Jobs;

public class MelodeeJobExecutionContext(CancellationToken cancellation) : IJobExecutionContext
{
    public const string ForceMode = "ForceMode";
    public const string ScanJustDirectory = "ScanJustDirectory";
    public const string Verbose = "Verbose";

    private readonly Dictionary<object, object> _dataMap = new();

    public void Put(object key, object objectValue)
    {
        if (!_dataMap.TryAdd(key, objectValue))
        {
            _dataMap[key] = objectValue;
        }
    }

    public object? Get(object key)
    {
        _dataMap.TryGetValue(key, out var value);
        return value;
    }

    public IScheduler Scheduler { get; } = null!;
    public ITrigger Trigger { get; } = null!;
    public ICalendar? Calendar { get; } = null!;
    public bool Recovering { get; } = false;
    public TriggerKey RecoveringTriggerKey { get; } = null!;
    public int RefireCount { get; } = 0;
    public JobDataMap MergedJobDataMap { get; } = null!;
    public IJobDetail JobDetail { get; } = new JobDetailImpl();
    public IJob JobInstance { get; } = null!;
    public DateTimeOffset FireTimeUtc { get; } = DateTimeOffset.MinValue;
    public DateTimeOffset? ScheduledFireTimeUtc { get; } = DateTimeOffset.MinValue;
    public DateTimeOffset? PreviousFireTimeUtc { get; } = DateTimeOffset.MinValue;
    public DateTimeOffset? NextFireTimeUtc { get; } = DateTimeOffset.MinValue;
    public string FireInstanceId { get; } = null!;
    public object? Result { get; set; }
    public TimeSpan JobRunTime { get; } = TimeSpan.Zero;
    public CancellationToken CancellationToken { get; } = cancellation;
}
