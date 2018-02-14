using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AppOnkyo.HELPER;
using AppOnkyo.SERIAL;
using AppOnkyo.SERVICE;

namespace AppOnkyo.ANDR_CUSTOM
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] {"android.net.wifi.STATE_CHANGE", "android.net.conn.CONNECTIVITY_CHANGE", "android.net.wifi.WIFI_STATE_CHANGED" })]
    public class BrinWifiStateChangeReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                NetworkInfo info = (NetworkInfo)intent.GetParcelableExtra(WifiManager.ExtraNetworkInfo);
                if (info?.IsConnected == true)
                {
                    //Toast.MakeText(context,"WIFI CONNECTED",ToastLength.Short).Show();
                    if (!DeviceService.isServiceRunning)
                    {
                        DeviceHelper dh = DeviceHelper.Instance();
                        foreach (var device in dh.liDevices)
                        {
                            if (device.conFlag == 2)
                            {
                                var dsp = new DeviceServiceParameter
                                {
                                    device = device.scanDevice,
                                    conFlag = device.conFlag,
                                    id = device.id
                                };
                                Task.Run((() =>
                                {
                                    System.Threading.Thread.Sleep(2500);
                                    StartDeviceService(dsp, context);
                                }));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (DeviceService.isServiceRunning)
                    {
                        DeviceService.StopService(true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //Toast.MakeText(context,"ERROR: "+e.Message,ToastLength.Short).Show();
            }
        }

        public void StartDeviceService(DeviceServiceParameter dsp, Context context)
        {
            if (DeviceService.isServiceRunning)
                throw new Exception("Service läuft bereits");
            Intent si = new Intent(Application.Context, typeof(DeviceService));
            si.PutExtra(IntentHelper.INTENT_DSP_VAL, dsp.ToString());
            context.StartService(si);
        }
    }
}