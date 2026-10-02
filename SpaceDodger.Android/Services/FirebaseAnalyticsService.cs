#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using SpaceDodger.Analytics;

namespace SpaceDodger.Droid.Services
{
    /// <summary>
    /// Android Firebase Analytics implementation of <see cref="IAnalyticsService"/>.
    /// Bridges high-level game telemetry to the Google Firebase Analytics SDK without
    /// impacting main game thread performance.
    ///
    /// Performance &amp; Threading:
    ///   - Initialized in the background via Task.Run (0 ms impact on cold start).
    ///   - Safely attached to AnalyticsManager.Service once instantiated.
    ///   - All calls wrapped in defensive try/catch; telemetry failure will never crash the game.
    /// </summary>
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        private readonly global::Firebase.Analytics.FirebaseAnalytics _firebase;

        private FirebaseAnalyticsService(global::Firebase.Analytics.FirebaseAnalytics firebase)
        {
            _firebase = firebase;
        }

        public bool IsEnabled => true;

        /// <summary>
        /// Initializes the Firebase SDK on a background thread and hooks it into AnalyticsManager.
        /// </summary>
        public static void Attach(Context context)
        {
            Task.Run(() =>
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    var firebase = global::Firebase.Analytics.FirebaseAnalytics.GetInstance(context);
                    if (firebase == null)
                    {
                        global::Android.Util.Log.Warn("SpaceDodger", "[Analytics] FirebaseAnalytics.GetInstance returned null; analytics disabled.");
                        return;
                    }

                    firebase.SetAnalyticsCollectionEnabled(true);
                    var service = new FirebaseAnalyticsService(firebase);

                    sw.Stop();
                    global::Android.Util.Log.Info("SpaceDodger",
                        $"[Analytics] Firebase Analytics ready in {sw.ElapsedMilliseconds}ms (background thread). Collection enabled.");

                    AnalyticsManager.Service = service;
                }
                catch (Exception ex)
                {
                    global::Android.Util.Log.Warn("SpaceDodger", $"[Analytics] Failed to initialize Firebase Analytics: {ex.Message}");
                }
            });
        }

        public void SetUserId(string userId)
        {
            try { _firebase.SetUserId(userId); }
            catch (Exception ex) { LogWarn(ex); }
        }

        public void SetUserProperty(string name, string value)
        {
            try
            {
                if (value != null && value.Length > 36)
                    value = value.Substring(0, 36);

                _firebase.SetUserProperty(name, value);
            }
            catch (Exception ex) { LogWarn(ex); }
        }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object>? parameters = null)
        {
            try
            {
                var bundle = new Bundle();
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var kv in parameters)
                    {
                        switch (kv.Value)
                        {
                            case int i: bundle.PutLong(kv.Key, i); break;
                            case long l: bundle.PutLong(kv.Key, l); break;
                            case bool b: bundle.PutLong(kv.Key, b ? 1 : 0); break;
                            case float f: bundle.PutDouble(kv.Key, f); break;
                            case double d: bundle.PutDouble(kv.Key, d); break;
                            case string s:
                                bundle.PutString(kv.Key, s.Length > 100 ? s.Substring(0, 100) : s);
                                break;
                            default:
                                bundle.PutString(kv.Key, kv.Value?.ToString() ?? string.Empty);
                                break;
                        }
                    }
                }

                _firebase.LogEvent(eventName, bundle);
            }
            catch (Exception ex) { LogWarn(ex); }
        }

        public void SetCollectionEnabled(bool enabled)
        {
            try { _firebase.SetAnalyticsCollectionEnabled(enabled); }
            catch (Exception ex) { LogWarn(ex); }
        }

        private static void LogWarn(Exception ex)
            => global::Android.Util.Log.Warn("SpaceDodger", $"[Analytics] Error: {ex.Message}");
    }
}
