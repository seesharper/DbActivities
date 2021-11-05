using System;
using System.Diagnostics;
namespace DbActivities
{
    public static class ExceptionExtensions
    {
        public static Activity AddExceptionEvent(this Activity activity, Exception exception)
        {
            var exceptionTags = new ActivityTagsCollection
            {
                [OpenTelemetrySemanticNames.ExceptionType] = exception.GetType(),
                [OpenTelemetrySemanticNames.ExceptionMessage] = exception.Message,
                [OpenTelemetrySemanticNames.ExceptionStackTrace] = exception.ToString(),
                [CustomTagNames.ExceptionSource] = exception.Source
            };

            activity.AddEvent(new ActivityEvent(OpenTelemetrySemanticNames.ExceptionEventName, tags: exceptionTags));
            return activity;
        }
    }
}