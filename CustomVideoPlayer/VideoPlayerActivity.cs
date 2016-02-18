using System;
using System.Timers;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Animation;

namespace CustomVideoPlayer
{
    [Activity(
        MainLauncher = true,
        Theme = "@android:style/Theme.DeviceDefault.NoActionBar.TranslucentDecor",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize
    )]
    public class AndroidVideoPlayerActivity : Activity
    {
        private FrameLayout rootView;
        private ImmersiveVideoView videoView;
        private ImageView playButton;

        private FrameLayout progressView;
        private ProgressBar loadingIndicator;
        private TextView position;
        private SeekBar progress;
        private TextView duration;

        private Timer loadingTimeoutTimer;
        private Timer updateProgressTimer;

        private bool isLoaded = false;
        private bool draggingProgress = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.video_player);

            FindViews();

            BindEvents();

            HideProgressView();
            HidePlayButton();
        }

        private void FindViews()
        {
            rootView = FindViewById<FrameLayout>(Resource.Id.rootView);
            videoView = FindViewById<ImmersiveVideoView>(Resource.Id.videoView);
            playButton = FindViewById<ImageView>(Resource.Id.playButton);

            progressView = FindViewById<FrameLayout>(Resource.Id.progressView);
            position = FindViewById<TextView>(Resource.Id.position);
            progress = FindViewById<SeekBar>(Resource.Id.progress);
            duration = FindViewById<TextView>(Resource.Id.duration);

            loadingIndicator = FindViewById<ProgressBar>(Resource.Id.loadingIndicator);
        }

        private void BindEvents()
        {
            videoView.Error += VideoView_Error;
            videoView.Prepared += VideoView_Prepared;
            videoView.Completion += VideoView_Completion;
            videoView.Play += VideoView_Play;
            videoView.Stop += VideoView_Stop;

            rootView.Click += RootView_Click;
            playButton.Click += PlayIcon_Click;

            progress.StartTrackingTouch += Progress_StartTrackingTouch;
            progress.StopTrackingTouch += Progress_StopTrackingTouch;
            progress.ProgressChanged += Progress_JumpToPosition;
        }

        protected override void OnResume()
        {
            base.OnResume();

            LoadMovie();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (loadingTimeoutTimer != null)
            {
                loadingTimeoutTimer.Stop();
                loadingTimeoutTimer.Close();

                updateProgressTimer.Stop();
                updateProgressTimer.Close();
            }
        }

        private void LoadMovie()
        {
            loadingIndicator.Visibility = ViewStates.Visible;

            var movieUri = Android.Net.Uri.Parse("http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4");
            videoView.SetVideoURI(movieUri);

            loadingTimeoutTimer = new Timer(10000);
            loadingTimeoutTimer.AutoReset = true;
            loadingTimeoutTimer.Elapsed += CheckIfVideoIsLoaded;
            loadingTimeoutTimer.Start();

            updateProgressTimer = new Timer(100);
            updateProgressTimer.Elapsed += OnUpdateProgress;
        }

        private void CheckIfVideoIsLoaded(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
                {
                    if (!isLoaded)
                    {
                        VideoView_Error(this, null);
                    }
                });
        }

        private void VideoView_Error(object sender, Android.Media.MediaPlayer.ErrorEventArgs e)
        {
            Toast.MakeText(this, "Dieses Videoformat wird von Ihrem Gerät leider nicht unterstützt.", ToastLength.Long).Show();
            Finish();
        }

        private void VideoView_Prepared(object sender, EventArgs e)
        {
            isLoaded = true;
            loadingIndicator.Visibility = ViewStates.Invisible;

            progress.Max = videoView.Duration;
            UpdateDurationLabel(videoView.Duration);

            videoView.Start();
        }

        private void VideoView_Completion(object sender, EventArgs e)
        {
            videoView.SeekTo(0);
            UpdatePositionLabel(0);
            progress.Progress = 0;

            playButton.SetImageResource(Android.Resource.Drawable.IcMediaPlay);
            updateProgressTimer.Stop();
            ShowProgressView(animated: true);
            ShowPlayButton(animated: true);
        }

        private void RootView_Click(object sender, EventArgs e)
        {
            if (progressView.Visibility == ViewStates.Visible)
            {
                HideSystemUI();
                HideProgressView(animated: true);
                if (videoView.IsPlaying)
                    HidePlayButton(animated: true);
            }
            else
            {
                ShowSystemUI();
                ShowProgressView(animated: true);
                ShowPlayButton(animated: true);
            }
        }

        private void PlayIcon_Click(object sender, EventArgs e)
        {
            if (videoView.IsPlaying)
            {
                videoView.Pause();
            }
            else
            {
                videoView.Start();
            }
        }

        private void Progress_StartTrackingTouch (object sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            draggingProgress = true;
        }

        private void Progress_StopTrackingTouch (object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            draggingProgress = false;
        }

        private void Progress_JumpToPosition (object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (!e.FromUser)
                return;

            int newPosition = e.Progress;
            videoView.SeekTo(newPosition);
            UpdatePositionLabel(newPosition);
        }

        private void OnUpdateProgress(object sender, ElapsedEventArgs e)
        {
            if (draggingProgress || !videoView.IsPlaying || progressView.Visibility != ViewStates.Visible)
                return;

            RunOnUiThread(() =>
                {
                    int milliseconds = videoView.CurrentPosition;

                    progress.Progress = milliseconds;

                    UpdatePositionLabel(milliseconds);
                });
        }

        private void UpdatePositionLabel(int milliseconds)
        {
            var timespan = TimeSpan.FromMilliseconds(milliseconds);
            if (timespan.TotalHours >= 1)
            {
                position.Text = timespan.ToString(@"hh\:mm\:ss");
            }
            else
            {
                position.Text = timespan.ToString(@"mm\:ss");
            }
        }

        private void UpdateDurationLabel(int milliseconds)
        {
            var timespan = TimeSpan.FromMilliseconds(milliseconds);
            if (timespan.TotalHours >= 1)
            {
                duration.Text = timespan.ToString(@"hh\:mm\:ss");
            }
            else
            {
                duration.Text = timespan.ToString(@"mm\:ss");
            }
        }

        private void HideSystemUI()
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                (int)SystemUiFlags.LayoutStable
                | (int)SystemUiFlags.LayoutFullscreen
                | (int)SystemUiFlags.LayoutHideNavigation
                | (int)SystemUiFlags.Fullscreen
                | (int)SystemUiFlags.HideNavigation
                | (int)SystemUiFlags.Immersive
            );
        }

        private void ShowSystemUI()
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                (int)SystemUiFlags.LayoutStable
                | (int)SystemUiFlags.LayoutFullscreen
                | (int)SystemUiFlags.LayoutHideNavigation
            );
        }

        private void ShowPlayButton(bool animated = false)
        {
            SetPlayButtonVisbility(true, animated);
        }

        private void HidePlayButton(bool animated = false)
        {
            SetPlayButtonVisbility(false, animated);
        }

        private void SetPlayButtonVisbility(bool isVisible, bool animated)
        {
            float targetAlpha = isVisible ? 1.0f : 0.0f;

            if (animated)
            {
                playButton.Visibility = ViewStates.Visible;
                var anim = ValueAnimator.OfFloat(playButton.Alpha, targetAlpha);
                anim.SetDuration(300);
                anim.Update += (sender, e) => playButton.Alpha = (float)e.Animation.AnimatedValue;
                anim.AnimationEnd += (sender, e) =>
                {
                    playButton.Alpha = targetAlpha;
                    playButton.Visibility = isVisible ? ViewStates.Visible : ViewStates.Invisible;
                };
                anim.Start();
            }
            else
            {
                playButton.Alpha = targetAlpha;
                playButton.Visibility = isVisible ? ViewStates.Visible : ViewStates.Invisible;
            }
        }

        private void ShowProgressView(bool animated = false)
        {
            SetProgressViewVisibility(true, animated);
            ShowSystemUI();
        }

        private void HideProgressView(bool animated = false)
        {
            SetProgressViewVisibility(false, animated);
            HideSystemUI();
        }

        private void SetProgressViewVisibility(bool isVisible, bool animated)
        {
            float targetAlpha = isVisible ? 1.0f : 0.0f;

            if (animated)
            {
                progressView.Visibility = ViewStates.Visible;
                var anim = ValueAnimator.OfFloat(progressView.Alpha, targetAlpha);
                anim.SetDuration(300);
                anim.Update += (sender, e) => progressView.Alpha = (float)e.Animation.AnimatedValue;
                anim.AnimationEnd += (sender, e) =>
                {
                    progressView.Alpha = targetAlpha;
                    progressView.Visibility = isVisible ? ViewStates.Visible : ViewStates.Invisible;
                };
                anim.Start();
            }
            else
            {
                progressView.Alpha = targetAlpha;
                progressView.Visibility = isVisible ? ViewStates.Visible : ViewStates.Invisible;
            }
        }

        private void VideoView_Play(object sender, EventArgs e)
        {
            playButton.SetImageResource(Android.Resource.Drawable.IcMediaPause);
            updateProgressTimer.Start();

            // TODO: Timeout
            HideProgressView(animated: true);
            HidePlayButton(animated: true);
        }

        private void VideoView_Stop(object sender, EventArgs e)
        {
            playButton.SetImageResource(Android.Resource.Drawable.IcMediaPlay);
            updateProgressTimer.Start();


            ShowProgressView(animated: true);
            ShowPlayButton(animated: true);
        }
    }
}




