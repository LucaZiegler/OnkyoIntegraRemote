using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.DATASET;

namespace AppOnkyo.ADAPTER
{
    public class DeviceScanAdapter : RecyclerAdapter
    {
        private List<ScanDevice> liMain = new List<ScanDevice>();
        private int lastItemExp = -1;

        public DeviceScanAdapter(RecyclerView rvMain)
        {
            this.rvMain = rvMain;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            FrameLayout itemView = (FrameLayout) LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_discover, parent, false);
            itemView.Click += VItem_Click;

            itemView.RemoveViewAt(2);
            return new ReceiverScanHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int i)
        {
            ReceiverScanHolder vh = holder as ReceiverScanHolder;
            var scanDevice = liMain[i];
            vh.tvTitle1.Text = scanDevice.title1;
            if (string.IsNullOrEmpty(scanDevice.title2))
            {
                vh.tvTitle2.Visibility = ViewStates.Gone;
            }
            else
            {
                vh.tvTitle2.Text = scanDevice.title2;
            }
            vh.vItem.Tag = i;
        }

        public List<ScanDevice> GetAllItems()
        {
            return liMain;
        }

        public override int ItemCount => liMain.Count;

        public class ReceiverScanHolder : RecyclerView.ViewHolder
        {
            public TextView tvTitle1 { get; set; }
            public TextView tvTitle2 { get; set; }
            public View vItem { get; set; }

            public ReceiverScanHolder(View itemView) : base(itemView)
            {
                vItem = itemView;
                tvTitle1 = itemView.FindViewById<TextView>(Resource.Id.tvTitle1);
                tvTitle2 = itemView.FindViewById<TextView>(Resource.Id.tvTitle2);
            }
        }

        public void AddItem(ScanDevice sd)
        {
            var pIns = liMain.Count;
            liMain.Add(sd);
            NotifyItemInserted(pIns);
        }

        public ScanDevice GetItem(int i)
        {
            return liMain[i];
        }

        public void UpdateItem(int i, ScanDevice sd)
        {
            if (sd.status != Constants.DEVICE_STAT_NONE)
            {
                if (i != lastItemExp && lastItemExp >= 0)
                {
                    var a = liMain[lastItemExp];
                    a.status = Constants.DEVICE_STAT_NONE;
                    liMain[lastItemExp] = a;
                    NotifyItemChanged(lastItemExp);
                }
                lastItemExp = i;
            }
            liMain[i] = sd;
            NotifyItemChanged(i);
        }

        public void DeleteItem(int i)
        {
            liMain.RemoveAt(i);
            NotifyItemRemoved(i);
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