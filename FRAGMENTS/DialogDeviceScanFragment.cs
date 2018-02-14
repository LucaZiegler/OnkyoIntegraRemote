using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.ADAPTER;
using AppOnkyo.DATASET;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using static AppOnkyo.Constants;

namespace AppOnkyo.FRAGMENTS
{
    public class DialogDeviceScanFragment : DialogFragment
    {
        private RecyclerView rvMain;
        private ViewFlipper vfMain;
        private SwipeRefreshLayout srMain;

        private DeviceScanAdapter rvAdapter;
        private RecyclerView.LayoutManager rvManager;

        private Discover deviceDiscoverHelper = null;

        public delegate void DeviceScanResultListener(PairedDevice pd);

        public static bool isVisible = false;

        public DeviceScanResultListener OnDeviceScanDialogResult;

        public DialogDeviceScanFragment(DeviceScanResultListener list)
        {
            isVisible = true;
            OnDeviceScanDialogResult = list;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            Dialog dialog = new Dialog(Activity);
            dialog.SetContentView(Resource.Layout.layout_dialog_device_scan);

            rvMain = dialog.FindViewById<RecyclerView>(Resource.Id.rvMain);
            vfMain = dialog.FindViewById<ViewFlipper>(Resource.Id.vfMain);
            srMain = dialog.FindViewById<SwipeRefreshLayout>(Resource.Id.srMain);

            rvAdapter = new DeviceScanAdapter(rvMain);
            rvAdapter.OnItemClicked += OnItemSelected;
            rvManager = new LinearLayoutManager(Application.Context);
            rvMain.SetLayoutManager(rvManager);
            rvMain.SetAdapter(rvAdapter);

            srMain.Refresh += OnRefresh;

            deviceDiscoverHelper = new Discover();
            deviceDiscoverHelper.OnStatusChanged += OnScanStatusChanged;
            deviceDiscoverHelper.OnDeviceFound += OnScanDeviceFound;
            deviceDiscoverHelper.Open();

            vfMain.PostDelayed(() =>
            {
                if (vfMain.DisplayedChild == 0)
                {
                    vfMain.DisplayedChild = 1;
                }
            }, 5000);

            return dialog;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            isVisible = false;
        }

        private void OnRefresh(object sender, EventArgs eventArgs)
        {
            Scan();
            srMain.Refreshing = false;
        }

        private void OnItemSelected(View v, int ind)
        {
            var a = rvAdapter.GetItem(ind);
            var b = a.ToPairedDevice();
            OnDeviceScanDialogResult?.Invoke(b);
            Dismiss();
        }

        private void OnScanDeviceFound(Discover.DeviceInfo deviceinfo)
        {
            Application.SynchronizationContext.Post(_ =>
            {
                try
                {

                    var sd = new ScanDevice();
                    sd.id = ViewHelper.CurrentTimeMillis();
                    sd.title1 = deviceinfo.Model;
                    sd.title2 = deviceinfo.IpAddress;
                    sd.scanDevice = deviceinfo;

                    rvAdapter.AddItem(sd);

                    vfMain.DisplayedChild = 2;
                }
                catch (Exception e)
                {
                }
            }, null);
        }

        public void Scan()
        {
            vfMain.DisplayedChild = 0;
            rvAdapter.Clear();
            deviceDiscoverHelper.Send();
        }

        private void OnScanStatusChanged(sbyte status)
        {
            Application.SynchronizationContext.Post(_ =>
            {
                switch (status)
                {
                    case STAT_OPEN:
                        Scan();
                        break;
                    case STAT_ERROR:

                        break;
                    case STAT_OPENING:

                        break;
                }
            }, null);
        }
    }
}