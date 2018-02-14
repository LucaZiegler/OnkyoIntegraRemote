using System;
using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AppOnkyo.HELPER;
using AppOnkyo.SERIAL;
using AppOnkyo.SERVICE;
using static AppOnkyo.Constants;

namespace AppOnkyo.ANDR_CUSTOM
{
    public abstract class BrinActivity : AppCompatActivity
    {
        private BrinBroadcastReceiver bbReceiver;
        public bool isActivityVisible = false;
        public bool isActivityActive = false;
        public bool isBroadcastOpen = false;

        protected override void OnCreate(Bundle b)
        {
            base.OnCreate(b);
            var s = new MediaSession(ApplicationContext, "asdf");
            s.SetCallback(new ButtonCallback());

            s.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
            s.Active = true;
        }

        public class ButtonCallback : MediaSession.Callback
        {
            public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
            {
                DeviceService.SendCommand(CmdHelper.Volume.Up);
                return true;
            }
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            //return base.DispatchKeyEvent(e);
            var action = e.Action;
            var keyCode = e.KeyCode;
            switch (keyCode)
            {
                case Keycode.VolumeUp:
                    if (action == KeyEventActions.Down)
                    {
                        DeviceService.SendCommand(CmdHelper.Volume.Up);
                    }
                    return true;
                case Keycode.VolumeDown:
                    if (action == KeyEventActions.Down)
                    {
                        DeviceService.SendCommand(CmdHelper.Volume.Down);
                    }
                    return true;
                default:
                    return base.DispatchKeyEvent(e);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return base.OnKeyDown(keyCode, e);
            if (keyCode == Keycode.VolumeDown)
            {
                DeviceService.SendCommand(CmdHelper.Volume.Down);
                return true;
            }

            if (keyCode == Keycode.VolumeUp)
            {
                DeviceService.SendCommand(CmdHelper.Volume.Up);
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
        /*
        public Listener.ServiceConnectedListener OnServiceConnected;
        public Listener.ServiceConnectingListener OnServiceConnecting;
        public Listener.ServiceDisconnectedListener OnServiceDisconnected;
        public Listener.ServiceMsgListener OnServiceMsg = null;*/

        public void Inject()
        {
            try
            {
                Cheeseknife.Inject(this);
            }
            catch (Exception e)
            {
                Toast.MakeText(this, $"Layout error: {e.Message}", ToastLength.Long).Show();
                Console.WriteLine(e);
                throw;
            }
        }

        public void StartDeviceService(DeviceServiceParameter dsp)
        {
            if (DeviceService.isServiceRunning)
                throw new Exception("Service läuft bereits");
            Intent si = new Intent(Application.Context, typeof(DeviceService));
            si.PutExtra(IntentHelper.INTENT_DSP_VAL, dsp.ToString());
            StartService(si);
        }

        public void OpenBroadcast()
        {
            Log.Debug("ACT_STAT", "0");
            if (isBroadcastOpen)
                return;
            Log.Debug("ACT_STAT", "1");

            bbReceiver = new BrinBroadcastReceiver();
            bbReceiver.OnReceived = delegate(Context context, Intent intent)
            {
                var dsr = IntentHelper.GetDeviceServiceResult(intent);
                switch (dsr.id)
                {
                    case SERVICE_STAT_CONNECTING:
                        OnServiceConnecting(dsr.deviceId);
                        break;
                    case SERVICE_STAT_CONNECTED:
                        OnServiceConnected(dsr.deviceId);
                        break;
                    case SERVICE_STAT_DISCONNECTED:
                        ServiceDisconnectReason sdr = ServiceDisconnectReason.DISC_NA;
                        if (dsr.discReason.HasValue)
                            sdr = dsr.discReason.Value;
                        OnServiceDisconnected(dsr.deviceId, sdr);
                        break;
                    case SERVICE_STAT_MSG:
                        OnServiceMsg(dsr.deviceId, dsr.extra);
                        break;
                    case SERVICE_STAT_ART:
                        OnArtResult(dsr.deviceId, Convert.ToInt32(dsr.extra));
                        break;
                }
            };
            IntentFilter i = new IntentFilter(SERVICE_ID);
            LocalBroadcastManager.GetInstance(Application.Context).RegisterReceiver(bbReceiver, i);
            OnBroadcastOpen();
            if (DeviceService.isServiceConnected)
            {
                OnServiceConnected(DeviceService._dsp?.device?.Identifier);
                if (DeviceService.GetNetArt() != null)
                {
                    OnArtResult(null, STAT_ART_DONE);
                }
            }
            else
            {
                OnServiceDisconnected(null, ServiceDisconnectReason.DISC_ALREADY);
            }
            isBroadcastOpen = true;
        }

        protected abstract void OnServiceMsg(string deviceId, string msg);

        protected abstract void OnServiceDisconnected(string deviceId, ServiceDisconnectReason sdr);

        protected abstract void OnServiceConnected(string deviceId);

        protected abstract void OnServiceConnecting(string deviceId);
        protected abstract void OnArtResult(string deviceId, int status);

        public void CloseBroadcast()
        {
            Log.Debug("ACT_STAT", "2");
            if (!isBroadcastOpen)
                return;
            Log.Debug("ACT_STAT", "3");
            LocalBroadcastManager.GetInstance(Application.Context).UnregisterReceiver(bbReceiver);
            OnBroadcastClosed();
            isBroadcastOpen = false;
        }

        public virtual void OnBroadcastOpen()
        {
        }

        public virtual void OnBroadcastClosed()
        {
        }
    }
}