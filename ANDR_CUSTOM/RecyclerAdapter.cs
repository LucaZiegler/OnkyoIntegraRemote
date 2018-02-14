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

namespace AppOnkyo.ANDR_CUSTOM
{
    public abstract class RecyclerAdapter : RecyclerView.Adapter
    {
        public bool IsEnabled = false;

        public delegate void ItemClickListener(View v, int ind);

        public ItemClickListener OnItemClicked;

        protected RecyclerView rvMain;

        public abstract void Clear();

        protected void VItem_Click(object sender, EventArgs e)
        {
            var view = (View)sender;
            int ind = rvMain.IndexOfChild(view);
            OnItemClicked?.Invoke(view, ind);
        }
    }
}