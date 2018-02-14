using System;
using Android.Animation;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using AppOnkyo.SERVICE;

namespace AppOnkyo.FRAGMENTS
{
    class ControlFragment : BrinFragment
    {
        private const int CHILD_NOT_CONNECTED = 1, CHILD_NORM = 0, CHILD_WAIT = 2;

        [InjectView(Resource.Id.svMain)] ScrollView svMain;
        [InjectView(Resource.Id.vfMain)] ViewFlipper vfMain;
        [InjectView(Resource.Id.swPower)] Switch swPower;
        [InjectView(Resource.Id.fbMute)] FloatingActionButton fbMute;

        [InjectView(Resource.Id.sbBass)] BrinSeekBar sbBass;
        [InjectView(Resource.Id.sbTreble)] BrinSeekBar sbTreble;
        [InjectView(Resource.Id.sbVol)] BrinSeekBar sbVol;
        [InjectView(Resource.Id.sbCTL)] BrinSeekBar sbCTL;

        [InjectView(Resource.Id.tvSlp)] TextView tvSLP;
        [InjectView(Resource.Id.tvIFVFpsIn)] TextView tvIFVFpsIn;
        [InjectView(Resource.Id.tvIFVFpsOut)] TextView tvIFVFpsOut;
        [InjectView(Resource.Id.tvIFAFreq)] TextView tvIFAFreq;
        [InjectView(Resource.Id.tvIFAChIn)] TextView tvIFAChIn;
        [InjectView(Resource.Id.tvIFAChOut)] TextView tvIFAChOut;
        [InjectView(Resource.Id.tvBass)] TextView tvBass;
        [InjectView(Resource.Id.tvTreble)] TextView tvTreble;
        [InjectView(Resource.Id.tvCTL)] TextView tvCTL;
        [InjectView(Resource.Id.tvInpVal)] TextView tvInpVal;
        [InjectView(Resource.Id.tvSouVal)] TextView tvSouVal;
        [InjectView(Resource.Id.tvVolVal)] TextView tvVolVal;

        [InjectView(Resource.Id.tvNetTitle1)] BrinScrollTextView tvNetTitle1;
        [InjectView(Resource.Id.tvNetTitle2)] BrinScrollTextView tvNetTitle2;
        [InjectView(Resource.Id.tvNetTitle3)] BrinScrollTextView tvNetTitle3;


        [InjectView(Resource.Id.ivInpVal)] ImageView ivInpVal;
        [InjectView(Resource.Id.ivSouVal)] ImageView ivSouVal;
        [InjectView(Resource.Id.ivIFVResIn)] ImageView ivIFVResIn;
        [InjectView(Resource.Id.ivIFVResOut)] ImageView ivIFVResOut;

        [InjectView(Resource.Id.ivSheetArrow)] ImageView ivSheetArrow;
        [InjectView(Resource.Id.ivIFAFormat)] ImageView ivIFAFormat;
        [InjectView(Resource.Id.ivNetCover)] ImageView ivNetCover;


        [InjectView(Resource.Id.llChildAudio)] LinearLayout llChildAudio;
        [InjectView(Resource.Id.llChildVideo)] LinearLayout llChildVideo;
        [InjectView(Resource.Id.llChildBass)] LinearLayout llChildBass;
        [InjectView(Resource.Id.llChildTreble)] LinearLayout llChildTreble;
        [InjectView(Resource.Id.llChildCTL)] LinearLayout llChildCTL;
        [InjectView(Resource.Id.llChildVol)] LinearLayout llChildVol;
        [InjectView(Resource.Id.llChildInp)] LinearLayout llChildInp;
        [InjectView(Resource.Id.llChildSou)] LinearLayout llChildSou;
        [InjectView(Resource.Id.llChildNet)] LinearLayout llChildNet;
        [InjectView(Resource.Id.llChildSlp)] LinearLayout llChildSlp;
        [InjectView(Resource.Id.llContItems)] LinearLayout llContItems;

