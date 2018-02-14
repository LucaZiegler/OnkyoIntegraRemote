using System;
using AppOnkyo.ISCP;

namespace AppOnkyo.DATASET
{
    public class ScanDevice
    {
        public string title1;
        public string title2;
        public sbyte status = Constants.DEVICE_STAT_NONE;
        public long id = DateTime.Now.ToFileTime();

        public Discover.DeviceInfo scanDevice;

        public PairedDevice ToPairedDevice()
        {
            return new PairedDevice
            {
                title1 = title1,
                status = Constants.DEVICE_STAT_NONE,
                id = id,
                scanDevice = scanDevice
            };
        }
    }
}