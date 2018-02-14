using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.DATASET;
using AppOnkyo.HELPER;
using AppOnkyo.SERIAL;
using static AppOnkyo.Constants;

namespace AppOnkyo.ADAPTER
{
    class DevicePairedAdapter : RecyclerAdapter
    {
        private DeviceHelper deviceHelper = DeviceHelper.Instance();
        private List<PairedDevice> liMain = new List<PairedDevice>();
        private int lastItemExp = -1;

        public DevicePairedAdapter(RecyclerView rvMain)
        {
            this.rvMain = rvMain;
            foreach (StoredDevice device in deviceHelper.liDevices.ToList())
            {
                AddItem(new PairedDevice
                {
                    scanDevice = device.scanDevice,
                    status = DEVICE_STAT_NONE,
                    title1 = device.title1,
                    conFlag = device.conFlag
                }, false);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_drawer_device, parent, false);
            itemView.Click += VItem_Click;
            return new ReceiverScanHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int i)
        {
            ReceiverScanHolder vh = holder as ReceiverScanHolder;

            //vh.vItem.Click += VItem_Click;
            //vh.ibOption.Click += OnItemOptionClick;

            var PairedDevice = liMain[i];
            if (PairedDevice.status == Constants.DEVICE_STAT_CONNECTING || PairedDevice.status == DEVICE_STAT_CONNECTED)
                vh.vItem.Visibility = ViewStates.Gone;
            vh.tvTitle1.Text = PairedDevice.title1;
            vh.vItem.Tag = i;
        }

        public override int ItemCount => liMain.Count;

        public class ReceiverScanHolder : RecyclerView.ViewHolder
        {
            public TextView tvTitle1 { get; set; }
            public View vItem { get; set; }

            public ReceiverScanHolder(View itemView) : base(itemView)
            {
                vItem = itemView;
                tvTitle1 = itemView.FindViewById<TextView>(Resource.Id.tvTitle1);
            }
        }

        public int AddItem(PairedDevice sd, bool store)
        {
            if (sd.conFlag > 0)
            {
                int i = 0;
                foreach (PairedDevice device in GetAllItems().ToList())
                {
                    if (device.conFlag > 0)
                    {
                        device.conFlag = 0;
                        UpdateItem(i, device, !store);
                    }
                    i++;
                }
            }
            var pIns = liMain.Count;
            liMain.Add(sd);
            NotifyItemInserted(pIns);
            if (store)
            {
                deviceHelper.liDevices.Add(sd.ToStoredDevice());
                deviceHelper.Save();
            }
            return pIns;
        }

        public List<PairedDevice> GetAllItems()
        {
            return liMain;
        }

        public PairedDevice GetItem(int i)
        {
            if (i >= liMain.Count)
                return null;
            return liMain[i];
        }

        public void UpdateItem(int i, PairedDevice sd, bool store)
        {
            try
            {
                if (sd.conFlag > 0)
                {
                    int c = 0;
                    foreach (PairedDevice device in GetAllItems().ToList())
                    {
                        if (i != c && device.conFlag > 0)
                        {
                            device.conFlag = 0;
                            UpdateItem(c, device, false);
                        }
                        c++;
                    }
                }
                if (sd.status != DEVICE_STAT_NONE)
                {
                    if (i != lastItemExp && lastItemExp >= 0)
                    {
                        var a = liMain[lastItemExp];
                        a.status = DEVICE_STAT_NONE;
                        liMain[lastItemExp] = a;
                        NotifyItemChanged(lastItemExp);
                    }
                    lastItemExp = i;
                }
                liMain[i] = sd;
                if (store)
                {
                    deviceHelper.liDevices[i] = sd.ToStoredDevice();
                    deviceHelper.Save();
                }
                NotifyItemChanged(i);
            }
            catch (Exception e)
            {
                Toast.MakeText(Application.Context, $"Error: {e.Message}", ToastLength.Short).Show();
            }
        }

        public void DeleteItem(int i, bool store)
        {
            liMain.RemoveAt(i);

            if (store)
            {
                deviceHelper.liDevices.RemoveAt(i);
                deviceHelper.Save();
            }
            NotifyItemRemoved(i);
            //NotifyDataSetChanged();
        }

        public override void Clear()
        {
            int c = liMain.Count;
            if (c > 0)
            {
                liMain.Clear();
                NotifyItemRangeRemoved(0, c);
            }
        }
    }
}