using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AppOnkyo.SERVICE;

namespace AppOnkyo.ANDR_CUSTOM
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Constants.BROAD_NOTIFY_CONNECTION,Constants.BROAD_NOTIFY_PLAYER })]
    public class BrinNotfiListener : BroadcastReceiver
    {
        public delegate void ReceiveListener(string key,int r);

        public static ReceiveListener OnReceived;

        public override void OnReceive(Context context, Intent intent)
        {
            int a = intent.Extras.GetInt("ACT", -1);
            if (a > -1)
                OnReceived?.Invoke(intent.Action,a);
        }
    }
}