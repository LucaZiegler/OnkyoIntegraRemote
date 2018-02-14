using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.OBJECTS;
using AppOnkyo.SERVICE;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace AppOnkyo.Activities.Control.Setup
{
    [Activity(Label = "GeneralSetupActivity", Theme = "@style/BrinThemeTranparent")]
    public class GeneralSetupActivity : BrinActivity
    {
        [InjectView(Resource.Id.nvMain)] NavigationView nvMain;
        private List<SetupOption> liMain = new List<SetupOption>();
        private ViewHelper.ServiceMsgListener cbService;

        public GeneralSetupActivity()
        {
            liMain = new List<SetupOption>
            {
                new SetupOption
                {
                    Title = "Display Mode",
                    Icon = Resource.Drawable.engineering_100_black,
                    Cmd = "DIF",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "Selector & Volume Display"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Display Digital Format (Temporary)"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "Display Video Format (Temporary)"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "Dimmer Level",
                    Icon = Resource.Drawable.engineering_100_black,
                    Cmd = "DIM",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "Bright"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "01",
                            Title = "Dim"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Dark"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "HDMI CEC",
                    Icon = Resource.Drawable.hdmi_100_black,
                    Cmd = "CEC",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "Off"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "01",
                            Title = "On (Recommended)"
                        }
                    }
                },
            };
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_setup);

            Inject();

            nvMain.NavigationItemSelected += OnNavItemSelected;
            ViewHelper.InflateSetupOptions(liMain, nvMain);

            int intentOption = Intent.GetIntExtra(Constants.INTENT_HDMI_CEC_SETUP, -1);
            if (intentOption >= 0)
            {
                nvMain.PostDelayed((() =>
                {
                    OnOptionSelected(intentOption);
                }), 200);
            }

            OpenBroadcast();
        }

        [Java.Interop.Export("OnFinish")]
        public void OnFinish(View v)
        {
            Finish();
        }

        private void OnNavItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs nisea)
        {
            OnOptionSelected(nisea.MenuItem.Order);
        }

        private void OnOptionSelected(int ind)
        {
            var option = liMain[ind];
            cbService = ViewHelper.ShowSetupOptionDialog(cbService, this, option);
            DeviceService.SendCommand($"{option.Cmd}QSTN");
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            cbService?.Invoke(msg);
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
            Finish();
        }

        protected override void OnServiceConnected(string deviceId)
        {
        }

        protected override void OnServiceConnecting(string deviceId)
        {
        }

        protected override void OnArtResult(string deviceId, int status)
        {
        }
    }
}