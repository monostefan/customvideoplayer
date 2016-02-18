using System;
using Android.Widget;
using Android.Content;
using Android.Runtime;
using Android.Util;

namespace CustomVideoPlayer
{
    public class ImmersiveVideoView : VideoView
    {
        public ImmersiveVideoView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {}

        public ImmersiveVideoView(Context context, IAttributeSet attrs) : base(context, attrs) {}

        public ImmersiveVideoView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {}

        public ImmersiveVideoView(Context context) : base(context) {}

        public event EventHandler Play;
        public event EventHandler Stop;

        public override void Start()
        {
            base.Start();
            Play.Invoke(this, null);
        }

        public override void Pause()
        {
            base.Pause();
            Stop?.Invoke(this, null);
        }
    }
}

