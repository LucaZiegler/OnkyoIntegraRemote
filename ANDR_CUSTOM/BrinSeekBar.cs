using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AppOnkyo.ANDR_CUSTOM
{
    public class BrinSeekBar : SeekBar
    {
        public bool isSeekActive;

        public delegate void ProgressChangedListener(object sender, ProgressChangedEventArgs pcea);

        public ProgressChangedListener OnProgressChanged;
        private int lastVal = 0;

        public BrinSeekBar(Context context) : base(context)
        {
            Init(context);
        }

        public BrinSeekBar(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public BrinSeekBar(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        public BrinSeekBar(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context,
            attrs, defStyleAttr, defStyleRes)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            StartTrackingTouch += delegate { isSeekActive = true; };
            StopTrackingTouch += delegate { isSeekActive = false; };
            ProgressChanged += delegate(object sender, ProgressChangedEventArgs pcea)
            {
                int dif = pcea.Progress - lastVal;
                if (pcea.FromUser && dif > (Max / 5))
                {
                    Progress = lastVal + 1;
                    lastVal = lastVal + 1;
                }
                else
                {
                    lastVal = pcea.Progress;
                }
                OnProgressChanged?.Invoke(sender, new ProgressChangedEventArgs(this, Progress, pcea.FromUser));
            };
        }
    }
}