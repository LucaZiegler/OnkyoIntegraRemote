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

namespace AppOnkyo.ADAPTER
{
    public class CheckableListAdapter : RecyclerView.Adapter
    {
        public delegate void ItemClickListener(View v, int ind);

        public ItemClickListener OnItemClicked;
        private List<CheckableListItem> liMain = new List<CheckableListItem>();
        private int lastItemChecked = -1;

        public class CheckableListItem
        {
            public string title;
            public bool isChecked;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_checkable, parent, false);
            itemView.Click += VItem_Click;
            return new ReceiverScanHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int i)
        {
            ReceiverScanHolder vh = holder as ReceiverScanHolder;
            var item = liMain[i];
            vh.tvTitle.Text = item.title;
            vh.ivChecked.Visibility = item.isChecked ? ViewStates.Visible : ViewStates.Gone;
            vh.vItem.Tag = i;
        }

        private void VItem_Click(object sender, EventArgs e)
        {
            var view = (View) sender;
            int ind = Convert.ToInt32(view.Tag.ToString());
            OnItemClicked?.Invoke(view, ind);
        }

        public override int ItemCount => liMain.Count;

        public class ReceiverScanHolder : RecyclerView.ViewHolder
        {
            public TextView tvTitle { get; set; }
            public ImageView ivChecked { get; set; }
            public View vItem { get; set; }

            public ReceiverScanHolder(View itemView) : base(itemView)
            {
                vItem = itemView;
                tvTitle = itemView.FindViewById<TextView>(Resource.Id.tvTitle1);
                ivChecked = itemView.FindViewById<ImageView>(Resource.Id.ivCheck);
            }
        }

        public List<CheckableListItem> GetAllItems()
        {
            return liMain;
        }

        public void AddAll(List<CheckableListItem> ad)
        {
            int from = liMain.Count;
            liMain.AddRange(ad);
            int to = ad.Count - 1;
            NotifyItemRangeInserted(from, to);
        }

        public void UpdateItem(int i, CheckableListItem sd)
        {
            liMain[i] = sd;
            NotifyItemChanged(i);
        }

        public void DeleteItem(int i)
        {
            liMain.RemoveAt(i);
            NotifyItemRemoved(i);
        }

        public void SetItemChecked(int ind)
        {
            if (ind < 0)
            {
                if (lastItemChecked >= 0)
                {
                    liMain[lastItemChecked].isChecked = false;
                    NotifyItemChanged(lastItemChecked);
                }
                return;
            }
            if (ind != lastItemChecked)
            {
                if (lastItemChecked >= 0)
                {
                    liMain[lastItemChecked].isChecked = false;
                    NotifyItemChanged(lastItemChecked);
                }

                liMain[ind].isChecked = true;
                NotifyItemChanged(ind);
            }
            lastItemChecked = ind;
        }
    }
}