using System;
using Android.Animation;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using AppOnkyo.SERVICE;
using static AppOnkyo.Constants;

namespace AppOnkyo.FRAGMENTS
{
    public class RemoteFragment : BrinFragment
    {
        [InjectView(Resource.Id.tlMain)] TabLayout tlMain;
        [InjectView(Resource.Id.flGesture)] FrameLayout flGesture;
        [InjectView(Resource.Id.nvSheet)] FrameLayout nvSheet;

        [InjectView(Resource.Id.ibRemEject)] ImageButton ibRemEject;
        [InjectView(Resource.Id.ibRemPw)] ImageButton ibRemPw;

        [InjectView(Resource.Id.btRemDisp)] Button btRemDisp;
        [InjectView(Resource.Id.btRemAngle)] Button btRemAngle;
        [InjectView(Resource.Id.btRemClear)] Button btRemClear;
        [InjectView(Resource.Id.btHdmiCec)] Button btHdmiCec;

        [InjectView(Resource.Id.llRemMedia1)] LinearLayout llRemMedia1;
        [InjectView(Resource.Id.llRemMedia2)] LinearLayout llRemMedia2;
        [InjectView(Resource.Id.llRemHolder)] LinearLayout llRemHolder;

        [InjectView(Resource.Id.vfMain)] ViewFlipper vfMain;

