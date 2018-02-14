using System;
using Android.App;
using Android.Content;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using AppOnkyo.HELPER;
using AppOnkyo.SERIAL;
using AppOnkyo.SERVICE;
using Fragment = Android.Support.V4.App.Fragment;
using static AppOnkyo.Constants;

namespace AppOnkyo.ANDR_CUSTOM
{
    public abstract class BrinFragment : Fragment
    {
        private BrinBroadcastReceiver bbReceiver;
        public bool isActivityVisible = false;
        public bool isActivityActive = false;
        public bool isBroadcastOpen = false;

        /*
        public Listener.ServiceConnectedListener OnServiceConnected;
        public Listener.ServiceConnectingListener OnServiceConnecting;
        public Listener.ServiceDisconnectedListener OnServiceDisconnected;
        public Listener.ServiceMsgListener OnServiceMsg = null;*/

        public void Inject(View layout)
        {
            Cheeseknife.Inject(this, layout);
        }

        public void StartDeviceService(DeviceServiceParameter dsp)
        {
            if (DeviceService.isServiceRunning)
            {
                if (DeviceService.isServiceConnecting)
                {
                    throw new Exception("Connecting...");
                }
                if (DeviceService.isServiceConnected)
                {
                    throw new Exception("Connected");
                }

                throw new Exception("Not connected");
            }
            Intent si = new Intent(Application.Context, typeof(DeviceService));
            si.PutExtra(IntentHelper.INTENT_DSP_VAL, dsp.ToString());
            Activity.StartService(si);
        }

        public void OpenBroadcast()
        {
            Log.Debug("FRAG_STAT", "0");
            if (isBroadcastOpen)
                return;
            Log.Debug("FRAG_STAT", "1");

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

        protected abstract void OnServiceConnecting(string deviceId);
        protected abstract void OnServiceConnected(string deviceId);
        protected abstract void OnServiceDisconnected(string deviceId, ServiceDisconnectReason reason);
        protected abstract void OnArtResult(string deviceId, int status);

        public void CloseBroadcast()
        {
            Log.Debug("FRAG_STAT", "2");
            if (!isBroadcastOpen)
                return;
            Log.Debug("FRAG_STAT", "3");
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

        public override bool UserVisibleHint
        {
            get { return base.UserVisibleHint; }
            set { base.UserVisibleHint = value; }
        }
    }
}