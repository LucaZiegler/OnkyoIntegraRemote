using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static AppOnkyo.Constants;

namespace AppOnkyo.HELPER
{
    public class Preferences
    {
        public readonly ISharedPreferences spMain;

        public Preferences()
        {
            spMain = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        }

        public void PutBool(string tag, bool val)
        {
            spMain.Edit().PutBoolean(tag, val).Apply();
        }

        public void PutString(string tag, string val)
        {
            spMain.Edit().PutString(tag, val).Apply();
        }

        public void PutInt(string tag, int val)
        {
            spMain.Edit().PutInt(tag, val).Apply();
        }

        public bool IsFirstAppStart()
        {
            bool f = spMain.GetBoolean(PREF_FIRST_START, true);
            PutBool(PREF_FIRST_START,false);
            return f;
        }
    }
}