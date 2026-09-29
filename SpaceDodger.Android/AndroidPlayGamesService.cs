using System;
using Android.App;
using Android.Content;
using Android.Gms.Games;
using Android.Gms.Tasks;
using Android.Widget;
using SpaceDodger.Core;

namespace SpaceDodger.Droid
{
    /// <summary>
    /// Google Play Games Services V2 implementation for Android.
    /// Handles frictionless sign-in once at launch, non-blocking background score submission,
    /// and showing global leaderboard UI on UI thread when requested by the user.
    /// </summary>
    public class AndroidPlayGamesService : IGameServices
    {
        private readonly Activity _activity;
        private static bool _isInitialized = false;
        private static bool _hasPromptedSignInAtLaunch = false;

        /// <summary>
        /// Default Leaderboard ID for Space Dodger high scores.
        /// Configured from Google Play Console (Published).
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

                // Check sign-in on launch, prompting the user at most once on startup.
                _activity.RunOnUiThread(() =>
                {
                    CheckAndPromptSignInAtLaunch();
                });
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] PlayGamesSdk.Initialize error: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts silent check on startup, or prompts interactive sign-in only ONCE.
        /// Never prompts automatically again during the rest of the game session.
        /// </summary>
        private void CheckAndPromptSignInAtLaunch()
        {
            if (_hasPromptedSignInAtLaunch) return;
            _hasPromptedSignInAtLaunch = true;

            try
            {
                var signInClient = PlayGames.GetGamesSignInClient(_activity);
                signInClient.IsAuthenticated().AddOnCompleteListener(new OnCompleteListener(task =>
                {
                    if (task.IsSuccessful && task.Result is AuthenticationResult authResult && authResult.IsAuthenticated)
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] User is already authenticated to Play Games on launch.");
                    }
                    else
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] User not authenticated on launch. Requesting sign-in once...");
                        signInClient.SignIn().AddOnCompleteListener(new OnCompleteListener(signInTask =>
                        {
                            if (signInTask.IsSuccessful && signInTask.Result is AuthenticationResult res && res.IsAuthenticated)
                            {
                                global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Startup sign-in succeeded.");
                            }
                            else
                            {
                                global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] Startup sign-in was dismissed or skipped by user.");
                            }
                        }));
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Warn("GPGS", $"[GPGS - WARNING] CheckAndPromptSignInAtLaunch error: {ex.Message}");
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

        /// <summary>
        /// Non-blocking, completely silent score submission.
        /// NEVER pops up a login dialog or pauses the game.
        /// The Play Games SDK caches the score locally and syncs automatically when online.
        /// </summary>
        private void SubmitScoreInternal(string leaderboardId, long score)
        {
            if (!_isInitialized || _activity == null)
                return;

            try
            {
                _activity.RunOnUiThread(() =>
                {
                    try
                    {
                        PlayGames.GetLeaderboardsClient(_activity).SubmitScore(leaderboardId, score);
                        global::Android.Util.Log.Info("GPGS", $"[GPGS - INFO] Score {score} submitted to {leaderboardId} in background.");
                    }
                    catch (Exception ex)
                    {
                        global::Android.Util.Log.Warn("GPGS", $"[GPGS - WARNING] SubmitScore background warning: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Warn("GPGS", $"[GPGS - WARNING] SubmitScore exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the player clicks the "World Ranking" button.
        /// Dispatches to the UI thread to ensure Android Task listeners are triggered reliably.
        /// </summary>
        public void ShowLeaderboards()
        {
            global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] ShowLeaderboards requested.");
            if (!_isInitialized || _activity == null)
            {
                global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] ShowLeaderboards failed: GPGS not initialized");
                return;
            }

            _activity.RunOnUiThread(() =>
            {
                ShowLeaderboardsOnUiThread();
            });
        }

        private void ShowLeaderboardsOnUiThread()
        {
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
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] User authenticated. Opening leaderboards intent...");
                        LaunchLeaderboardIntent();
                    }
                    else
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] User not authenticated. Requesting interactive sign-in before showing leaderboard...");
                        signInClient.SignIn().AddOnCompleteListener(new OnCompleteListener(signInTask =>
                        {
                            if (signInTask.IsSuccessful && signInTask.Result is AuthenticationResult res && res.IsAuthenticated)
                            {
                                global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Interactive sign-in succeeded! Opening leaderboard...");
                                LaunchLeaderboardIntent();
                            }
                            else
                            {
                                global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] Sign-in cancelled or failed.");
                                Toast.MakeText(_activity, "Google Play Oyunlar girişi yapılamadı.", ToastLength.Short)?.Show();
                            }
                        }));
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] ShowLeaderboardsOnUiThread exception: {ex.Message}");
                LaunchLeaderboardIntent();
            }
        }

        private void LaunchLeaderboardIntent()
        {
            try
            {
                var leaderboardsClient = PlayGames.GetLeaderboardsClient(_activity);
                
                var intentTask = !string.IsNullOrEmpty(LeaderboardId)
                    ? leaderboardsClient.GetLeaderboardIntent(LeaderboardId)
                    : leaderboardsClient.GetAllLeaderboardsIntent();

                intentTask.AddOnSuccessListener(new SuccessListener((intentObj) =>
                {
                    if (intentObj is Intent intent)
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] Leaderboard intent received! Opening UI...");
                        _activity.StartActivityForResult(intent, 9002);
                    }
                    else
                    {
                        global::Android.Util.Log.Warn("GPGS", "[GPGS - WARNING] Leaderboard intent object is not an Intent.");
                    }
                }))
                .AddOnFailureListener(new FailureListener((ex) =>
                {
                    global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] Failed to get leaderboard intent: {ex.Message}");

                    // Fallback to all leaderboards if specific ID failed
                    if (!string.IsNullOrEmpty(LeaderboardId))
                    {
                        global::Android.Util.Log.Info("GPGS", "[GPGS - INFO] Retrying with GetAllLeaderboardsIntent fallback...");
                        leaderboardsClient.GetAllLeaderboardsIntent()
                            .AddOnSuccessListener(new SuccessListener((allIntentObj) =>
                            {
                                if (allIntentObj is Intent allIntent)
                                {
                                    global::Android.Util.Log.Info("GPGS", "[GPGS - SUCCESS] All-leaderboards intent received! Opening UI...");
                                    _activity.StartActivityForResult(allIntent, 9002);
                                }
                            }))
                            .AddOnFailureListener(new FailureListener((allEx) =>
                            {
                                global::Android.Util.Log.Error("GPGS", $"[GPGS - ERROR] GetAllLeaderboardsIntent also failed: {allEx.Message}");
                                Toast.MakeText(_activity, "Liderlik tablosu açılamadı: " + allEx.Message, ToastLength.Long)?.Show();
                            }));
                    }
                    else
                    {
                        Toast.MakeText(_activity, "Liderlik tablosu açılamadı.", ToastLength.Short)?.Show();
                    }
                }));
            }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("GPGS", $"[GPGS - CRITICAL] LaunchLeaderboardIntent exception: {ex.Message}");
            }
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
