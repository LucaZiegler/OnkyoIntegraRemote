namespace AppOnkyo.HELPER
{
    public class Listener
    {
        public delegate void ServiceConnectedListener(string deviceId);

        public delegate void ServiceConnectingListener(string deviceId);

        public delegate void ServiceDisconnectedListener(string deviceId);

        public delegate void ServiceMsgListener(string deviceId, string msg);
    }
}