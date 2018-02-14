using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Util;
using static AppOnkyo.Constants;
using Exception = Java.Lang.Exception;
using Timer = System.Timers.Timer;


namespace AppOnkyo.ISCP
{
    public class Discover
    {
        private const string TAG = "DISC";
        private UdpClient udpClient = null;
        private IPEndPoint udpGroup = null;
        private bool receiving = false;
        private Timer trTimeOut = null;

        public delegate void DeviceFoundListener(DeviceInfo deviceInfo);

        public delegate void ErrorListener(Exception ex);

        public delegate void StatusChangedListener(sbyte status);

        public DeviceFoundListener OnDeviceFound = null;
        public ErrorListener OnError = null;
        public StatusChangedListener OnStatusChanged = null;

        
        public void Open()
        {
            try
            {
                OnStatusChanged?.Invoke(STAT_OPENING);
                udpClient = new UdpClient(60128);
                udpGroup = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 60128);
                //udpGroup = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

                Task task = null;
                task = Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            receiving = true;
                            OnStatusChanged?.Invoke(STAT_OPEN);
                            while (receiving)
                            {
                                var bytes = udpClient.Receive(ref udpGroup);
                                var res = Encoding.ASCII.GetString(bytes);
                                if (res.StartsWith("ISCP") && !res.Contains("xECNQSTN"))
                                {
                                    DeviceInfo device = new DeviceInfo(udpGroup.Address, res);

                                    OnDeviceFound?.Invoke(device);
                                }
                                else
                                {
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            OnStatusChanged?.Invoke(STAT_ERROR);
                            OnError?.Invoke(ex);
                        }
                        receiving = false;
                        Thread.Sleep(2000);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex.ToString());
                OnStatusChanged?.Invoke(STAT_ERROR);
                OnError?.Invoke(ex);
            }
        }

        public void Send()
        {
            try
            {
                byte[] bts = ISCPHelper.Generate("ECNQSTN", "x");

                udpClient.Send(bts, bts.Length, udpGroup);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
                OnStatusChanged?.Invoke(STAT_ERROR);
            }
        }

        public void Stop()
        {
            receiving = false;
            udpClient.Close();
        }

        public class DeviceInfo
        {
            public string IpAddress;
            public string Model;
            public string Destination;
            public string Identifier;
            public int Port;

            public DeviceInfo()
            {
            }

            public DeviceInfo(IPAddress ipAddress, string raw)
            {
                IpAddress = ipAddress.ToString();

                raw = raw.Substring(raw.IndexOf("!") + 5);
                raw = raw.Substring(0, raw.IndexOf("\r\n") - 1);
                string[] ar = raw.Split('/');
                Model = ar[0];
                Port = Convert.ToInt32(ar[1]);
                Destination = ar[2];
                Identifier = ar[3];
            }
        }
    }
}