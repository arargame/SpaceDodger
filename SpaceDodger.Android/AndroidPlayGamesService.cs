using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Games;
using Android.Gms.Tasks;
using SpaceDodger.Core;

namespace SpaceDodger.Droid
{
    /// <summary>
    /// Google Play Games Services V2 implementation for Android.
    /// Handles frictionless sign-in, submitting scores to leaderboards,
    /// interactive sign-in fallback upon error 4 (SIGN_IN_REQUIRED),
    /// and showing global leaderboard UI intents.
    /// </summary>
    public class AndroidPlayGamesService : IGameServices
    {
        private readonly Activity _activity;
        private static bool _isInitialized = false;

        /// <summary>
        /// Default Leaderboard ID for Space Dodger high scores.
        /// Can be set or updated dynamically from Play Console.
        /// </summary>
        public static string LeaderboardId { get; set; } = "CgkI3Mf-_rQCEAIQAQ";

        /// <summary>
        /// Optional Leaderboard ID for highest reached level.
        /// </summary>
        public static string LevelLeaderboardId { get; set; } = string.Empty;

        public AndroidPlayGamesService(Activity activity)
        {
            _activity = activity;
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized || _activity == null) return;

            try
            {
                global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] Initializing Play Games Services V2 SDK...");
                PlayGamesSdk.Initialize(_activity);
                _isInitialized = true;
                global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Play Games Services V2 SDK initialized successfully.");

