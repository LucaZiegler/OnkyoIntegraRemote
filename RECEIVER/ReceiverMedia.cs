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

namespace AppOnkyo.RECEIVER
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "android.media.VOLUME_CHANGED_ACTION" })]

    public class ReceiverMedia : BroadcastReceiver
    {
        public string ComponentName => Class.Name;

        public override void OnReceive(Context context, Intent intent)
        {

        }
    }
}