namespace AppOnkyo.SERIAL
{
    public class DeviceServiceResult
    {
        public string deviceId;
        public sbyte id;
        public string extra;
        public Constants.ServiceDisconnectReason? discReason = null;

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}