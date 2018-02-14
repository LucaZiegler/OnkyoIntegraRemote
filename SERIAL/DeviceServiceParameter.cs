using AppOnkyo.DATASET;
using AppOnkyo.ISCP;

namespace AppOnkyo.SERIAL
{
    public class DeviceServiceParameter
    {
        public Discover.DeviceInfo device;
        public byte conFlag;
        public long id;

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}