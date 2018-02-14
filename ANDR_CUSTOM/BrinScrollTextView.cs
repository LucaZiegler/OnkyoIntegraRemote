using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace AppOnkyo.ANDR_CUSTOM
{
    [Register("AppOnkyo.ANDR_CUSTOM.BrinScrollTextView")]
    public class BrinScrollTextView : TextView
    {
        public BrinScrollTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            SetScrolling();
        }

        public BrinScrollTextView(Context context) : base(context)
        {
            SetScrolling();
        }

        public BrinScrollTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetScrolling();
        }

        public BrinScrollTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            SetScrolling();
        }

        public BrinScrollTextView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            SetScrolling();
        }

        private void SetScrolling()
        {
            HorizontalFadingEdgeEnabled = true;
            SetSingleLine();
            Ellipsize = TextUtils.TruncateAt.Marquee;
            Selected = true;
            FreezesText = true;
        }

        public override void SetText(ICharSequence text, BufferType type)
        {
            base.SetText(text, type);
            Selected = true;
        }

        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            if (gainFocus)
                base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
        }

        public override void OnWindowFocusChanged(bool hasWindowFocus)
        {
            if (hasWindowFocus)
                base.OnWindowFocusChanged(hasWindowFocus);
        }

        public override bool IsFocused
        {
            get => true;
        }
    }
}