using System;
using Android.Widget;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Android.Animation;

namespace CustomVideoPlayer
{
    public class SystemFrameLayout : FrameLayout
    {
        public SystemFrameLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {}
        

        public SystemFrameLayout(Context context) : base(context) {}
        

        public SystemFrameLayout(Context context, IAttributeSet attrs) : base(context, attrs) {}
        

        public SystemFrameLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) {}


        protected override bool FitSystemWindows(Android.Graphics.Rect insets)
        {
            if (FitsSystemWindows)
            {
                base.FitSystemWindows(insets);
            }
            else
            {
                SetPaddingBottomAnimated(0);
            }

            return true;
        }

        private void SetPaddingBottomAnimated(int value)
        {
            var a = ValueAnimator.OfInt(PaddingBottom, value);
            a.SetDuration(300);
            a.Update += (sender, e) => SetPadding(0, 0, 0, (int)e.Animation.AnimatedValue);
            a.StartDelay = 200;
            a.Start();
        }
    }
}