        private ViewHelper.GestureDetector gt;
        private BottomSheetBehavior bsbMain;
        private bool _userVisibleHint;
        private Preferences prefsMain;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup c, Bundle b)
        {
            try
            {
                var layout = inflater.Inflate(Resource.Layout.layout_remote, c, false);
                Inject(layout);

                prefsMain = new Preferences();

                int lastTab = prefsMain.spMain.GetInt(PREF_FRAG_REM_LAST, 1);
                ViewHelper.AddTabLayoutTab(tlMain, "Player", lastTab == 0);
                ViewHelper.AddTabLayoutTab(tlMain, "Receiver", lastTab == 1);
                ViewHelper.AddTabLayoutTab(tlMain, "TV", lastTab == 2);

                tlMain.TabSelected += delegate
                {
                    prefsMain.PutInt(PREF_FRAG_REM_LAST, tlMain.SelectedTabPosition);
                    int lastState = BottomSheetBehavior.StateCollapsed;
                    if (bsbMain.State == BottomSheetBehavior.StateCollapsed ||
                        bsbMain.State == BottomSheetBehavior.StateExpanded)
                        lastState = bsbMain.State;
                    bsbMain.Hideable = true;
                    bsbMain.State = BottomSheetBehavior.StateHidden;

                    RefreshButtons();


                    nvSheet.Post(delegate
                    {
                        try
                        {
                            bsbMain.State = lastState;
                            bsbMain.Hideable = false;
                        }
                        catch (Exception e)
                        {
                            Toast.MakeText(Activity, $"UI Error: {e.Message}", ToastLength.Short).Show();
                        }
                    });
                };
                gt = new ViewHelper.GestureDetector(flGesture, Application.Context);
                gt.OnGestureEvent += delegate(int id)
                {
                    string cmd = GetCurCmd();
                    switch (id)
                    {
                        case 1:
                            cmd += CmdHelper.Menu.UP;
                            break;
                        case 2:
                            cmd += CmdHelper.Menu.LEFT;
                            break;
                        case 3:
                            cmd += CmdHelper.Menu.DOWN;
                            break;
                        case 4:
                            cmd += CmdHelper.Menu.RIGHT;
                            break;
                        case 5:
                            cmd += CmdHelper.Menu.ENTER;
                            break;
                    }
                    DeviceService.SendCommand($"{cmd}");
                };

                bsbMain = BottomSheetBehavior.From(nvSheet);
                var bsc = new ViewHelper.BrinBottomSheetCallBack();
                bsbMain.SetBottomSheetCallback(bsc);

                nvSheet.PostDelayed(delegate
                {
                    try
                    {
                        bsbMain.Hideable = true;
                        bsbMain.State = BottomSheetBehavior.StateHidden;
                        bsbMain.State = BottomSheetBehavior.StateCollapsed;
                        bsbMain.Hideable = false;
                    }
                    catch (Exception e)
                    {
                    }
                }, 1000);


                RefreshButtons();

                LayoutTransition transition = new LayoutTransition();
                transition.SetAnimateParentHierarchy(false);
                //llRemHolder.LayoutTransition = transition;

                OpenBroadcast();
                return layout;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void RefreshButtons()
        {
            int p = tlMain.SelectedTabPosition;
            llRemMedia2.Visibility = ViewHelper.VisibilityConverter(p == 0);

            ibRemEject.Visibility = ViewHelper.VisibilityConverter(p == 0);
            ibRemPw.Visibility = ViewHelper.VisibilityConverter(p != 1);
            btRemDisp.Visibility = ViewHelper.VisibilityConverter(p != 1);
            btRemAngle.Visibility = ViewHelper.VisibilityConverter(p == 0);
            btRemClear.Visibility = ViewHelper.VisibilityConverter(p != 1);
            llRemMedia1.Visibility = ViewHelper.VisibilityConverter(p == 0);
        }

        public override bool UserVisibleHint
        {
            get { return _userVisibleHint; }
            set
            {
                if (!value)
                {
                    try
                    {
                        if (bsbMain.State == BottomSheetBehavior.StateExpanded)
                            bsbMain.State = BottomSheetBehavior.StateCollapsed;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                else
                {
                }
                _userVisibleHint = value;
            }
        }

        private string GetCurCmd()
        {
            switch (tlMain.SelectedTabPosition)
            {
                case 0:
                    return CmdHelper.MenuPlayer.Com;
                case 1:
                    return CmdHelper.Menu.Com;
                case 2:
                    return CmdHelper.MenuTV.Com;
            }
            return null;
        }

        public void OnRemoteEvent(int id)
        {
            var cmd = GetCurCmd();
            switch (id)
            {
                case 0:
                    cmd += CmdHelper.MenuPlayer.RETURN;
                    break;
                case 1:
                    switch (tlMain.SelectedTabPosition)
                    {
                        case 0:
                            cmd += CmdHelper.MenuPlayer.TOPMENU;
                            break;
                        case 1:
                            cmd += CmdHelper.Menu.HOME;
                            break;
                        case 2:
                            cmd += CmdHelper.MenuTV.GUIDE_TOPMENU;
                            break;
                    }
                    break;
                case 2:
                    switch (tlMain.SelectedTabPosition)
                    {
                        case 0:
                            cmd += CmdHelper.Menu.MENU;
                            break;
                        case 1:
                            cmd += CmdHelper.Menu.HOME;
                            break;
                        case 2:
                            cmd += CmdHelper.MenuTV.INPUT;
                            break;
                    }
                    break;
                case 3:
                    cmd += CmdHelper.MenuPlayer.POWER;
                    break;
                case 4:
                    cmd += CmdHelper.MenuPlayer.OPEN_CLOSE;
                    break;
                case 5:
                    cmd += CmdHelper.MenuPlayer.DISP;
                    break;
                case 6:
                    cmd += CmdHelper.MenuPlayer.SKIP_BACK;
                    break;
                case 7:
                    cmd += CmdHelper.MenuPlayer.STOP;
                    break;
                case 8:
                    cmd += CmdHelper.MenuPlayer.PAUSE;
                    break;
                case 9:
                    cmd += CmdHelper.MenuPlayer.SKIP_FORW;
                    break;
                case 10:
                    cmd += CmdHelper.MenuPlayer.REW;
                    break;
                case 11:
                    cmd += CmdHelper.MenuPlayer.PLAY;
                    break;
                case 12:
                    cmd += CmdHelper.MenuPlayer.FF;
                    break;
                case 13:
                    cmd += CmdHelper.MenuPlayer.ANGLE;
                    break;
                case 14:
                    cmd += CmdHelper.MenuPlayer.SETUP;
                    break;
                case 15:
                    cmd += CmdHelper.MenuPlayer.CLEAR;
                    break;
            }
            DeviceService.SendCommand(cmd);
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            try
            {
                var res = ISCPHelper.Parse(msg);
                switch (res[0])
                {
                    case CmdHelper.HdmiCec.Com:
                        var cmdHdmiCecState = new CmdHelper.HdmiCec(res[1]);
                        vfMain.DisplayedChild = cmdHdmiCecState.HdmiCecEnabled ? 0 : 1;
                        break;
                    case CmdHelper.Power.Com:
                        var pw = CmdHelper.Power.Converter(res[1]);
                        btHdmiCec.Enabled = pw;
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnServiceConnecting(string deviceId)
        {
        }

        protected override void OnServiceConnected(string deviceId)
        {
            DeviceService.SendCommand(CmdHelper.HdmiCec.Request);
        }

        protected override void OnServiceDisconnected(string deviceId, ServiceDisconnectReason sdr)
        {
            btHdmiCec.Enabled = false;
        }

        protected override void OnArtResult(string deviceId, int status)
        {
        }
    }
}