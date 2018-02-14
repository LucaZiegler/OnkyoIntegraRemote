using AppOnkyo.ISCP;

namespace AppOnkyo.SERIAL
{
    public class StoredDevice
    {
        public string title1;
        public string title2;
        public sbyte status;
        public long id;
        public byte conFlag;

        public Discover.DeviceInfo scanDevice;
    }
}