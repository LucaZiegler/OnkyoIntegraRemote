using System;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.Activities.Activation;
using AppOnkyo.Activities.Control;
using AppOnkyo.Activities.Control.Setup;
using AppOnkyo.Activities.Net;
using AppOnkyo.ADAPTER;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.DATASET;
using AppOnkyo.FRAGMENTS;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using AppOnkyo.RECEIVER;
using AppOnkyo.SERIAL;
using AppOnkyo.SERVICE;
using HockeyApp.Android;
using Debug = System.Diagnostics.Debug;
using Fragment = Android.Support.V4.App.Fragment;
using Thread = Java.Lang.Thread;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace AppOnkyo
{
    [Activity(Label = "Onkyo Remote", MainLauncher = true)]
    public class HomeActivity : BrinActivity
    {
        [InjectView(Resource.Id.vpMain)] BrinViewPager vpMain;

        [InjectView(Resource.Id.bnMain)] BottomNavigationView bnMain;

        [InjectView(Resource.Id.tbMain)] Toolbar tbMain;

        [InjectView(Resource.Id.ivBack)] ImageView ivBack;

        [InjectView(Resource.Id.dlMain)] DrawerLayout dlMain;

        [InjectView(Resource.Id.nvDrawer)] NavigationView nvDrawer;

        [InjectView(Resource.Id.ibDeviceOption)] ImageButton ibDeviceOption;

        [InjectView(Resource.Id.tvDrawerTitle)] TextView tvDrawerTitle;

        [InjectView(Resource.Id.tvDrawerSubTitle)] TextView tvDrawerSubTitle;

        private CmdHelper.SleepTimer cmdSlp = null;
        private ActionBarDrawerToggle abDrawerToggle;
        private readonly DeviceHelper deviceHelper = DeviceHelper.Instance();
        private ISubMenu smDrawerDevices;
        private ISubMenu smDrawerOther;
        private Preferences prefs = new Preferences();

        protected override void OnCreate(Bundle b)
        {
            base.OnCreate(b);
            CrashManager.Register(this,Constants.KEY_HOCKEY);
            try
            {
                SetContentView(Resource.Layout.layout_home);

                Inject();


                int intentTab = Intent.GetIntExtra(Constants.INTENT_HOME_TAB, 0);
                if (intentTab > 0)
                    SetCurrentTab(intentTab);

                bnMain.NavigationItemSelected +=
                    delegate(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
                    {
                        e.Handled = true;
                        SetCurrentTab(e.Item.Order);
                    };

                ViewHelper.DisableShiftMode(bnMain);

                ViewPagerAdapter vpAdapter = new ViewPagerAdapter(SupportFragmentManager);
                vpAdapter.AddFragment(new ControlFragment());
                vpAdapter.AddFragment(new RemoteFragment());
                vpAdapter.AddFragment(new ExtraFragment());
                vpAdapter.AddFragment(new AppSettingsFragment());
                vpMain.Adapter = vpAdapter;
                vpMain.OffscreenPageLimit = 3;


                SetSupportActionBar(tbMain);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                abDrawerToggle = new ActionBarDrawerToggle(this, dlMain, tbMain, Resource.String.app_name,
                    Resource.String.app_name)
                {
                    DrawerIndicatorEnabled = true
                };

                dlMain.AddDrawerListener(abDrawerToggle);
                abDrawerToggle.SyncState();

                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
                Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.SetStatusBarColor(Color.Transparent);


                smDrawerDevices = nvDrawer.Menu.AddSubMenu(0, 0, 0, "Saved receiver");
                smDrawerOther = nvDrawer.Menu.AddSubMenu("Other");
                smDrawerDevices.SetGroupCheckable(0, true, false);

                int i = 0;
                foreach (var device in deviceHelper.liDevices)
                {
                    AddDrawerItem(
                        smDrawerDevices,
                        i,
                        device.scanDevice.Model,
                        Resource.Drawable.av_receiver_100_black,
                        true,
                        0,
                        Resource.Layout.layout_home_drawer_item_action, device.id.ToString());

                    if (device.conFlag > 0)
                    {
                        ConnectWithDevice(i);
                    }
                    i++;
                }

                AddDrawerItem(
                    smDrawerOther, 0, "Add device",
                    Resource.Drawable.add_100_white,
                    true, 1, null, null);
                AddDrawerItem(
                    smDrawerOther, 1, "Sleep timer",
                    Resource.Drawable.sleep_timer_100_black,
                    true, 1, null, null);
                AddDrawerItem(
                    smDrawerOther, 2, "Buy key",
                    Resource.Drawable.key_100_black,
                    true, 1, null, null);
                AddDrawerItem(
                    smDrawerOther, 3, "Help",
                    Resource.Drawable.help_100_black,
                    true, 1, null, null);

                nvDrawer.NavigationItemSelected += OnNavDrawerItemSelected;
                nvDrawer.BringToFront();

                if (prefs.IsFirstAppStart())
                    ShowWelcomeDialog();

                RegisterMedia();

                OpenBroadcast();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Toast.MakeText(this, e.Message, ToastLength.Long).Show();
            }
        }

        private void ConnectWithDevice(int i)
        {
            var device = deviceHelper.liDevices[i];
            if (DeviceService.isServiceRunning &&
                DeviceService._dsp?.device?.Identifier == device.scanDevice.Identifier)
                return;

            int delay = 0;
            if (DeviceService.isServiceRunning)
            {
                DeviceService.StopService(true);
                delay = 400;
            }

            nvDrawer.PostDelayed(delegate
            {
                try
                {
                    var dsp = new DeviceServiceParameter();
                    dsp.device = device.scanDevice;
                    dsp.conFlag = device.conFlag;
                    dsp.id = device.id;
                    StartDeviceService(dsp);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Toast.MakeText(this, $"Internal connection error {e.Message}", ToastLength.Long).Show();
                }
            }, delay);
        }

        private void RegisterMedia()
        {
            this.VolumeControlStream = Stream.Music;
        }

        private void AddDrawerItem(ISubMenu sm, int ind, string title, int icon, bool checkable, int groupId,
            int? actionLayout, string actionId)
        {
            var it = sm.Add(groupId, ind, ind, title);
            it.SetCheckable(checkable);
            it.SetIcon(icon);

            if (actionLayout.HasValue)
            {
                var v = LayoutInflater.Inflate(actionLayout.Value, null, false);
                v.Tag = actionId;
                it.SetActionView(v);
            }
        }

        private void OnNavDrawerItemSelected(object o, NavigationView.NavigationItemSelectedEventArgs nisea)
        {
            try
            {
                int itemInd = nisea.MenuItem.Order;

                switch (nisea.MenuItem.GroupId)
                {
                    case 0:
                        ConnectWithDevice(itemInd);
                        nisea.Handled = false;
                        break;
                    case 1:
                        switch (nisea.MenuItem.Order)
                        {
                            case 0:
                                if (deviceHelper.liDevices.Count == 0 ||
                                    prefs.spMain.GetBoolean(Constants.PREF_KEY_BOUGHT, false))
                                    ShowAddDeviceDialog();
                                else
                                {
                                    Toast.MakeText(this, $"Please buy the key to save multiple devices",
                                        ToastLength.Short).Show();
                                }
                                break;
                            case 1:
                                ShowSlpDialog();
                                break;
                            case 2:
                                StartActivity(new Intent(Application.Context, typeof(DonateActivity)));
                                break;
                            case 3:
                                Intent emailIntent = new Intent(Intent.ActionSendto, Android.Net.Uri.FromParts(
                                    "mailto", "luca.ziegler@rocketmail.com", null));
                                emailIntent.PutExtra(Intent.ExtraSubject, "Help");
                                //emailIntent.PutExtra(Intent.ExtraText, "Body");
                                StartActivity(Intent.CreateChooser(emailIntent, "Send email to developer..."));
                                break;
                        }
                        nisea.Handled = false;
                        break;
                }
            }
            catch (Exception e)
            {
                Toast.MakeText(this, e.Message, ToastLength.Short).Show();
            }
        }

        private void ShowDeviceDialog(int itemInd)
        {
            var device = deviceHelper.liDevices[itemInd];

            var dia = new Dialog(this);
            dia.SetContentView(Resource.Layout.dialog_pair_device);

            var tvTitle = dia.FindViewById<TextView>(Resource.Id.tvDiaTitle);
            var tvMsg = dia.FindViewById<TextView>(Resource.Id.tvDiaMsg);
            var btNeg = dia.FindViewById<Button>(Resource.Id.btDiaNeg);
            var btPos = dia.FindViewById<Button>(Resource.Id.btDiaPos);

            tvTitle.Text = device.scanDevice.Model;
            tvMsg.Text =
                $"IP-address: {device.scanDevice.IpAddress}:{device.scanDevice.Port}\nIdentifier: {device.scanDevice.Identifier}\nInternal-ID: {itemInd}";

            btNeg.Click += delegate
            {
                // Delete
                deviceHelper.DeleteDevice(itemInd);
                smDrawerDevices.RemoveItem(itemInd);
                if (DeviceService.isServiceConnected)
                {
                    if (device.id == DeviceService._dsp.id)
                    {
                        DeviceService.StopService(true);
                    }
                }

                dia.Dismiss();
            };
            btPos.Click += delegate
            {
                // Close
                dia.Dismiss();
            };

            dia.Show();
        }

        private void ShowWelcomeDialog()
        {
            var dia = new Dialog(this);
            dia.SetContentView(Resource.Layout.dialog_welcome);
            var btOk = dia.FindViewById<Button>(Resource.Id.btDiaOk);
            btOk.Click += delegate { dia.Dismiss(); };
            dia.Show();
        }


        protected override void OnPostCreate(Bundle b)
        {
            base.OnPostCreate(b);
            abDrawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            abDrawerToggle.SyncState();
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs ueea)
        {
            Toast.MakeText(this, ueea.ExceptionObject.ToString(), ToastLength.Long).Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            int menuId = -1;
            switch (vpMain.CurrentItem)
            {
                case 0:
                    menuId = Resource.Menu.menu_frag_control;
                    break;
                case 1:
                    menuId = Resource.Menu.menu_frag_remote;
                    break;
                case 3:
                    menuId = Resource.Menu.menu_frag_device;
                    break;
            }
            if (menuId >= 0)
                MenuInflater.Inflate(menuId, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.abKey:
                    StartActivity(new Intent(Application.Context, typeof(DonateActivity)));
                    break;
                case Resource.Id.abCec:
                    SetCurrentTab(2);
                    bnMain.PostDelayed((() =>
                    {
                        var intent = new Intent(this, typeof(GeneralSetupActivity));
                        intent.PutExtra(Constants.INTENT_HDMI_CEC_SETUP, 2);
                        StartActivity(intent);
                    }), 200);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private Fragment GetCurrentFragment()
        {
            return SupportFragmentManager.FindFragmentByTag("android:switcher:" + Resource.Id.vpMain + ":" +
                                                            vpMain.CurrentItem);
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            var res = ISCPHelper.Parse(msg);
            switch (res[0])
            {
                case CmdHelper.SleepTimer.Com:
                    try
                    {
                        cmdSlp = new CmdHelper.SleepTimer(res[1]);
                    }
                    catch (Exception)
                    {
                        cmdSlp = null;
                    }
                    break;
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (vpMain.CurrentItem != 0)
                return base.DispatchTouchEvent(ev);

            if (ev.Action == MotionEventActions.Down)
            {
                ((ControlFragment) GetCurrentFragment()).CollapseLevels(ev);
            }

            return base.DispatchTouchEvent(ev);
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
            try
            {
                SupportActionBar.Subtitle = "Disconnected";

                tvDrawerTitle.Text = "Not connected";
                tvDrawerSubTitle.Text = null;

                ibDeviceOption.Visibility = ViewStates.Gone;

                ViewHelper.AnimateVisibility(false, ivBack);
                ivBack.SetImageBitmap(null);

                UncheckNavigation();
                ShowDrawer(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void UncheckNavigation()
        {
            int size = smDrawerDevices.Size();
            for (int i = 0; i < size; i++)
            {
                smDrawerDevices.GetItem(i).SetChecked(false);
            }
        }

        protected override void OnServiceConnected(string deviceId)
        {
            var dsp = DeviceService._dsp;
            SupportActionBar.Subtitle = $"Connected with {dsp.device.Model}";

            tvDrawerTitle.Text = dsp.device.Model;
            tvDrawerSubTitle.Text = $"Connected";

            //ibDeviceOption.Visibility = ViewStates.Visible;

            UpdateDeviceItem(dsp.id, true);
        }

        private void UpdateDeviceItem(long deviceId, bool active)
        {
            int i = 0;
            foreach (var device in deviceHelper.liDevices)
            {
                if (device.id == deviceId)
                {
                    smDrawerDevices.GetItem(i).SetChecked(active);
                    return;
                }
                i++;
            }
        }

        protected override void OnServiceConnecting(string deviceId)
        {
            SupportActionBar.Subtitle = "Connecting...";

            var dsp = DeviceService._dsp;
            tvDrawerTitle.Text = dsp.device.Model;
            tvDrawerSubTitle.Text = "Connecting...";

            ibDeviceOption.Visibility = ViewStates.Visible;

            UpdateDeviceItem(dsp.id, true);
        }

        protected override void OnArtResult(string deviceId, int status)
        {
            try
            {
                switch (status)
                {
                    case Constants.STAT_ART_LOADING:
                        break;
                    case Constants.STAT_ART_ERR:
                        break;
                    case Constants.STAT_ART_DONE:
                        Bitmap bm = DeviceService.GetNetArt();
                        if (bm == null)
                            throw new NullReferenceException();
                        bm = ViewHelper.BlurBitmap(bm, 15, Application.Context);
                        ivBack.SetImageBitmap(bm);
                        ViewHelper.AnimateVisibility(true, ivBack);
                        return;
                }
            }
            catch (Exception e)
            {
            }
            ViewHelper.AnimateVisibility(false, ivBack);
            ivBack.SetImageBitmap(null);
        }

        [Java.Interop.Export("OnDeviceOption")]
        public void OnDeviceOption(View v)
        {
            SetCurrentTab(3);
        }

        [Java.Interop.Export("OnEnableHdmiCecClick")]
        public void OnEnableHdmiCecClick(View v)
        {
            DeviceService.SendCommand(CmdHelper.HdmiCec.Set(true));
        }

        private void ShowAddDeviceDialog()
        {
            if (DialogDeviceScanFragment.isVisible)
                return;

            var dia = new DialogDeviceScanFragment(OnDialogDeviceScanResult);
            dia.Show(FragmentManager, "DEVICE_DIA");
        }

        private void OnDialogDeviceScanResult(PairedDevice pd)
        {
            var sd = new StoredDevice
            {
                scanDevice = pd.scanDevice,
                conFlag = 2,
                id = pd.id
            };
            var conInd = deviceHelper.liDevices.Count;
            deviceHelper.liDevices.Add(sd);
            deviceHelper.Save();


            AddDrawerItem(
                smDrawerDevices,
                smDrawerDevices.Size(),
                sd.scanDevice.Model,
                Resource.Drawable.av_receiver_100_black,
                true,
                0,
                Resource.Layout.layout_home_drawer_item_action, pd.id.ToString());

            ConnectWithDevice(conInd);
        }

        [Java.Interop.Export("OnDrawerDeviceOption")]
        public void OnDrawerDeviceOption(View v)
        {
            try
            {
                long id = long.Parse(v.Tag.ToString());
                int i = 0;
                foreach (var device in deviceHelper.liDevices)
                {
                    if (device.id == id)
                    {
                        ShowDeviceDialog(i);
                        return;
                    }
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Toast.MakeText(this, $"UI ERROR: {e.Message}", ToastLength.Short).Show();
            }
        }

        [Java.Interop.Export("OnIFA")]
        public void OnIFA(View v)
        {
            DeviceService.SendCommand(CmdHelper.AudioInformation.Request);
        }

        [Java.Interop.Export("OnIFV")]
        public void OnIFV(View v)
        {
            DeviceService.SendCommand(CmdHelper.VideoInformation.Request);
        }

        [Java.Interop.Export("OnSLP")]
        public void OnSLP(View v)
        {
            ShowSlpDialog();
        }

        public void ShowSlpDialog()
        {
            Dialog dialog = new Dialog(this);
            dialog.SetContentView(Resource.Layout.dialog_slp);

            var sbVal = dialog.FindViewById<SeekBar>(Resource.Id.sbDiaSlpVal);
            var tvVal = dialog.FindViewById<TextView>(Resource.Id.tvDiaSlpVal);

            sbVal.ProgressChanged += delegate(object sender, SeekBar.ProgressChangedEventArgs args)
            {
                tvVal.Text = args.Progress == 0 ? "OFF" : args.Progress.ToString("00");
                if (args.FromUser)
                    DeviceService.SendCommand(CmdHelper.SleepTimer.Set(args.Progress));
            };

            if (cmdSlp == null)
            {
                tvVal.Text = "N/A";
            }
            else
            {
                sbVal.Progress = cmdSlp.TimeLeft;
            }

            dialog.Show();
        }

        [Java.Interop.Export("OnRemoteEvent")]
        public void OnRemoteEvent(View v)
        {
            int id = Convert.ToInt32(v.Tag.ToString());
            var frag = (RemoteFragment) GetCurrentFragment();
            frag.OnRemoteEvent(id);
        }

        [Java.Interop.Export("OnOpenSoundList")]
        public void OnOpenSoundList(View v)
        {
            var intent = new Intent(this, typeof(InputSoundActivity));
            intent.PutExtra("TAB_IND", 1);
            StartActivity(intent);
        }

        [Java.Interop.Export("OnOpenInputList")]
        public void OnOpenInputList(View v)
        {
            var intent = new Intent(this, typeof(InputSoundActivity));
            intent.PutExtra("TAB_IND", 0);
            StartActivity(intent);
        }

        [Java.Interop.Export("OnVolDn")]
        public void OnVolDn(View v)
        {
            DeviceService.SendCommand(CmdHelper.Volume.Down);
        }

        [Java.Interop.Export("OnVolUp")]
        public void OnVolUp(View v)
        {
            DeviceService.SendCommand(CmdHelper.Volume.Up);
        }

        [Java.Interop.Export("OnMute")]
        public void OnMute(View v)
        {
            DeviceService.SendCommand(CmdHelper.Mute.Toggle);
        }

        [Java.Interop.Export("OnTabDevices")]
        public void OnTabDevices(View v)
        {
            ShowDrawer(true);
        }

        private void ShowDrawer(bool show)
        {
            if (show)
            {
                dlMain.OpenDrawer((int) GravityFlags.Left);
            }
            else
            {
                dlMain.CloseDrawer((int) GravityFlags.Left);
            }
        }

        [Java.Interop.Export("OnCTLDn")]
        public void OnCTLDn(View v)
        {
            DeviceService.SendCommand(CmdHelper.CenterSpeaker.Down);
        }

        [Java.Interop.Export("OnCTLUp")]
        public void OnCTLUp(View v)
        {
            DeviceService.SendCommand(CmdHelper.CenterSpeaker.Up);
        }

        [Java.Interop.Export("OnBassDn")]
        public void OnBassDn(View v)
        {
            DeviceService.SendCommand(CmdHelper.ToneFront.BassDown);
        }

        [Java.Interop.Export("OnBassUp")]
        public void OnBassUp(View v)
        {
            DeviceService.SendCommand(CmdHelper.ToneFront.BassUp);
        }

        [Java.Interop.Export("OnTrebleDn")]
        public void OnTrebleDn(View v)
        {
            DeviceService.SendCommand(CmdHelper.ToneFront.TrebleDown);
        }

        [Java.Interop.Export("OnTrebleUp")]
        public void OnTrebleUp(View v)
        {
            DeviceService.SendCommand(CmdHelper.ToneFront.TrebleUp);
        }

        [Java.Interop.Export("OnOpenPlayer")]
        public void OnOpenPlayer(View v)
        {
            try
            {
                var intent = new Intent(this, typeof(PlayerActivity));
                //StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetCurrentTab(int ind)
        {
            vpMain.SetCurrentItem(ind, true);
            bnMain.Menu.GetItem(ind).SetChecked(true);
            InvalidateOptionsMenu();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (DeviceService.isServiceRunning)
            {
                if (DeviceService._dsp?.conFlag < 2)
                    DeviceService.StopService(true);
            }
        }

        public void ShowSpeakerValues()
        {
            SetCurrentTab(0);
            ControlFragment.ExpandLevels();
        }
    }
}