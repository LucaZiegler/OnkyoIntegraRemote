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
    [Activity(Label = "AudioSetupActivity", Theme = "@style/BrinThemeTranparent")]
    public class AudioSetupActivity : BrinActivity
    {
        [InjectView(Resource.Id.nvMain)] NavigationView nvMain;
        private List<SetupOption> liMain = new List<SetupOption>();
        private ViewHelper.ServiceMsgListener cbService;

        public AudioSetupActivity()
        {
            liMain = new List<SetupOption>
            {
                new SetupOption
                {
                    Title = "Phase Matching Bass",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "PMB",
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
                            Title = "On"
                        },
                    }
                },
                new SetupOption
                {
                    Title = "Audio Selector",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "SLA",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "Auto"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "01",
                            Title = "Multi-Channel"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Analog"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "iLINK"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "04",
                            Title = "HDMI"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "05",
                            Title = "COAX/OPT"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "06",
                            Title = "Balance"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "07",
                            Title = "ARC"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "Late Night",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "LTN",
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
                            Title = "Low"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "High"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "Auto"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "Cinema Filter",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "RAS",
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
                            Title = "On"
                        },
                    }
                },
                new SetupOption
                {
                    Title = "Audyssey 2EQ/MultEQ/MultEQ XT",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "ADY",
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
                            Title = "Movie"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Music"
                        },
                    }
                },
                new SetupOption
                {
                    Title = "Audyssey Dynamic EQ",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "ADQ",
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
                            Title = "On"
                        },
                    }
                },
                new SetupOption
                {
                    Title = "Audyssey Dynamic Volume",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "ADV",
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
                            Title = "Light"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Medium"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "Heavy"
                        },
                    }
                },
                new SetupOption
                {
                    Title = "Dolby Volume",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "DVL",
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
                            Title = "On"
                        },
                    }
                },
                new SetupOption
                {
                    Title = "Music Optimizer",
                    Icon = Resource.Drawable.audio_100_black,
                    Cmd = "MOT",
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
                            Title = "On"
                        },
                    }
                },
            };
        }

        protected override void OnCreate(Bundle b)
        {
            base.OnCreate(b);
            SetContentView(Resource.Layout.layout_setup);

            Inject();
            

            nvMain.NavigationItemSelected += OnNavItemSelected;
            ViewHelper.InflateSetupOptions(liMain,nvMain);

            OpenBroadcast();
        }

        [Java.Interop.Export("OnFinish")]
        public void OnFinish(View v)
        {
            Finish();
        }

        private void OnNavItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs nisea)
        {
            var option = liMain[nisea.MenuItem.Order];
            cbService = ViewHelper.ShowSetupOptionDialog(cbService, this,option);
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