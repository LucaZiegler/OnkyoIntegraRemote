using Android.App;
using Android.Content;

namespace AppOnkyo.ANDR_CUSTOM
{
    [BroadcastReceiver]
    [IntentFilter(new[] {Constants.SERVICE_ID})]
    public class BrinBroadcastReceiver : BroadcastReceiver
    {
        public delegate void ReceiverListener(Context context, Intent intent);

        public ReceiverListener OnReceived;

        public override void OnReceive(Context context, Intent intent)
        {
            OnReceived?.Invoke(context, intent);
        }
    }
}