namespace Common.Attributes.Isekai;

public enum IsekaiMethodTimeout
{
    /// <summary>
    /// Use if you expect considerable variation in the response time, but the data you're providing is likely to
    /// be critical to the calling service. Maybe you should consider improving the performance of the endpoint or
    /// using different workflows such as queuing a background task.
    /// </summary>
    High = 10000,

    /// <summary>
    /// Use a Medium timeout if you expect the majority of calls to your service to complete well within 1 second.
    /// You would expect calls to this endpoint to be happy waiting for up to 1 second because the data or
    /// functionality it provides is part of a more critical workflow.
    /// </summary>
    Medium = 1000,

    /// <summary>
    /// Use this if you expect the service to respond 99.99% of the time within 100ms. Or maybe you expect your
    /// service method to be called very frequently and would prefer slow clients to fail fast. You may also set
    /// your timeout to Low in cases where you expect clients to handle failure with little or no impact.
    /// </summary>
    Low = 100,

    /// <summary>
    /// This will result in `CancellationToken.None` You should use it ONLY if you know what you are doing. Really!
    /// </summary>
    None = int.MaxValue
}
