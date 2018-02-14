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

namespace AppOnkyo.FRAGMENTS
{
    public class LevelBottomSheetFragment : BottomSheetDialogFragment
    {
        public override void SetupDialog(Dialog dialog, int style)
        {
            base.SetupDialog(dialog, style);
            View layout = View.Inflate(Application.Context, Resource.Layout.layout_level, null);
            dialog.SetContentView(layout);
            
        }
    }
}