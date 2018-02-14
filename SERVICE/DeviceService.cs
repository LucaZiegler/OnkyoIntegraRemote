using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using AppOnkyo.SERIAL;
using static AppOnkyo.Constants;
using Exception = System.Exception;

namespace AppOnkyo.SERVICE
{
    [Service(Label = "DeviceService", Enabled = true, Exported = true)]
    public class DeviceService : Service
    {
        public delegate void SimpleListener();

        public SimpleListener SimpleEvent;

        public const string TAG = "DEVICE_SERVICE_LOG";
        public const int notifyIdConnecting = 1, notifyIdConnected = 2, notifyIdPlayer = 3;
        public static DeviceServiceParameter _dsp;
        private static TcpClient tcpClient;
        public static bool isServiceRunning;
        public static bool isServiceConnecting;
        public static bool isServiceConnected;

        public delegate void ServiceStatusListener(sbyte status);

        public static ServiceStatusListener OnServiceStatusChangedEvent;
        private static bool isNotifyPlayerActive;
        private CmdHelper.NetTitleName netTitleName;
        private CmdHelper.NetArtistName netArtistName;
        private CmdHelper.NetAlbumName netAlbumName;
        private CmdHelper.Input cmdInputState;
        private static CmdHelper.Power cmdPowerStatus;
        private static CmdHelper.NetArt netArt;
        private static CmdHelper.NetStatus netStatus;
        private static ServiceDisconnectReason? serviceDisconnectReason = null;

        public DeviceService()
        {
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
            {
                serviceDisconnectReason = ServiceDisconnectReason.DISC_INTERNAL_ERR;
                StopService(false);
                Console.WriteLine(args.ExceptionObject);
                Toast.MakeText(Application.Context, $"APP_ERROR: {args.ExceptionObject}", ToastLength.Long).Show();
            };
        }

        public static CmdHelper.Power CmdPowerStatus => cmdPowerStatus;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            ShowNotificationConnected(false);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            isServiceRunning = true;
            OnServiceStatusChangedEvent += OnServiceStatusChanged;
            _dsp = IntentHelper.GetDeviceServiceParameter(intent);
            serviceDisconnectReason = null;
            //Toast.MakeText(Application.Context, "START", ToastLength.Short).Show();
            if (_dsp == null)
            {
                serviceDisconnectReason = ServiceDisconnectReason.DISC_INTERNAL_ERR;
                StopService(false);
            }
            else
            {
                netArt = new CmdHelper.NetArt();
                netArt.OnArtStatusChanged = OnArtStatusChanged;
                BrinNotfiListener.OnReceived = OnReceived;
                OpenConnection();
            }
            return StartCommandResult.RedeliverIntent;
        }

        private void OnReceived(string key, int r)
        {
            switch (key)
            {
                case BROAD_NOTIFY_PLAYER:
                    switch (r)
                    {
                        case 0:
                            SendCommand(CmdHelper.NetOperation.Set(CmdHelper.NetOperation.TRACK_DOWN));
                            break;
                        case 1:
                            SendCommand(CmdHelper.NetOperation.Set(CmdHelper.NetOperation.STOP));
                            break;
                        case 2:
                            SendCommand(CmdHelper.NetOperation.Set(CmdHelper.NetOperation.PAUSE));
                            break;
                        case 3:
                            SendCommand(CmdHelper.NetOperation.Set(CmdHelper.NetOperation.TRACK_UP));
                            break;
                    }
                    break;
                case BROAD_NOTIFY_CONNECTION:
                    switch (r)
                    {
                        case 0:
                            SendCommand(CmdHelper.Power.Set(!cmdPowerStatus.PowerState));
                            break;
                        case 1:
                            SendCommand(CmdHelper.Mute.Toggle);
                            break;
                        case 2:
                            SendCommand(CmdHelper.Volume.Down);
                            break;
                        case 3:
                            SendCommand(CmdHelper.Volume.Up);
                            break;
                        case 4:
                            serviceDisconnectReason = ServiceDisconnectReason.DISC_USER;
                            StopService(true);
                            break;
                    }
                    break;
            }
        }

