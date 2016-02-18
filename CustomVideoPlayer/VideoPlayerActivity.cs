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
        ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize
    )]
    public class AndroidVideoPlayerActivity : Activity
    {
        private FrameLayout rootView;
        private ImageView playIcon;

        private FrameLayout controlls;
        private ProgressBar loadingIndicator;
        private ImmersiveVideoView videoView;
        private LinearLayout progressView;
        private TextView position;
        private ProgressBar progress;
        private TextView duration;

        private Timer timer;
        private bool isLoaded = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.video_player);

            FindViews();

            BindEvents();

            HideControlls();
        }

        private void FindViews()
        {
            rootView = FindViewById<FrameLayout>(Resource.Id.rootView);
            videoView = FindViewById<ImmersiveVideoView>(Resource.Id.videoView);

            controlls = FindViewById<FrameLayout>(Resource.Id.controlls);
            loadingIndicator = FindViewById<ProgressBar>(Resource.Id.loadingIndicator);
            playIcon = FindViewById<ImageView>(Resource.Id.playIcon);
            progressView = FindViewById<LinearLayout>(Resource.Id.progressView);
            position = FindViewById<TextView>(Resource.Id.position);
            progress = FindViewById<ProgressBar>(Resource.Id.progress);
            duration = FindViewById<TextView>(Resource.Id.duration);
        }

        private void BindEvents()
        {
            videoView.Error += VideoView_Error;
            videoView.Prepared += VideoView_Prepared;
            videoView.Completion += VideoView_Completion;
            videoView.Play += VideoView_Play;
            videoView.Stop += VideoView_Stop;

            rootView.Click += RootView_Click;
            playIcon.Click += PlayIcon_Click;
        }

        protected override void OnResume()
        {
            base.OnResume();

            LoadMovie();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (timer != null)
            {
                timer.Stop();
                timer.Close();
            }
        }

        private void LoadMovie()
        {
            loadingIndicator.Visibility = ViewStates.Visible;

            var movieUri = Android.Net.Uri.Parse("http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4");
            videoView.SetVideoURI(movieUri);

            timer = new Timer();
            timer.Elapsed += CheckIfVideoIsLoaded;
            timer.Interval = 10000;
            timer.Start();
        }

        private void CheckIfVideoIsLoaded (object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() => 
                {
                    if (!isLoaded)
                    {
                        VideoView_Error(this, null);
                    }
                });
        }

        private void RootView_Click (object sender, EventArgs e)
        {
            if (controlls.Visibility == ViewStates.Visible)
            {
                HideSystemUI();
                HideControlls(animated: true);
            }
            else
            {
                ShowSystemUI();
                ShowControlls(animated: true);
            }
        }

        private void PlayIcon_Click (object sender, EventArgs e)
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

        private void VideoView_Error(object sender, Android.Media.MediaPlayer.ErrorEventArgs e)
        {
            Toast.MakeText(this, "Dieses Videoformat wird von Ihrem Gerät leider nicht unterstützt.", ToastLength.Long).Show();
            Finish();
        }

        private void VideoView_Prepared(object sender, EventArgs e)
        {
            isLoaded = true;
            loadingIndicator.Visibility = ViewStates.Invisible;
            playIcon.Visibility = ViewStates.Visible;
            UpdateDuration(videoView.Duration);
            videoView.Start();
        }

        private void UpdateDuration(int milliseconds)
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

        private void VideoView_Completion(object sender, EventArgs e)
        {
            videoView.SeekTo(0);
            ShowControlls(animated: true);
            playIcon.Visibility = ViewStates.Visible;
        }

        private void HideSystemUI()
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility) (
                (int) SystemUiFlags.LayoutStable
                | (int) SystemUiFlags.LayoutFullscreen
                | (int) SystemUiFlags.LayoutHideNavigation
                | (int) SystemUiFlags.Fullscreen
                | (int) SystemUiFlags.HideNavigation
                | (int) SystemUiFlags.Immersive
            );
        }

        private void ShowSystemUI()
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility) (
                (int) SystemUiFlags.LayoutStable
                | (int) SystemUiFlags.LayoutFullscreen
                | (int) SystemUiFlags.LayoutHideNavigation
            );
        }

        private void ShowControlls(bool animated = false)
        {
            SetControllsVisibility(true, animated);
            ShowSystemUI();
        }

        private void HideControlls(bool animated = false)
        {
            SetControllsVisibility(false, animated);
            HideSystemUI();
        }

        private void SetControllsVisibility(bool isVisible, bool animated = false)
        {
            float targetAlpha = isVisible ? 1.0f : 0.0f;

            if (animated)
            {
                controlls.Visibility = ViewStates.Visible;
                var hide = ValueAnimator.OfFloat(controlls.Alpha, targetAlpha);
                hide.SetDuration(300);
                hide.Update += (sender, e) => controlls.Alpha = (float)e.Animation.AnimatedValue;
                hide.AnimationEnd += (sender, e) =>
                    controlls.Visibility = isVisible ? ViewStates.Visible : ViewStates.Invisible;
                hide.Start();
            }
            else
            {
                controlls.Alpha = targetAlpha;
                controlls.Visibility = ViewStates.Visible;
            }
        }

        private void VideoView_Play (object sender, EventArgs e)
        {
            playIcon.SetImageResource(Android.Resource.Drawable.IcMediaPause);
            HideControlls(animated: true);
        }

        private void VideoView_Stop (object sender, EventArgs e)
        {
            playIcon.SetImageResource(Android.Resource.Drawable.IcMediaPlay);
        }
    }
}