        [InjectView(Resource.Id.flSheet)] FrameLayout flSheet;
        [InjectView(Resource.Id.flBlock)] FrameLayout flBlock;

        private bool _userVisibleHint;
        private static BottomSheetBehavior bsb;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle b)
        {
            try
            {
                var layout = inflater.Inflate(Resource.Layout.layout_control, container, false);
                Inject(layout);

                swPower.Click += SwPowerOnCheckedChange;
                sbVol.OnProgressChanged += SbVolOnProgressChanged;
                sbCTL.OnProgressChanged += SbCtlOnProgressChanged;
                sbBass.OnProgressChanged += SbBassOnProgressChanged;
                sbTreble.OnProgressChanged += SbTrebleOnProgressChanged;

                bsb = BottomSheetBehavior.From(flSheet);
                var bc = new ViewHelper.BrinBottomSheetCallBack
                {
                    OnSlideChanged = OnSlideChanged,
                    OnStateStatusChanged = OnStateStatusChanged
                };
                bsb.SetBottomSheetCallback(bc);

                llChildAudio.Visibility = ViewStates.Gone;
                llChildVideo.Visibility = ViewStates.Gone;
                llChildInp.Visibility = ViewStates.Gone;
                llChildSou.Visibility = ViewStates.Gone;
                llChildNet.Visibility = ViewStates.Gone;
                llChildSlp.Visibility = ViewStates.Gone;


                OpenBroadcast();


                return layout;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void CollapseLevels()
        {
            if (bsb.State == BottomSheetBehavior.StateExpanded)
            {
                bsb.State = BottomSheetBehavior.StateCollapsed;
            }
        }

        public static void ExpandLevels()
        {
            if (bsb.State == BottomSheetBehavior.StateCollapsed)
            {
                bsb.State = BottomSheetBehavior.StateExpanded;
            }
        }

        protected override void OnArtResult(string deviceId, int status)
        {
            try
            {
                switch (status)
                {
                    case Constants.STAT_ART_LOADING:
                        ivNetCover.SetImageBitmap(null);
                        break;
                    case Constants.STAT_ART_ERR:
                        ivNetCover.SetImageBitmap(null);
                        break;
                    case Constants.STAT_ART_DONE:
                        Bitmap bm = DeviceService.GetNetArt();
                        if (bm == null)
                            throw new NullReferenceException();
                        ivNetCover.SetImageBitmap(bm);
                        ivNetCover.Visibility = ViewStates.Visible;
                        return;
                }
            }
            catch (Exception e)
            {
            }
            ivNetCover.Visibility = ViewStates.Gone;
        }


        private void SbTrebleOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs pcea)
        {
            if (!pcea.FromUser)
                return;
            DeviceService.SendCommand(CmdHelper.ToneFront.SetTreble((pcea.Progress / 10) - 10));
        }

        private void SbBassOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs pcea)
        {
            if (!pcea.FromUser)
                return;
            DeviceService.SendCommand(CmdHelper.ToneFront.SetBass((pcea.Progress / 10) - 10));
        }

