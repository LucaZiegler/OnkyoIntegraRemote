using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Opengl;
using Android.Renderscripts;
using Android.Support.Design.Internal;
using Android.Support.Design.Widget;
using Android.Support.V4.Content.Res;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AppOnkyo.ISCP;
using AppOnkyo.OBJECTS;
using AppOnkyo.SERVICE;
using Java.Lang;
using Math = Java.Lang.Math;
using Orientation = Android.Widget.Orientation;

namespace AppOnkyo.HELPER
{
    public class ViewHelper
    {
        public delegate void ServiceMsgListener(string msg);

        public static ServiceMsgListener ShowSetupOptionDialog(ServiceMsgListener cb, Activity act,
            SetupOption setupOption)
        {
            bool answered = false;

            var dia = new Dialog(act);
            dia.SetContentView(Resource.Layout.dialog_setup_option);

            var rgMain = dia.FindViewById<RadioGroup>(Resource.Id.rgMain);
            var tvTitle = dia.FindViewById<TextView>(Resource.Id.tvTitle);

            tvTitle.Text = setupOption.Title;
            rgMain.Orientation = Orientation.Vertical;

            cb = delegate(string msg)
            {
                var res = ISCPHelper.Parse(msg);
                if (res[0] == setupOption.Cmd)
                {
                    answered = true;
                    if (res[1] == "N/A")
                    {
                        Toast.MakeText(Application.Context, "Receiver has answered with N/A", ToastLength.Short).Show();
                        return;
                    }
                    int c = 0;
                    foreach (var entry in setupOption.LiEntries)
                    {
                        if (res[1] == entry.Cmd)
                        {
                            ((RadioButton)rgMain.GetChildAt(c)).Checked = true;
                            break;
                        }
                        c++;
                    }
                }
            };

            int i = 0;
            foreach (var entry in setupOption.LiEntries)
            {
                var rgItem = (RadioButton) LayoutInflater.From(act)
                    .Inflate(Resource.Layout.item_setup_entry, rgMain, false);
                rgItem.Text = entry.Title;
                rgItem.Tag = i;
                rgItem.Click += delegate
                {
                    int indSel = Convert.ToInt32(rgItem.Tag);
                    var ent = setupOption.LiEntries[indSel];
                    DeviceService.SendCommand($"{setupOption.Cmd}{ent.Cmd}");
                };

                rgMain.AddView(rgItem);
                i++;
            }

            dia.Show();

            rgMain.PostDelayed(() =>
            {
                if (!answered)
                {
                    Toast.MakeText(act, $"Your model may not support this feature", ToastLength.Long).Show();
                }
            }, 500);

            if (DeviceService.CmdPowerStatus?.PowerState == false)
            {
                    Toast.MakeText(act, $"Your receiver is turned off and may not responding", ToastLength.Long).Show();
                }
            return cb;
        }

        public static void InflateSetupOptions(List<SetupOption> liOptions, NavigationView nv)
        {
            int i = 0;
            foreach (var option in liOptions)
            {
                var it = nv.Menu.Add(0, i, i, option.Title);
                it.SetIcon(option.Icon);
                i++;
            }
        }

        public class BrinBottomSheetCallBack : BottomSheetBehavior.BottomSheetCallback
        {
            public delegate void SlideListener(View bs, float slideOffset);

            public delegate void StateChangedListener(View bottomSheet, int newState);

            public SlideListener OnSlideChanged;
            public StateChangedListener OnStateStatusChanged;


            public override void OnSlide(View bottomSheet, float slideOffset)
            {
                OnSlideChanged?.Invoke(bottomSheet, slideOffset);
            }

            public override void OnStateChanged(View bottomSheet, int newState)
            {
                OnStateStatusChanged?.Invoke(bottomSheet, newState);
            }
        }

        public static ViewStates VisibilityConverter(bool visible)
        {
            return visible ? ViewStates.Visible : ViewStates.Gone;
        }

        public static bool VisibilityConverter(ViewStates visible)
        {
            return visible == ViewStates.Visible;
        }

