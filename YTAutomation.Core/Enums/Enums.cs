namespace YTAutomation.Core.Enums;

public enum VideoJobStatus
{
    Pending = 0,
    GeneratingScript = 1,
    GeneratingVoiceover = 2,
    GeneratingVideo = 3,
    GeneratingThumbnail = 4,
    OptimizingSEO = 5,
    ReadyToPublish = 6,
    Publishing = 7,
    Published = 8,
    Failed = 9,
    Cancelled = 10
}

public enum ScheduledPostStatus
{
    Scheduled = 0,
    InProgress = 1,
    Published = 2,
    Failed = 3,
    Cancelled = 4
}
