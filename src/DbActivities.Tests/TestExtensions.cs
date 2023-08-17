using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;

namespace DbActivities
{
    public static class StartedActivitiesExtensions
    {
        public static Activity GetActivity(this IEnumerable<Activity> activities, string name)
        {
            var activity = activities.SingleOrDefault(activity => activity.OperationName == name);
            activity.Should().NotBeNull();
            // activity.Duration.Should().NotBe(TimeSpan.Zero);
            return activity;
        }

        public static void ShouldHaveExceptionEvent(this Activity activity)
        {
            var exceptionEvent = activity.Events.SingleOrDefault();
            exceptionEvent.Name.Should().Be(OpenTelemetrySemanticNames.ExceptionEventName);
            exceptionEvent.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.ExceptionMessage);
            exceptionEvent.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.ExceptionStackTrace);
            exceptionEvent.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.ExceptionType);
            exceptionEvent.Tags.Should().Contain(tag => tag.Key == CustomTagNames.ExceptionSource);
        }

        public static void ShouldHaveCallLevelTags(this Activity activity, string operation)
        {
            activity.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.DbStatement && tag.Value.Length > 0);
            activity.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.DbName && tag.Value.Length > 0);
            activity.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.DbOperation && tag.Value == operation);
        }

        public static void ShouldHaveConnectionLevelTags(this Activity activity)
        {
            activity.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.DbSystem && tag.Value.Length > 0);
            activity.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.DbConnectionString && tag.Value.Length > 0);
            activity.Tags.Should().Contain(tag => tag.Key == OpenTelemetrySemanticNames.DbUser);
        }
    }
}