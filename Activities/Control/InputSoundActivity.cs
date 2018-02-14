using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AppOnkyo.ADAPTER;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using AppOnkyo.ISCP;
using AppOnkyo.SERVICE;

namespace AppOnkyo.Activities.Control
{
    [Activity(Label = "InputSoundActivity", Theme = "@style/BrinThemeTranparent")]
    public class InputSoundActivity : BrinActivity
    {
        [InjectView(Resource.Id.vfMain)] ViewFlipper vfMain;
        [InjectView(Resource.Id.rv1)] RecyclerView rvInput;
        [InjectView(Resource.Id.rv2)] RecyclerView rvSound;
        [InjectView(Resource.Id.tlMain)] TabLayout tlMain;


        private RecyclerView.LayoutManager rvInputManager;
        private RecyclerView.LayoutManager rvSoundManager;
        private CheckableListAdapter rvInputAdapter;
        private CheckableListAdapter rvSoundAdapter;

        protected override void OnCreate(Bundle sis)
        {
            base.OnCreate(sis);
            SetContentView(Resource.Layout.layout_input_sound);
            Inject();

            OpenBroadcast();

            tlMain.AddTab(tlMain.NewTab().SetText("Input").SetTag(0));
            tlMain.AddTab(tlMain.NewTab().SetText("Sound").SetTag(1));

            tlMain.TabSelected += delegate(object sender, TabLayout.TabSelectedEventArgs args)
            {
                vfMain.DisplayedChild = Convert.ToInt32(args.Tab.Tag);
            };

            var indIntent = Intent.GetIntExtra("TAB_IND", 0);
            tlMain.GetTabAt(indIntent).Select();

            rvInputAdapter = new CheckableListAdapter();
            rvInputAdapter.OnItemClicked += OnInputSelected;
            rvInputManager = new LinearLayoutManager(Application.Context);
            rvInput.SetLayoutManager(rvInputManager);
            rvInput.SetAdapter(rvInputAdapter);

            rvSoundAdapter = new CheckableListAdapter();
            rvSoundAdapter.OnItemClicked += OnSoundSelected;
            rvSoundManager = new LinearLayoutManager(Application.Context);
            rvSound.SetLayoutManager(rvSoundManager);
            rvSound.SetAdapter(rvSoundAdapter);

            List<CheckableListAdapter.CheckableListItem> li = new List<CheckableListAdapter.CheckableListItem>();
            foreach (CmdHelper.Input input in CmdHelper.Input.Inputs)
            {
                li.Add(new CheckableListAdapter.CheckableListItem {isChecked = false, title = input.Name});
            }
            rvInputAdapter.AddAll(li);

            li.Clear();
            foreach (CmdHelper.ListeningMode listeningMode in CmdHelper.ListeningMode.ListeningModes)
            {
                li.Add(new CheckableListAdapter.CheckableListItem {isChecked = false, title = listeningMode.Name});
            }
            rvSoundAdapter.AddAll(li);
        }

        [Java.Interop.Export("OnFinish")]
        public void OnFinish(View v)
        {
            Finish();
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
            try
            {
                var res = ISCPHelper.Parse(msg);
                switch (res[0])
                {
                    case CmdHelper.Input.Com:
                        var indInp = CmdHelper.Input.ConverterToIndex(res[1]);
                        rvInputAdapter.SetItemChecked(indInp);
                        rvInput.SmoothScrollToPosition(indInp);
                        break;
                    case CmdHelper.ListeningMode.Com:
                        var indSou = CmdHelper.ListeningMode.ConverterToIndex(res[1]);
                        rvSoundAdapter.SetItemChecked(indSou);
                        rvSound.SmoothScrollToPosition(indSou);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
            Finish();
        }

        protected override void OnServiceConnected(string deviceId)
        {
        }

        protected override void OnServiceConnecting(string deviceId)
        {
            Finish();
        }

        protected override void OnArtResult(string deviceId, int status)
        {
        }

        private void OnSoundSelected(View v, int ind)
        {
            DeviceService.SendCommand(
                CmdHelper.ListeningMode.Set(CmdHelper.ListeningMode.ListeningModes[ind].Parameter));
        }

        private void OnInputSelected(View v, int ind)
        {
            DeviceService.SendCommand(CmdHelper.Input.Set(CmdHelper.Input.Inputs[ind].Parameter));
        }

        public override void OnBroadcastOpen()
        {
            DeviceService.SendCommand(CmdHelper.Input.Request);
            DeviceService.SendCommand(CmdHelper.ListeningMode.Request);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CloseBroadcast();
        }
    }
}