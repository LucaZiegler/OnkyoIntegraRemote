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
    [Activity(Label = "VideoSetupActivity", Theme = "@style/BrinThemeTranparent")]
    public class VideoSetupActivity : BrinActivity
    {
        [InjectView(Resource.Id.nvMain)] NavigationView nvMain;
        private List<SetupOption> liMain = new List<SetupOption>();
        private ViewHelper.ServiceMsgListener cbService;

        public VideoSetupActivity()
        {
            liMain = new List<SetupOption>
            {
                new SetupOption
                {
                    Title = "HDMI Output Selector",
                    Icon = Resource.Drawable.hdmi_100_black,
                    Cmd = "HDO",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "None"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "01",
                            Title = "HDMI Main"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "HDMI Sub"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "Both"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "04",
                            Title = "Both (Main)"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "05",
                            Title = "Both (Sub)"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "HDMI Audio Out",
                    Icon = Resource.Drawable.hdmi_100_black,
                    Cmd = "HAO",
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
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Auto"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "Monitor Out Resolution",
                    Icon = Resource.Drawable.resolution_100_black,
                    Cmd = "RES",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "Through"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "01",
                            Title = "Auto"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "480p"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "720p"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "04",
                            Title = "1080i"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "05",
                            Title = "1080p"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "07",
                            Title = "1080p/24fs"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "08",
                            Title = "4k Upscaling"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "06",
                            Title = "Source"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "Video Wide Mode",
                    Icon = Resource.Drawable.resolution_100_black,
                    Cmd = "VWM",
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
                            Title = "4:3"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Full"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "Zoom"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "04",
                            Title = "Wide Zoom"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "05",
                            Title = "Smart Zoom"
                        }
                    }
                },
                new SetupOption
                {
                    Title = "Video Picture Mode",
                    Icon = Resource.Drawable.picture_100_black,
                    Cmd = "VPM",
                    LiEntries = new[]
                    {
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "00",
                            Title = "Through"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "01",
                            Title = "Custom"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "02",
                            Title = "Cinema"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "03",
                            Title = "Game"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "05",
                            Title = "ISF Day"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "06",
                            Title = "ISF Night"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "07",
                            Title = "Streaming"
                        },
                        new SetupOption.SetupOptionEntry
                        {
                            Cmd = "08",
                            Title = "Direct"
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