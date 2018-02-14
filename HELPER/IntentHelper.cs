using System;
using Android.Content;
using AppOnkyo.SERIAL;

namespace AppOnkyo.HELPER
{
    public class IntentHelper
    {
        public const string INTENT_DSP_VAL = "INT_DSP";
        public const string INTENT_DSR_VAL = "INT_DSR";

        public static DeviceServiceParameter GetDeviceServiceParameter(Intent intent)
        {
            try
            {
                string v = intent.GetStringExtra(INTENT_DSP_VAL);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceServiceParameter>(v);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static DeviceServiceResult GetDeviceServiceResult(Intent intent)
        {
            try
            {
                string v = intent.GetStringExtra(INTENT_DSR_VAL);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceServiceResult>(v);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static int GetId()
        {
            return DateTime.Now.Millisecond & 0xfffffff;
        }
    }
}