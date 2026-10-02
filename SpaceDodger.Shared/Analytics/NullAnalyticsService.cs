#nullable enable
using System.Collections.Generic;

namespace SpaceDodger.Analytics
{
    /// <summary>
    /// Safe no-op analytics service used when running on Desktop or when Firebase is unattached (Null Object Pattern).
    /// </summary>
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public static readonly NullAnalyticsService Instance = new NullAnalyticsService();

        private NullAnalyticsService() { }

        public bool IsEnabled => false;

        public void SetUserId(string userId) { }

        public void SetUserProperty(string name, string value) { }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object>? parameters = null) { }

        public void SetCollectionEnabled(bool enabled) { }
    }
}
