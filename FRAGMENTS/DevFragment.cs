using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.SERVICE;

namespace AppOnkyo.FRAGMENTS
{
    class DevFragment : BrinFragment
    {
        [InjectView(Resource.Id.etMain)] EditText etMain;
        [InjectView(Resource.Id.tvMain)] TextView tvMain;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var layout = inflater.Inflate(Resource.Layout.layout_dev, container, false);
            Inject(layout);

            OpenBroadcast();

            etMain.EditorAction += delegate (object sender, TextView.EditorActionEventArgs args)
            {
                if (args.ActionId == ImeAction.Send)
                {
                    DeviceService.SendCommand(etMain.Text);
                }
            };

            return layout;
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            if (tvMain.Text.Length > 5000)
                tvMain.Text = "+++RESET+++";
            tvMain.Text = msg + "\n" + tvMain.Text;
        }

        protected override void OnServiceConnecting(string deviceId)
        {
        }

        protected override void OnServiceConnected(string deviceId)
        {
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
        }

        protected override void OnArtResult(string deviceId, int status)
        {
        }

        public override void OnDestroy()
        {
            base.OnPause();
            CloseBroadcast();
        }
    }
}