        private void SbCtlOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs pcea)
        {
            if (!pcea.FromUser)
                return;
            DeviceService.SendCommand(CmdHelper.CenterSpeaker.Set((pcea.Progress / 10) - 12));
        }

        private void OnStateStatusChanged(View bottomSheet, int newState)
        {
        }

        private void OnSlideChanged(View bs, float slideOffset)
        {
            fbMute.Animate().ScaleX(1 - slideOffset).ScaleY(1 - slideOffset).SetDuration(0).Start();
            ivSheetArrow.Rotation = (1 - slideOffset) * 180;
        }

        private void SwPowerOnCheckedChange(object s, EventArgs e)
        {
            DeviceService.SendCommand(CmdHelper.Power.Set(swPower.Checked));
        }

        private void SbVolOnProgressChanged(object s, SeekBar.ProgressChangedEventArgs pcea)
        {
            if (!pcea.FromUser)
                return;
            DeviceService.SendCommand(CmdHelper.Volume.Set(pcea.Progress / 10));
        }

        private void Animate(BrinSeekBar v, string val, int i)
        {
            if (v.isSeekActive)
                return;
            ObjectAnimator animation = ObjectAnimator.OfInt(v, val, i);
            animation.SetDuration(200); // 0.5 second
            animation.SetInterpolator(new AccelerateDecelerateInterpolator());
            animation.Start();
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            Log.Debug("OnServiceMsg: ", msg);
            var res = ISCPHelper.Parse(msg);
            switch (res[0])
            {
                case CmdHelper.SleepTimer.Com:
                    try
                    {
                        var slp = new CmdHelper.SleepTimer(res[1]);
                        if (slp.Active)
                        {
                            tvSLP.Text = $"{slp.TimeLeft} {(slp.TimeLeft == 1 ? "minute" : "minutes")} left";
                        }
                        else
                        {
                            throw new Exception("Sleep timer not active");
                        }
                        llChildSlp.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        llChildSlp.Visibility = ViewStates.Gone;
                    }
                    break;

                case CmdHelper.NetTitleName.Com:
                    var netTitle = new CmdHelper.NetTitleName(res[1]);
                    if (tvNetTitle1.Text != netTitle.TitleName)
                        tvNetTitle1.Visibility = ViewStates.Gone;
                    tvNetTitle1.Text = netTitle.TitleName;
                    tvNetTitle1.Visibility = ViewStates.Visible;
                    break;
                case CmdHelper.NetArtistName.Com:
                    var netArtist = new CmdHelper.NetArtistName(res[1]);
                    if (tvNetTitle2.Text != netArtist.ArtistName)
                        tvNetTitle2.Visibility = ViewStates.Gone;
                    tvNetTitle2.Text = netArtist.ArtistName;
                    tvNetTitle2.Visibility = ViewStates.Visible;
                    break;
                case CmdHelper.NetAlbumName.Com:
                    var netAlbum = new CmdHelper.NetAlbumName(res[1]);
                    if (tvNetTitle3.Text != netAlbum.AlbumName)
                        tvNetTitle3.Visibility = ViewStates.Gone;
                    tvNetTitle3.Text = netAlbum.AlbumName;
                    tvNetTitle3.Visibility = ViewStates.Visible;
                    break;

                case CmdHelper.NetStatus.Com:
                    try
                    {
                        var netStatus = new CmdHelper.NetStatus(res[1]);
                        if (netStatus.StatusPlay == CmdHelper.NetStatus.Status.STOP)
                            throw new Exception();
                        llChildNet.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        llChildNet.Visibility = ViewStates.Gone;
                    }
                    break;

                case CmdHelper.NetTime.Com:
                    /*
                    try
                    {
                        var netTime = new CmdHelper.NetTime(res[1]);
                        if (netTime.tsProg == null || netTime.tsAll == null)
                        {
                            pbNetProg.Indeterminate = true;
                        }
                        else
                        {
                            pbNetProg.Max = Convert.ToInt32(netTime.tsAll.Value.TotalSeconds);
                            pbNetProg.Progress = Convert.ToInt32(netTime.tsProg.Value.TotalSeconds);
                            pbNetProg.Indeterminate = false;
                        }
                        pbNetProg.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        pbNetProg.Visibility = ViewStates.Gone;
                    }*/
                    break;
                case CmdHelper.Volume.Com:
                    try
                    {
                        int vol = CmdHelper.Volume.Converter(res[1]);
                        //sbVol.Progress = vol;
                        tvVolVal.Text = vol.ToString();

                        Animate(sbVol, "progress", vol * 10);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    llChildVol.Visibility = ViewStates.Visible;
                    break;
                case CmdHelper.Power.Com:
                    bool isPwOn = CmdHelper.Power.Converter(res[1]);
                    swPower.Checked = isPwOn;

                    ViewHelper.AnimateVisibility(!isPwOn, flBlock);
                    //flBlock.Visibility = isPwOn ? ViewStates.Gone : ViewStates.Visible;
                    break;
                case CmdHelper.Mute.Com:
                    bool isMuted = CmdHelper.Mute.Converter(res[1]);

                    fbMute.SetImageResource(isMuted
                        ? Resource.Drawable.mute_off_100_white
                        : Resource.Drawable.mute_on_100_white);
                    break;
                case CmdHelper.Input.Com:
                    try
                    {
                        tvInpVal.Text = CmdHelper.Input.Converter(res[1]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        tvInpVal.Text = $"N/A ({res[1]})";
                    }
                    llChildInp.Visibility = ViewStates.Visible;
                    DeviceService.SendCommand(CmdHelper.AudioInformation.Request);
                    DeviceService.SendCommand(CmdHelper.VideoInformation.Request);
                    break;
                case CmdHelper.ListeningMode.Com:
                    try
                    {
                        tvSouVal.Text = CmdHelper.ListeningMode.Converter(res[1]);
                    }
                    catch (Exception e)
                    {
                        tvSouVal.Text = $"N/A ({res[1]})";
                        Console.WriteLine(e);
                    }
                    llChildSou.Visibility = ViewStates.Visible;
                    DeviceService.SendCommand(CmdHelper.AudioInformation.Request);
                    DeviceService.SendCommand(CmdHelper.VideoInformation.Request);
                    break;
                case CmdHelper.CenterSpeaker.Com:
                    try
                    {
                        int vol = CmdHelper.CenterSpeaker.Converter(res[1]);
                        tvCTL.Text = vol.ToString();

                        Animate(sbCTL, "progress", (vol + 12) * 10);
                        llChildCTL.Visibility = ViewStates.Visible;
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        llChildCTL.Visibility = ViewStates.Gone;
                    }
                    break;
                case CmdHelper.ToneFront.Com:
                    try
                    {
                        var tfRes = CmdHelper.ToneFront.Converter(res[1]);

                        if (tfRes.Bass)
                        {
                            tvBass.Text = tfRes.BassVal.ToString();

                            Animate(sbBass, "progress", (tfRes.BassVal + 10) * 10);
                            llChildBass.Visibility = ViewStates.Visible;
                        }
                        if (tfRes.Treble)
                        {
                            tvTreble.Text = tfRes.TrebleVal.ToString();

                            Animate(sbTreble, "progress", (tfRes.TrebleVal + 10) * 10);
                            llChildTreble.Visibility = ViewStates.Visible;
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        llChildBass.Visibility = ViewStates.Gone;
                        llChildTreble.Visibility = ViewStates.Gone;
                    }
                    return;
                case CmdHelper.AudioInformation.Com:
                    try
                    {
                        var resIFA = CmdHelper.AudioInformation.Converter(res[1]);
                        int formatRes = Resource.Drawable.na_100_black;
                        string f = resIFA.format.ToLower();
                        if (f.Contains("dolby"))
                        {
                            if (f.Contains("true"))
                            {
                                formatRes = Resource.Drawable.dolby_true_hd;
                            }
                            else
                            {
                                formatRes = Resource.Drawable.dolby_100_black;
                            }
                        }
                        else if (f.Contains("dts"))
                        {
                            if (f.Contains("hd"))
                            {
                                formatRes = Resource.Drawable.dts_hd;
                            }
                            else
                            {
                                formatRes = Resource.Drawable.dts_100_black;
                            }
                        }
                        else if (f.Contains("pcm"))
                        {
                            formatRes = Resource.Drawable.pcm_100_black;
                        }

                        ivIFAFormat.SetImageResource(formatRes);
                        tvIFAFreq.Text = $"{resIFA.freq}";
                        tvIFAChIn.Text = $"{resIFA.channelIn}";
                        tvIFAChOut.Text = $"{resIFA.channelOut}";

                        llChildAudio.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        llChildAudio.Visibility = ViewStates.Gone;
                    }
                    return;
                case CmdHelper.VideoInformation.Com:
                    try
                    {
                        var resIFV = CmdHelper.VideoInformation.Converter(res[1]);
                        int iconResInRes = Resource.Drawable.no_video_100_black;
                        string freqIn = null;
                        if (resIFV.resIn.Contains("x"))
                        {
                            if (resIFV.resIn.Contains("1080"))
                            {
                                iconResInRes = Resource.Drawable.res_hd_1080p_100;
                            }
                            else if (resIFV.resIn.Contains("720"))
                            {
                                iconResInRes = Resource.Drawable.res_hd_720p_100;
                            }
                            else if (resIFV.resIn.Contains("2160"))
                            {
                                iconResInRes = Resource.Drawable.res_4k_100_black;
                            }
                            if (resIFV.resIn.Contains("Hz"))
                            {
                                freqIn = resIFV.resIn.Substring(resIFV.resIn.IndexOf("Hz") - 3, 2) + "\nHz";
                            }
                        }

                        tvIFVFpsIn.Text = freqIn ?? "N/A";
                        ivIFVResIn.SetImageResource(iconResInRes);


                        int iconResOutRes = Resource.Drawable.no_video_100_black;
                        string freqOut = null;
                        if (resIFV.resOut.Contains("x"))
                        {
                            if (resIFV.resOut.Contains("1080"))
                            {
                                iconResOutRes = Resource.Drawable.res_hd_1080p_100;
                            }
                            else if (resIFV.resOut.Contains("720"))
                            {
                                iconResOutRes = Resource.Drawable.res_hd_720p_100;
                            }
                            else if (resIFV.resOut.Contains("2160"))
                            {
                                iconResOutRes = Resource.Drawable.res_4k_100_black;
                            }
                            if (resIFV.resOut.Contains("Hz"))
                            {
                                freqOut = resIFV.resOut.Substring(resIFV.resOut.IndexOf("Hz") - 3, 2) + "\nHz";
                            }
                        }

                        tvIFVFpsOut.Text = freqOut ?? "N/A";
                        ivIFVResOut.SetImageResource(iconResOutRes);

                        llChildVideo.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        llChildVideo.Visibility = ViewStates.Gone;
                    }
                    return;
            }
        }

        public void CollapseLevels(MotionEvent ev)
        {
            if (bsb.State == BottomSheetBehavior.StateExpanded)
            {
                Rect outRect = new Rect();
                flSheet.GetGlobalVisibleRect(outRect);

                if (!outRect.Contains((int) ev.RawX, (int) ev.RawY))
                bsb.State = BottomSheetBehavior.StateCollapsed;
            }
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
            vfMain.DisplayedChild = CHILD_NOT_CONNECTED;
            llChildVol.Visibility = ViewStates.Gone;
            llChildInp.Visibility = ViewStates.Gone;
            llChildSou.Visibility = ViewStates.Gone;
        }

        protected override void OnServiceConnected(string deviceId)
        {
            vfMain.DisplayedChild = CHILD_NORM;
            DeviceService.SendCommand(CmdHelper.Power.Request);
            DeviceService.SendCommand(CmdHelper.Volume.Request);
            DeviceService.SendCommand(CmdHelper.Input.Request);
            DeviceService.SendCommand(CmdHelper.ListeningMode.Request);
            DeviceService.SendCommand(CmdHelper.NetStatus.Request);
            DeviceService.SendCommand(CmdHelper.Mute.Request);
            //DeviceService.SendCommand(CmdHelper.AudioInformation.Request);
            //DeviceService.SendCommand(CmdHelper.VideoInformation.Request);
            DeviceService.SendCommand(CmdHelper.SleepTimer.Request);
            DeviceService.SendCommand(CmdHelper.CenterSpeaker.Request);
            DeviceService.SendCommand(CmdHelper.ToneFront.Request);
        }

        protected override void OnServiceConnecting(string deviceId)
        {
            vfMain.DisplayedChild = CHILD_NOT_CONNECTED;
        }

        public override void OnDestroy()
        {
            base.OnPause();
            CloseBroadcast();
        }
    }
}