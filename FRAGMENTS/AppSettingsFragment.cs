using System;
using Android.OS;
using Android.Views;
using Fragment = Android.Support.V4.App.Fragment;

namespace AppOnkyo.FRAGMENTS
{
    class AppSettingsFragment : Fragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View layout = null;
            try
            {
                layout = inflater.Inflate(Resource.Layout.layout_app_settings, container, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return layout;
        }
    }
}