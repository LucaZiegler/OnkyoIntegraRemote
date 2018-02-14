using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using AppOnkyo.SERVICE;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace AppOnkyo.Activities.Net
{
    [Activity(Label = "PlayerActivity")]
    class PlayerActivity : BrinActivity
    {
        [InjectView(Resource.Id.tbMain)] Toolbar tbMain;
        [InjectView(Resource.Id.flSheet)] FrameLayout flSheet;

        [InjectView(Resource.Id.tvPlayerTitle1)] TextView tvPlayerTitle1;
        [InjectView(Resource.Id.tvPlayerTitle2)] TextView tvPlayerTitle2;
        [InjectView(Resource.Id.tvTime1)] TextView tvTime1;
        [InjectView(Resource.Id.tvTime2)] TextView tvTime2;

        [InjectView(Resource.Id.sbTime)] SeekBar sbTime;

        [InjectView(Resource.Id.ivCover)] ImageView ivCover;

        [InjectView(Resource.Id.ibPlayerPause)] ImageButton ibPlayerPause;

        private BottomSheetBehavior bsbMain;

        protected override void OnCreate(Bundle b)
        {
            try
            {
                base.OnCreate(b);
                SetContentView(Resource.Layout.layout_player);
                Inject();

                SetSupportActionBar(tbMain);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                tbMain.NavigationClick += delegate { OnBackPressed(); };

                bsbMain = BottomSheetBehavior.From(flSheet);
                bsbMain.Hideable = true;
                bsbMain.State = BottomSheetBehavior.StateHidden;

                OpenBroadcast();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Java.Interop.Export("OnMediaEvent")]
        public void OnMediaEvent(View v)
        {
            int i = Convert.ToInt32(v.Tag.ToString());
            string cmd = null;
            switch (i)
            {
                case 0:
                    cmd = CmdHelper.NetOperation.TRACK_DOWN;
                    break;
                case 1:
                    cmd = CmdHelper.NetOperation.STOP;
                    break;
                case 2:
                    cmd = CmdHelper.NetOperation.PAUSE;
                    break;
                case 3:
                    cmd = CmdHelper.NetOperation.TRACK_UP;
                    break;
                case 4:
                    cmd = CmdHelper.NetOperation.RANDOM;
                    break;
                case 5:
                    cmd = CmdHelper.NetOperation.REPEAT;
                    break;
            }
            DeviceService.SendCommand(CmdHelper.NetOperation.Set(cmd));
        }

        public override void OnBackPressed()
        {
            if (bsbMain.State == BottomSheetBehavior.StateExpanded)
            {
                bsbMain.State = BottomSheetBehavior.StateCollapsed;
                return;
            }
            base.OnBackPressed();
        }

        protected override void OnArtResult(string deviceId, int status)
        {
            try
            {
                switch (status)
                {
                    case Constants.STAT_ART_LOADING:
                        ivCover.SetImageBitmap(null);
                        break;
                    case Constants.STAT_ART_ERR:
                        ivCover.SetImageBitmap(null);
                        break;
                    case Constants.STAT_ART_DONE:
                        ivCover.SetImageBitmap(DeviceService.GetNetArt());
                        ivCover.Visibility = ViewStates.Visible;
                        return;
                }
            }
            catch (Exception e)
            {
            }
            ivCover.Visibility = ViewStates.Gone;
        }


        protected override void OnServiceMsg(string deviceId, string msg)
        {
            var res = ISCPHelper.Parse(msg);
            switch (res[0])
            {
                case CmdHelper.NetTime.Com:
                    try
                    {
                        var netTime = new CmdHelper.NetTime(res[1]);
                        tvTime1.Text = netTime.tsProg.Value.ToString(@"mm\:ss");
                        tvTime2.Text = netTime.tsAll.Value.ToString(@"mm\:ss");
                        sbTime.Progress = (int) netTime.tsProg.Value.TotalSeconds;
                        sbTime.Max = (int) netTime.tsAll.Value.TotalSeconds;
                    }
                    catch (Exception e)
                    {
                        sbTime.Enabled = false;
                        tvTime1.Text = "00:00";
                        tvTime2.Text = "00:00";
                    }
                    break;
                case CmdHelper.NetPopup.Com:
                    var netPopup = new CmdHelper.NetPopup(res[1]);
                    break;
                case CmdHelper.NetListInfo.Com:
                    var netList = new CmdHelper.NetListInfo(res[1]);
                    break;
                case CmdHelper.NetStatus.Com:
                    var netStatus = new CmdHelper.NetStatus(res[1]);
                    ibPlayerPause.SetImageResource(
                        netStatus.StatusPlay == CmdHelper.NetStatus.Status.PLAY
                            ? Resource.Drawable.pause_100_black
                            : Resource.Drawable.play_100_black
                    );
                    if (netStatus.StatusPlay != CmdHelper.NetStatus.Status.STOP)
                    {
                        DeviceService.SendCommand(CmdHelper.NetTitleName.Request);
                        DeviceService.SendCommand(CmdHelper.NetArtistName.Request);
                    }
                    ShowBottonSheet(netStatus.StatusPlay != CmdHelper.NetStatus.Status.STOP);
                    break;
                case CmdHelper.NetTitleName.Com:
                    try
                    {
                        var netTitleName = new CmdHelper.NetTitleName(res[1]);
                        if (netTitleName.TitleName != tvPlayerTitle1.Text)
                        {
                            tvPlayerTitle1.Visibility = ViewStates.Gone;
                            tvPlayerTitle1.Text = netTitleName.TitleName;
                        }
                        tvPlayerTitle1.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        tvPlayerTitle1.Visibility = ViewStates.Gone;
                    }
                    break;
                case CmdHelper.NetArtistName.Com:
                    try
                    {
                        var netArtistName = new CmdHelper.NetArtistName(res[1]);
                        if (netArtistName.ArtistName != tvPlayerTitle2.Text)
                        {
                            tvPlayerTitle2.Visibility = ViewStates.Gone;
                            tvPlayerTitle2.Text = netArtistName.ArtistName;
                        }
                        tvPlayerTitle2.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        tvPlayerTitle2.Visibility = ViewStates.Gone;
                    }
                    break;
            }
        }

        private void ShowBottonSheet(bool show)
        {
            if (show)
            {
                bsbMain.Hideable = false;
                if (bsbMain.State == BottomSheetBehavior.StateHidden)
                {
                    bsbMain.State = BottomSheetBehavior.StateExpanded;
                }
            }
            else
            {
                bsbMain.Hideable = true;
                if (bsbMain.State != BottomSheetBehavior.StateHidden)
                {
                    bsbMain.State = BottomSheetBehavior.StateHidden;
                }
            }
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
            Finish();
        }

        protected override void OnServiceConnected(string deviceId)
        {
            DeviceService.SendCommand(CmdHelper.NetStatus.Request);
        }

        protected override void OnServiceConnecting(string deviceId)
        {
            Finish();
        }
    }
}