                // Attempt background authentication check / sign-in on start
                CheckAndSignInSilently();
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] PlayGamesSdk.Initialize error: {ex.Message}");
            }
        }

        private void CheckAndSignInSilently()
        {
            try
            {
                var signInClient = PlayGames.GetGamesSignInClient(_activity);
                signInClient.IsAuthenticated().AddOnCompleteListener(new OnCompleteListener(task =>
                {
                    if (task.IsSuccessful && task.Result is AuthenticationResult authResult && authResult.IsAuthenticated)
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] User is already authenticated to Play Games.");
                    }
                    else
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] User not authenticated yet. Ready for interactive sign-in on request.");
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Warn("GPGS", $"[GPGS - WARNING] CheckAndSignInSilently warning: {ex.Message}");
            }
        }

        public void SubmitHighScore(long score)
        {
            if (string.IsNullOrEmpty(LeaderboardId))
            {
                global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] SubmitHighScore skipped: LeaderboardId is not configured.");
                return;
            }

            SubmitScoreInternal(LeaderboardId, score);
        }

        public void SubmitHighestLevel(int level)
        {
            if (string.IsNullOrEmpty(LevelLeaderboardId)) return;
            SubmitScoreInternal(LevelLeaderboardId, level);
        }

        private void SubmitScoreInternal(string leaderboardId, long score)
        {
            if (!_isInitialized || _activity == null)
            {
                global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] SubmitScore failed: Service not initialized or Activity is null");
                return;
            }

            try
            {
                global::Android.Util.Log.Info("GPGS", $"[GPGS - INFO] Submitting score {score} to leaderboard {leaderboardId}...");

                PlayGames.GetLeaderboardsClient(_activity)
                    .SubmitScoreImmediate(leaderboardId, score)
                    .AddOnSuccessListener(new SuccessListener((obj) =>
                    {
                        global::Android.Util.Log.Info("GPGS", $"[GPGS - SUCCESS] Score {score} successfully submitted to {leaderboardId}");
                    }))
                    .AddOnFailureListener(new FailureListener((ex) =>
                    {
                        global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] Immediate submission failed ({ex.Message}). Caching offline with PlayGames...");
                        CacheScoreOffline(leaderboardId, score);

                        // If unauthenticated (code 4), trigger sign in
                        if (IsSignInRequiredError(ex))
                        {
                            global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] SubmitScore failed with SIGN_IN_REQUIRED. Triggering sign in...");
                            RequestSignIn(success =>
                            {
                                if (success)
                                {
                                    // Retry submission once signed in
                                    CacheScoreOffline(leaderboardId, score);
                                }
                            });
                        }
                    }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] SubmitScore exception: {ex.Message}");
                CacheScoreOffline(leaderboardId, score);
            }
        }

        private void CacheScoreOffline(string leaderboardId, long score)
        {
            try
            {
                PlayGames.GetLeaderboardsClient(_activity).SubmitScore(leaderboardId, score);
                global::Android.Util.Log.Info("GPGS", $"[GPGS - INFO] Score {score} cached offline in Play Games SDK.");
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] Failed to cache score offline: {ex.Message}");
            }
        }

        public void ShowLeaderboards()
        {
            global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] ShowLeaderboards requested.");
            if (!_isInitialized || _activity == null)
            {
                global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] ShowLeaderboards failed: GPGS not initialized");
                return;
            }

            try
            {
                var signInClient = PlayGames.GetGamesSignInClient(_activity);
                signInClient.IsAuthenticated().AddOnCompleteListener(new OnCompleteListener(task =>
                {
                    bool isAuthenticated = task.IsSuccessful &&
                                           task.Result is AuthenticationResult authResult &&
                                           authResult.IsAuthenticated;

                    if (isAuthenticated)
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] User is authenticated. Launching leaderboard intent...");
                        OpenLeaderboardIntentInternal(isRetryAfterSignIn: false);
                    }
                    else
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] User not authenticated yet. Initiating interactive sign-in...");
                        _activity.RunOnUiThread(() =>
                        {
                            try
                            {
                                Android.Widget.Toast.MakeText(_activity, "Signing in to Google Play Games...", Android.Widget.ToastLength.Short)?.Show();
                            }
                            catch { }
                        });

                        RequestSignIn(success =>
                        {
                            if (success)
                            {
                                global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Interactive sign-in succeeded. Retrying ShowLeaderboards...");
                                OpenLeaderboardIntentInternal(isRetryAfterSignIn: true);
                            }
                            else
                            {
                                global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] Sign-in cancelled or failed. (Ensure account is added to Play Console Testers and SHA-1 matches)");
                                _activity.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        Android.Widget.Toast.MakeText(_activity, "Google Play Games login failed. Please check tester account in Play Console.", Android.Widget.ToastLength.Long)?.Show();
                                    }
                                    catch { }
                                });
                            }
                        });
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] ShowLeaderboards exception: {ex.Message}");
                OpenLeaderboardIntentInternal(isRetryAfterSignIn: false);
            }
        }

        private void OpenLeaderboardIntentInternal(bool isRetryAfterSignIn)
        {
            try
            {
                var leaderboardsClient = PlayGames.GetLeaderboardsClient(_activity);
                
                // If a specific LeaderboardId is provided, try that first; else get all leaderboards
                var intentTask = !string.IsNullOrEmpty(LeaderboardId)
                    ? leaderboardsClient.GetLeaderboardIntent(LeaderboardId)
                    : leaderboardsClient.GetAllLeaderboardsIntent();

                intentTask.AddOnSuccessListener(new SuccessListener((intentObj) =>
                {
                    if (intentObj is Intent intent)
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Leaderboard intent received, launching UI...");
                        _activity.StartActivityForResult(intent, 9002);
                    }
                }))
                .AddOnFailureListener(new FailureListener((ex) =>
                {
                    global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] ShowLeaderboards failed to open intent: {ex.Message}");

                    // Error Code 4: SIGN_IN_REQUIRED
                    if (IsSignInRequiredError(ex))
                    {
                        if (!isRetryAfterSignIn)
                        {
                            global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] Error 4 (SIGN_IN_REQUIRED) detected. Requesting interactive sign in...");
                            RequestSignIn(success =>
                            {
                                if (success)
                                {
                                    global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Sign-in succeeded! Retrying ShowLeaderboards...");
                                    OpenLeaderboardIntentInternal(isRetryAfterSignIn: true);
                                }
                            });
                        }
                    }
                    else if (!string.IsNullOrEmpty(LeaderboardId))
                    {
                        // Fallback: If specific leaderboard failed (e.g. invalid ID or draft), try GetAllLeaderboardsIntent
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] Specific leaderboard intent failed. Retrying with GetAllLeaderboardsIntent...");
                        leaderboardsClient.GetAllLeaderboardsIntent()
                            .AddOnSuccessListener(new SuccessListener((allIntentObj) =>
                            {
                                if (allIntentObj is Intent allIntent)
                                {
                                    global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] All leaderboards intent received, launching UI...");
                                    _activity.StartActivityForResult(allIntent, 9002);
                                }
                            }))
                            .AddOnFailureListener(new FailureListener((allEx) =>
                            {
                                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] GetAllLeaderboardsIntent also failed: {allEx.Message}");
                            }));
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - CRITICAL] ShowLeaderboards exception: {ex.Message}");
            }
        }

        public void RequestSignIn(Action<bool> onComplete)
        {
            if (!_isInitialized || _activity == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                var signInClient = PlayGames.GetGamesSignInClient(_activity);
                signInClient.SignIn().AddOnCompleteListener(new OnCompleteListener(task =>
                {
                    if (task.IsSuccessful && task.Result is AuthenticationResult result && result.IsAuthenticated)
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Interactive SignIn completed. Authenticated = true");
                        onComplete?.Invoke(true);
                    }
                    else
                    {
                        global::Android.Util.Log.Warn("GPGS", $"[GPGS - WARNING] Interactive SignIn completed with failure or cancelled: IsSuccessful={task.IsSuccessful}");
                        onComplete?.Invoke(false);
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] RequestSignIn exception: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        private static bool IsSignInRequiredError(Exception ex)
        {
            if (ex == null) return false;
            if (ex is global::Android.Gms.Common.Apis.ApiException apiEx && apiEx.StatusCode == 4)
                return true;
            if (ex.Message != null && (ex.Message.Contains("4:") || ex.Message.Contains("SIGN_IN_REQUIRED")))
                return true;
            return false;
        }

        private sealed class SuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            private readonly Action<Java.Lang.Object> _callback;
            public SuccessListener(Action<Java.Lang.Object> callback) => _callback = callback;
            public void OnSuccess(Java.Lang.Object result) => _callback?.Invoke(result);
        }

        private sealed class FailureListener : Java.Lang.Object, IOnFailureListener
        {
            private readonly Action<Java.Lang.Exception> _callback;
            public FailureListener(Action<Java.Lang.Exception> callback) => _callback = callback;
            public void OnFailure(Java.Lang.Exception exception) => _callback?.Invoke(exception);
        }

        private sealed class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
        {
            private readonly Action<global::Android.Gms.Tasks.Task> _callback;
            public OnCompleteListener(Action<global::Android.Gms.Tasks.Task> callback) => _callback = callback;
            public void OnComplete(global::Android.Gms.Tasks.Task task) => _callback?.Invoke(task);
        }
    }
}
