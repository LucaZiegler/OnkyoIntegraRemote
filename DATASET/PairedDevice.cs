using System;
using AppOnkyo.ISCP;
using AppOnkyo.SERIAL;

namespace AppOnkyo.DATASET
{
    public class PairedDevice
    {
        public string title1;
        public sbyte status = Constants.DEVICE_STAT_NONE;
        public long id;
        public byte conFlag;

        public Discover.DeviceInfo scanDevice;

        public StoredDevice ToStoredDevice()
        {
            return new StoredDevice
            {
                scanDevice = scanDevice,
                status = Constants.DEVICE_STAT_NONE,
                title1 = title1,
                conFlag = conFlag
            };
        }
    }
}