        private void OnArtStatusChanged(int status, Bitmap bm)
        {
            try
            {
                UpdateNowPlayingNotification();
                SendServiceBc(new DeviceServiceResult
                {
                    id = SERVICE_STAT_ART,
                    extra = status.ToString()
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnServiceStatusChanged(sbyte status)
        {
            var dsr = new DeviceServiceResult
            {
                id = status
            };
            switch (status)
            {
                case SERVICE_STAT_CONNECTING:
                    ShowNotificationConnecting(true);
                    break;
                case SERVICE_STAT_CONNECTED:
                    ShowNotificationConnecting(false);
                    ShowNotificationConnected(true);
                    break;
                case SERVICE_STAT_DISCONNECTED:
                    ShowNotificationConnecting(false);
                    ShowNotificationConnected(false);
                    if (serviceDisconnectReason != null) dsr.discReason = serviceDisconnectReason.Value;
                    break;
            }
            SendServiceBc(dsr);
        }

        private NotificationManager GetNotifyManager()
        {
            return (NotificationManager) ApplicationContext.GetSystemService(NotificationService);
        }

        private void ShowNotificationConnected(bool show)
        {
            var nm = GetNotifyManager();

            if (show)
            {
                NotificationCompat.Builder nb = new NotificationCompat.Builder(ApplicationContext);
                var remoteView = new RemoteViews(PackageName, Resource.Layout.layout_notify_connected);

                remoteView.SetTextViewText(Resource.Id.tvNotifyTitle, GetString(Resource.String.app_name));
                remoteView.SetTextViewText(Resource.Id.tvNotifyCnctdWith, $"Connected with {_dsp.device.Model}");


                Intent itAction = new Intent(BROAD_NOTIFY_CONNECTION);
                itAction.PutExtra("ACT", 0);
                PendingIntent piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                remoteView.SetOnClickPendingIntent(Resource.Id.ibNotifiAction1, piAction);

                itAction = new Intent(BROAD_NOTIFY_CONNECTION);
                itAction.PutExtra("ACT", 1);
                piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                remoteView.SetOnClickPendingIntent(Resource.Id.ibNotifiAction2, piAction);

                itAction = new Intent(BROAD_NOTIFY_CONNECTION);
                itAction.PutExtra("ACT", 2);
                piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                remoteView.SetOnClickPendingIntent(Resource.Id.ibNotifiAction3, piAction);

                itAction = new Intent(BROAD_NOTIFY_CONNECTION);
                itAction.PutExtra("ACT", 3);
                piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                remoteView.SetOnClickPendingIntent(Resource.Id.ibNotifiAction4, piAction);

                Intent intent = new Intent(this, typeof(HomeActivity));
                intent.PutExtra(INTENT_HOME_TAB, 0);
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.SingleTop);
                PendingIntent pendingIntent = PendingIntent.GetActivity(this, IntentHelper.GetId(), intent, 0);
                nb.SetContentIntent(pendingIntent);

                nb.SetSmallIcon(Resource.Drawable.av_receiver_100_black);
                nb.SetOngoing(true);
                nb.SetCustomBigContentView(remoteView);
                var x = remoteView.Clone();
                x.SetViewVisibility(Resource.Id.llNotifyExp, ViewStates.Gone);
                nb.SetCustomContentView(x);


                nm.Notify(notifyIdConnected, nb.Build());
            }
            else
            {
                nm.CancelAll();
            }
        }

        private void ShowNotificationConnecting(bool show)
        {
            NotificationManager nm =
                (NotificationManager) ApplicationContext.GetSystemService(NotificationService);

            if (show)
            {
                NotificationCompat.Builder nb = new NotificationCompat.Builder(ApplicationContext);

                Intent intent = new Intent(this, typeof(HomeActivity));
                intent.PutExtra(INTENT_HOME_TAB, 2);
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.SingleTop);
                PendingIntent pendingIntent = PendingIntent.GetActivity(this, IntentHelper.GetId(), intent, 0);
                nb.SetContentIntent(pendingIntent);

                nb.SetSmallIcon(Resource.Drawable.av_receiver_100_black);
                nb.SetContentTitle("Onkyo Remote");
                nb.SetContentText("Connecting...");
                nb.SetProgress(0, 0, true);
                nb.SetOngoing(true);

                nm.Notify(notifyIdConnecting, nb.Build());
            }
            else
            {
                nm.Cancel(notifyIdConnecting);
            }
        }

        public static void SendCommand(string msg)
        {
            Log.Debug(TAG, "SendCommand: " + msg);
            try
            {
                if (string.IsNullOrEmpty(msg))
                    return;

                if (!isServiceRunning || isServiceConnected)
                {
                }
                var bts = ISCPHelper.Generate(msg, "1");


                var stream = tcpClient.GetStream();

                stream.Write(bts, 0, bts.Length);
                stream.Flush();
                Log.Debug(TAG, "CommandSent: " + msg);
            }
            catch (Exception e)
            {
                Log.Debug(TAG, "Error Sent: " + e.Message);
                Console.WriteLine(e);
                serviceDisconnectReason = ServiceDisconnectReason.DISC_INTERNAL_ERR;
                StopService(false);
            }
        }

        public static void StopService(bool byUser)
        {
            if (byUser && serviceDisconnectReason == null)
                serviceDisconnectReason = ServiceDisconnectReason.DISC_USER;

            Log.Debug(TAG, "StopService");
            CloseConnection();
            if (isServiceRunning)
            {
                var c = Application.Context;
                c.StopService(new Intent(c, typeof(DeviceService)));
            }
        }

        private void OpenConnection()
        {
            isServiceConnecting = true;
            SendServiceBc(new DeviceServiceResult
            {
                id = SERVICE_STAT_CONNECTING
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                }
            });
            Task.Run(async () =>
            {
                try
                {
                    Log.Debug(TAG, "Connecting");
                    OnServiceStatusChangedEvent?.Invoke(SERVICE_STAT_CONNECTING);
                    tcpClient = new TcpClient();
                    var result = tcpClient.BeginConnect(_dsp.device.IpAddress, _dsp.device.Port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                    if (!success)
                        throw new SocketException();
                    if (!tcpClient.Connected)
                        throw new SocketException();

                    //tcpClient.SendTimeout = 5000;
                    tcpClient.NoDelay = true;
                    tcpClient.Client.NoDelay = true;
                    //tcpClient.ReceiveTimeout = 5000;
                    StreamReader sr = new StreamReader(tcpClient.GetStream());
                    isServiceConnecting = false;
                    isServiceConnected = true;
                    OnServiceStatusChangedEvent?.Invoke(SERVICE_STAT_CONNECTED);
                    SendServiceBc(new DeviceServiceResult
                    {
                        id = SERVICE_STAT_CONNECTED
                    });
                    Log.Debug(TAG, "Connected");
                    string str;
                    OnConnected();
                    while (tcpClient.Connected)
                    {
                        Log.Debug(TAG, "Read");
                        str = sr.ReadLine()?.Trim();
                        //tcpClient.ReceiveTimeout = 0;
                        OnSocketMsgReceived(str);
                        Log.Debug(TAG, "Readed: " + str);
                    }
                    Log.Debug(TAG, "END");
                }
                catch (SocketException)
                {
                    if (serviceDisconnectReason == null)
                        serviceDisconnectReason = ServiceDisconnectReason.DISC_CONNECT_ERR;
                }
                catch (Exception e)
                {
                    Log.Debug(TAG, "Exception: " + e.Message);
                    if (serviceDisconnectReason == null)
                        serviceDisconnectReason = ServiceDisconnectReason.DISC_BROKEN;
                }
                finally
                {
                    if (tcpClient?.Connected == true)
                        tcpClient.Close();
                    tcpClient?.Dispose();
                    Log.Debug(TAG, "Finally");
                    isServiceConnecting = false;
                    isServiceConnected = false;
                    OnServiceStatusChangedEvent?.Invoke(SERVICE_STAT_DISCONNECTED);
                    StopService(false);
                }
            });
        }

        private void OnConnected()
        {
            SendCommand(CmdHelper.NetStatus.Request);
        }

        public override void OnDestroy()
        {
            //Toast.MakeText(Application.Context, "STOP", ToastLength.Short).Show();
            base.OnDestroy();
            if (!serviceDisconnectReason.HasValue)
            {
                Toast.MakeText(Application.Context, "KILLED", ToastLength.Long).Show();
            }
            try
            {
                OnServiceStatusChangedEvent?.Invoke(SERVICE_STAT_DISCONNECTED);
                Log.Debug(TAG, "OnDestroy");
                CloseConnection();
                isServiceRunning = false;
            }
            catch (Exception e)
            {
                Toast.MakeText(Application.Context, $"Error {e.Message}\n{e.StackTrace}", ToastLength.Long).Show();
                Console.WriteLine(e);
            }
        }

        private static void CloseConnection()
        {
            Log.Debug(TAG, "CloseConnection");
            try
            {
                if (tcpClient?.Connected == true)
                {
                    tcpClient.Close();
                }
            }
            catch (Exception e)
            {
            }
        }

        public static Bitmap GetNetArt()
        {
            return netArt.bmArt;
        }

        private void OnSocketMsgReceived(string str)
        {
            try
            {
                var res = ISCPHelper.Parse(str);
                switch (res[0])
                {
                    case CmdHelper.Input.Com:
                        cmdInputState = new CmdHelper.Input(res[1]);
                        if (cmdInputState?.Parameter == "2E")
                        {
                            SendCommand(CmdHelper.NetTitleName.Request);
                            SendCommand(CmdHelper.NetArtistName.Request);
                            SendCommand(CmdHelper.NetAlbumName.Request);
                        }
                        break;
                    case CmdHelper.Power.Com:
                        cmdPowerStatus = new CmdHelper.Power(res[1]);
                        break;
                    case CmdHelper.NetStatus.Com:
                        netStatus = new CmdHelper.NetStatus(res[1]);
                        if (netStatus.StatusPlay != CmdHelper.NetStatus.Status.STOP)
                        {
                            SendCommand(CmdHelper.NetTitleName.Request);
                            SendCommand(CmdHelper.NetArtistName.Request);
                            SendCommand(CmdHelper.NetAlbumName.Request);
                            //ShowNowPlayingNotification(true);
                        }
                        else if (cmdInputState?.Parameter == "2E")
                        {
                        }
                        else
                        {
                            ShowNowPlayingNotification(false);
                            netArt.Clear();
                        }
                        break;
                    case CmdHelper.NetTitleName.Com:
                        netTitleName = new CmdHelper.NetTitleName(res[1]);
                        if ((netStatus.StatusPlay == CmdHelper.NetStatus.Status.STOP ||
                             string.IsNullOrEmpty(netTitleName.TitleName)) &&
                            cmdInputState.Parameter != "2E")
                        {
                            ShowNowPlayingNotification(false);
                        }
                        else
                        {
                            ShowNowPlayingNotification(true);
                            UpdateNowPlayingNotification();
                        }
                        break;
                    case CmdHelper.NetArtistName.Com:
                        netArtistName = new CmdHelper.NetArtistName(res[1]);
                        UpdateNowPlayingNotification();
                        break;
                    case CmdHelper.NetAlbumName.Com:
                        netAlbumName = new CmdHelper.NetAlbumName(res[1]);
                        UpdateNowPlayingNotification();
                        break;
                    case CmdHelper.NetArt.Com:
                        netArt.OnArtMsgReceived(res[1]);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            var dsr = new DeviceServiceResult
            {
                id = SERVICE_STAT_MSG,
                extra = str
            };
            SendServiceBc(dsr);
        }

        private void ShowNowPlayingNotification(bool show)
        {
            if (show == isNotifyPlayerActive)
                return;
            Application.SynchronizationContext.Post(_ =>
            {
                try
                {
                    var nm = GetNotifyManager();

                    if (show)
                    {
                        var remoteViewExp = new RemoteViews(PackageName, Resource.Layout.layout_notifi_player_exp);

                        NotificationCompat.Builder nb = new NotificationCompat.Builder(ApplicationContext);


                        Intent intent = new Intent(this, typeof(HomeActivity));
                        intent.PutExtra(INTENT_HOME_TAB, 0);
                        intent.AddFlags(ActivityFlags.ClearTop);
                        intent.AddFlags(ActivityFlags.SingleTop);
                        PendingIntent pendingIntent = PendingIntent.GetActivity(this, IntentHelper.GetId(), intent, 0);
                        nb.SetContentIntent(pendingIntent);

                        nb.SetSmallIcon(Resource.Drawable.av_receiver_100_black);
                        nb.SetCustomBigContentView(remoteViewExp);
                        var x = remoteViewExp.Clone();
                        x.SetViewVisibility(Resource.Id.llNotifyExp, ViewStates.Gone);
                        nb.SetCustomContentView(x);
                        nb.SetOngoing(true);

                        var notifi = nb.Build();
                        nm.Notify(notifyIdPlayer, notifi);
                        isNotifyPlayerActive = true;
                    }
                    else
                    {
                        nm.Cancel(notifyIdPlayer);
                        isNotifyPlayerActive = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }, null);
        }

        private void UpdateNowPlayingNotification()
        {
            Application.SynchronizationContext.Post(_ =>
            {
                try
                {
                    if (isNotifyPlayerActive)
                    {
                        var nm = GetNotifyManager();
                        var remoteViewExp = new RemoteViews(PackageName, Resource.Layout.layout_notifi_player_exp);

                        remoteViewExp.SetImageViewResource(Resource.Id.ibNotifiAction3,
                            netStatus.StatusPlay == CmdHelper.NetStatus.Status.PAUSE
                                ? Resource.Drawable.play_100_black
                                : Resource.Drawable.pause_100_black);

                        Intent itAction = new Intent(BROAD_NOTIFY_PLAYER);
                        itAction.PutExtra("ACT", 0);
                        PendingIntent piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                        remoteViewExp.SetOnClickPendingIntent(Resource.Id.ibNotifiAction1, piAction);

                        itAction = new Intent(BROAD_NOTIFY_PLAYER);
                        itAction.PutExtra("ACT", 1);
                        piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                        remoteViewExp.SetOnClickPendingIntent(Resource.Id.ibNotifiAction2, piAction);

                        itAction = new Intent(BROAD_NOTIFY_PLAYER);
                        itAction.PutExtra("ACT", 2);
                        piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                        remoteViewExp.SetOnClickPendingIntent(Resource.Id.ibNotifiAction3, piAction);

                        itAction = new Intent(BROAD_NOTIFY_PLAYER);
                        itAction.PutExtra("ACT", 3);
                        piAction = PendingIntent.GetBroadcast(this, IntentHelper.GetId(), itAction, 0);
                        remoteViewExp.SetOnClickPendingIntent(Resource.Id.ibNotifiAction4, piAction);

                        var titleName = netTitleName?.TitleName;
                        var artistName = netArtistName?.ArtistName;

                        if (cmdInputState?.Parameter == "2E")
                        {
                            artistName = netTitleName?.TitleName;
                            titleName = "Bluetooth";
                        }

                        if (titleName != null)
                        {
                            remoteViewExp.SetTextViewText(Resource.Id.tvNotifiTitle1, titleName);
                            remoteViewExp.SetViewVisibility(Resource.Id.tvNotifiTitle1, ViewStates.Visible);
                        }

                        if (artistName != null)
                        {
                            remoteViewExp.SetTextViewText(Resource.Id.tvNotifiTitle2, artistName);
                            remoteViewExp.SetViewVisibility(Resource.Id.tvNotifiTitle2, ViewStates.Visible);
                        }

                        if (netAlbumName?.AlbumName != null)
                        {
                            remoteViewExp.SetTextViewText(Resource.Id.tvNotifiTitle3, netAlbumName.AlbumName);
                            remoteViewExp.SetViewVisibility(Resource.Id.tvNotifiTitle3, ViewStates.Visible);
                        }

                        if (netArt?.bmArt != null)
                        {
                            remoteViewExp.SetImageViewBitmap(Resource.Id.ivNotifiCover, netArt.bmArt);
                            remoteViewExp.SetViewVisibility(Resource.Id.ivNotifiCover, ViewStates.Visible);
                        }
                        else
                        {
                            remoteViewExp.SetImageViewResource(Resource.Id.ivNotifiCover, Resource.Drawable.app_icon);
                            remoteViewExp.SetViewVisibility(Resource.Id.ivNotifiCover, ViewStates.Visible);
                        }

                        NotificationCompat.Builder nb = new NotificationCompat.Builder(ApplicationContext);


                        Intent intent = new Intent(this, typeof(HomeActivity));
                        intent.PutExtra(INTENT_HOME_TAB, 0);
                        intent.AddFlags(ActivityFlags.ClearTop);
                        intent.AddFlags(ActivityFlags.SingleTop);
                        PendingIntent pendingIntent = PendingIntent.GetActivity(this, IntentHelper.GetId(), intent, 0);
                        nb.SetContentIntent(pendingIntent);

                        nb.SetCustomBigContentView(remoteViewExp);
                        var x = remoteViewExp.Clone();
                        x.SetViewVisibility(Resource.Id.llNotifyExp, ViewStates.Gone);
                        nb.SetCustomContentView(x);

                        nb.SetSmallIcon(Resource.Drawable.av_receiver_100_black);
                        nb.SetOngoing(true);


                        nm.Notify(notifyIdPlayer, nb.Build());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }, null);
        }

        private void SendServiceBc(DeviceServiceResult dsr)
        {
            if (dsr == null)
                return;
            Log.Debug(TAG, "SendServiceBc");
            dsr.deviceId = _dsp.device.Identifier;
            var intent = new Intent(SERVICE_ID);
            intent.PutExtra(IntentHelper.INTENT_DSR_VAL, dsr.ToString());
            Application.SynchronizationContext.Post(_ =>
            {
                LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);
                Log.Debug(TAG, "ServiceBcSent");
            }, null);
        }
    }
}