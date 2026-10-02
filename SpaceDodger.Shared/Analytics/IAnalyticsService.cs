#nullable enable
using System.Collections.Generic;

namespace SpaceDodger.Analytics
{
    /// <summary>
    /// Contract for the platform-specific analytics backends (Firebase, Mock, etc.).
    /// Ensures game logic is decoupled from external telemetry SDKs (SOLID - Dependency Inversion).
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>True if analytics backend is connected and active.</summary>
        bool IsEnabled { get; }

        /// <summary>Sets the persistent player identifier.</summary>
        void SetUserId(string userId);

        /// <summary>Sets a persistent user property for player segmentation (GA4 user property, max 36 chars).</summary>
        void SetUserProperty(string name, string value);

        /// <summary>Logs a telemetry event with optional parameters.</summary>
        void LogEvent(string eventName, IReadOnlyDictionary<string, object>? parameters = null);

        /// <summary>Enables or disables data collection (GDPR / UMP consent compliance).</summary>
        void SetCollectionEnabled(bool enabled);
    }
}
