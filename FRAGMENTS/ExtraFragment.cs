using System;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using AppOnkyo.Activities.Control.Setup;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;

namespace AppOnkyo.FRAGMENTS
{
    public class ExtraFragment : BrinFragment
    {

        [InjectView(Resource.Id.nvDrawer)] NavigationView nvDrawer;
        private int optionState = 0;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var layout = inflater.Inflate(Resource.Layout.layout_extra, container, false);
            Inject(layout);

            nvDrawer.NavigationItemSelected += OnNavItemSelected;

            OpenBroadcast();
            return layout;
        }

        private void OnNavItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs nisea)
        {
            switch (nisea.MenuItem.Order)
            {
                case 0:
                    StartActivity(new Intent(Activity,typeof(AudioSetupActivity)));
                    break;
                case 1:
                    StartActivity(new Intent(Activity,typeof(VideoSetupActivity)));
                    break;
                case 2:
                    StartActivity(new Intent(Activity,typeof(GeneralSetupActivity)));
                    break;
                case 3:
                    ((HomeActivity)Activity).ShowSpeakerValues();
                    break;
                case 4:
                    ((HomeActivity)Activity).ShowSlpDialog();
                    break;
            }
        }

        private void SetOptionsAvailable(bool a)
        {
            nvDrawer.Menu.GetItem(0).SetEnabled(a);
            nvDrawer.Menu.GetItem(1).SetEnabled(a);
            nvDrawer.Menu.GetItem(2).SetEnabled(a);
            nvDrawer.Menu.GetItem(3).SetEnabled(a);
            nvDrawer.Menu.GetItem(4).SetEnabled(a);
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            try
            {
                var res = ISCPHelper.Parse(msg);
                switch (res[0])
                {
                    case CmdHelper.Power.Com:
                        var pwState = CmdHelper.Power.Converter(res[1]);
                        SetOptionsAvailable(pwState);
                        optionState = pwState ? 2 : 3;
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
            optionState = 1;
            SetOptionsAvailable(true);
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
            optionState = 0;
            SetOptionsAvailable(false);
        }

        protected override void OnArtResult(string deviceId, int status)
        {
            
        }
    }
}