        public static void FadeVisibility(bool show, View view)
        {
            if ((show && view.Visibility == ViewStates.Visible) || !show && view.Visibility != ViewStates.Visible)
                return;
            view.ClearAnimation();
            if (show)
            {
                var anim = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_in);
                view.Visibility = ViewStates.Visible;
                view.StartAnimation(anim);
            }
            else
            {
                var anim = AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.abc_fade_out);
                view.StartAnimation(anim);
                anim.AnimationEnd += delegate(object sender, Animation.AnimationEndEventArgs args)
                {
                    view.Visibility = ViewStates.Gone;
                };
            }
        }

        public static void AnimateVisibility(bool show, View view)
        {
            if ((show && view.Visibility == ViewStates.Visible) || !show && view.Visibility != ViewStates.Visible)
                return;

            int cx = view.Width / 2;
            int cy = view.Height / 2;
            float radius = (float) Math.Hypot(cx, cy);
            if (show)
            {
                Animator anim = ViewAnimationUtils.CreateCircularReveal(view, cx, cy, 0, radius);
                view.Visibility = ViewStates.Visible;
                anim.Start();
            }
            else
            {
                try
                {
                    Animator anim =
                        ViewAnimationUtils.CreateCircularReveal(view, cx, cy, radius, 0);
                    anim.AnimationEnd += delegate { view.Visibility = ViewStates.Invisible; };
                    anim.Start();
                }
                catch (System.Exception)
                {
                    view.Visibility = ViewStates.Invisible;
                }
            }
        }

        public static Bitmap BlurBitmap(Bitmap smallBitmap, int radius, Context context)
        {
            Bitmap bitmap = Bitmap.CreateBitmap(
                smallBitmap.Width, smallBitmap.Height,
                Bitmap.Config.Argb8888);

            RenderScript renderScript = RenderScript.Create(context);

            Allocation blurInput = Allocation.CreateFromBitmap(renderScript, smallBitmap);
            Allocation blurOutput = Allocation.CreateFromBitmap(renderScript, bitmap);

            ScriptIntrinsicBlur blur = ScriptIntrinsicBlur.Create(renderScript,
                Element.U8_4(renderScript));
            blur.SetInput(blurInput);
            blur.SetRadius(radius); // radius must be 0 < r <= 25
            blur.ForEach(blurOutput);

            blurOutput.CopyTo(bitmap);
            renderScript.Destroy();

            return bitmap;
        }

        public static void AddTabLayoutTab(TabLayout tl, string label, bool defaultTab)
        {
            int ind = tl.TabCount;
            var t = tl.NewTab();
            t.SetText(label);
            t.SetTag(tl.TabCount);
            tl.AddTab(t);
            if (defaultTab)
                tl.GetTabAt(ind).Select();
        }

        public static void DisableShiftMode(BottomNavigationView view)
        {
            BottomNavigationMenuView menuView = (BottomNavigationMenuView) view.GetChildAt(0);
            try
            {
                var sm = menuView.Class.GetDeclaredField("mShiftingMode");
                sm.Accessible = true;
                sm.SetBoolean(menuView, false);
                sm.Accessible = false;
                for (int i = 0; i < menuView.ChildCount; i++)
                {
                    BottomNavigationItemView item = (BottomNavigationItemView) menuView.GetChildAt(i);
                    item.SetShiftingMode(false);
                    item.SetChecked(item.ItemData.IsChecked);
                }
            }
            catch (System.Exception)
            {
            }
        }

        public class GestureDetector
        {
            private float startX, startY;
            private long timeStart, timeEnd;
            private bool moved = false;
            private FrameLayout.LayoutParams layoutParams;

            public delegate void GestureListener(int id);

            public GestureListener OnGestureEvent;

            public GestureDetector(FrameLayout v, Context c)
            {
                var iv = new ImageView(c);
                iv.SetImageResource(Resource.Drawable.touch_100_white);
                int size = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, 50, Resources.System.DisplayMetrics);
                int halfSize = size / 2;
                layoutParams = new FrameLayout.LayoutParams(size, size);
                iv.LayoutParameters = layoutParams;
                iv.Visibility = ViewStates.Gone;

                v.AddView(iv);

                v.Touch += delegate(object sender, View.TouchEventArgs args)
                {
                    float curX = args.Event.GetX();
                    float curY = args.Event.GetY();

                    layoutParams.MarginStart = (int) curX - halfSize;
                    layoutParams.TopMargin = (int) curY - halfSize;
                    iv.LayoutParameters = layoutParams;

                    switch (args.Event.ActionMasked)
                    {
                        case MotionEventActions.Down:
                            moved = false;
                            startX = curX;
                            startY = curY;
                            timeStart = CurrentTimeMillis();
                            FadeVisibility(true, iv);
                            //iv.Visibility = ViewStates.Visible;
                            break;
                        case MotionEventActions.Move:
                            moved = true;
                            break;
                        case MotionEventActions.Up:
                            timeEnd = CurrentTimeMillis();
                            /*Toast.MakeText(Application.Context,
                                    $"TIME: {timeEnd - timeStart}\nPIXEL: {(curX - startX) + (curY - startY)}",
                                    ToastLength.Short)
                                .Show();*/
                            if (!moved)
                            {
                                OnGestureEvent?.Invoke(5);
                            }
                            else
                            {
                                OnMove(startX, startY, curX, curY, timeStart, timeEnd);
                            }
                            FadeVisibility(false, iv);
                            //iv.Visibility = ViewStates.Invisible;
                            break;
                    }
                };
            }

            public bool OnMove(float x1, float y1, float x2, float y2, long tStart, long tEnd)
            {
                long timeDif = tEnd - tStart;
                if (timeDif < 110)
                {
                    OnGestureEvent?.Invoke(5);
                }
                else
                {
                    int s = getSlope(x1, y1, x2, y2);
                    OnGestureEvent?.Invoke(s);
                }
                return false;
            }

            private int getSlope(float x1, float y1, float x2, float y2)
            {
                float xDif = x2 - x1;
                float yDif = y2 - y1;

                /*
                if (xDif > -5 && xDif < 5 && yDif > -5 && xDif < 5)
                    return 5;*/

                double angle = Java.Lang.Math.ToDegrees(Java.Lang.Math.Atan2(y1 - y2, x2 - x1));
                if (angle > 45 && angle <= 135)
                    // top
                    return 1;
                if (angle >= 135 && angle < 180 || angle < -135 && angle > -180)
                    // left
                    return 2;
                if (angle < -45 && angle >= -135)
                    // down
                    return 3;
                if (angle > -45 && angle <= 45)
                    // right
                    return 4;
                return 5;
            }
        }

        public static long CurrentTimeMillis()
        {
            var m = Java.Lang.JavaSystem.CurrentTimeMillis();
            return m;
        }
    }
}