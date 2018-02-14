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
using AppOnkyo.SERIAL;

namespace AppOnkyo.HELPER
{
    class DeviceHelper
    {
        public List<StoredDevice> liDevices = new List<StoredDevice>();
        
        public static DeviceHelper FromString(string json)
        {
            if (json == null)
                return null;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceHelper>(json);
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        private DeviceHelper()
        {
            // Disabled construktor
        }

        public static DeviceHelper Instance()
        {
            var deviceHelper = new DeviceHelper();

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var raw = prefs.GetString("RCR", null);
            if (raw != null)
                deviceHelper = FromString(raw);

            return deviceHelper;
        }

        public void Save()
        {
            string ser = ToString();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context).Edit();
            prefs.PutString("RCR", ser).Apply();
        }

        public void DeleteDevice(int itemInd)
        {
            liDevices.RemoveAt(itemInd);
            Save();
        }
    }
}