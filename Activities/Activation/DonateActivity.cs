using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AppOnkyo.ANDR_CUSTOM;
using AppOnkyo.HELPER;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using static AppOnkyo.Constants;
using Exception = System.Exception;
using ItemType = Plugin.InAppBilling.Abstractions.ItemType;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace AppOnkyo.Activities.Activation
{
    [Activity(Label = "DonateActivity")]
    public class DonateActivity : BrinActivity
    {
        [InjectView(Resource.Id.vfMain)] ViewFlipper vfMain;
        [InjectView(Resource.Id.tvError)] TextView tvError;
        [InjectView(Resource.Id.tbMain)] Toolbar tbMain;
        private IInAppBilling bpMain;
        private Preferences prefs = new Preferences();

        private const int CHILD_WAIT = 0, CHILD_ERR = 1, CHILD_BUY = 2, CHILD_BOUGHT = 3;

        protected override void OnCreate(Bundle b)
        {
            base.OnCreate(b);
            SetContentView(Resource.Layout.layout_donate);
            Inject();

            SetSupportActionBar(tbMain);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            tbMain.NavigationClick += delegate { OnBackPressed(); };

            Connect();
        }

        private async void Connect()
        {
            try
            {
                if (!CrossInAppBilling.IsSupported)
                {
                    throw new Exception("The in app billing service is not available on your device");
                }
                bpMain = CrossInAppBilling.Current;
                //bpMain.InTestingMode = true;
                var connected = await bpMain.ConnectAsync();
                if (connected)
                {
                    OnBillingInitialized();
                }
                else
                {
                    OnError("Not connected");
                }
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public async void OnBillingInitialized()
        {
            var purchases = await bpMain.GetPurchasesAsync(ItemType.InAppPurchase);

            //check for null just incase
            if (purchases?.Any(p => p.ProductId == Constants.BILLING_KEY_ID) ?? false)
            {
                //Purchase restored
                SetChild(CHILD_BOUGHT);
                prefs.PutBool(PREF_KEY_BOUGHT,true);
            }
            else
            {
                //no purchases found
                SetChild(CHILD_BUY);
                prefs.PutBool(PREF_KEY_BOUGHT, false);
            }
        }


        [Java.Interop.Export("OnBuyKey")]
        public void OnBuyKey(View v)
        {
            BuyKeyAsync();
        }

        private async Task<bool> BuyKeyAsync()
        {
            try
            {
                SetChild(CHILD_WAIT);
                var purchase = await bpMain.PurchaseAsync(BILLING_KEY_ID, ItemType.InAppPurchase, BILLING_AUTH);

                //possibility that a null came through.
                if (purchase == null)
                {
                    OnError("Billing error");
                }
                else
                {
                    OnPurchaseSuccess(BILLING_KEY_ID);
                    SetChild(CHILD_BOUGHT);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
            return true;
        }

        private void OnPurchaseSuccess(string key)
        {
            prefs.PutBool(PREF_KEY_BOUGHT, true);
        }

        private void OnError(string msg)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    tvError.Text = msg;
                    SetChild(CHILD_ERR);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private void SetChild(int ind)
        {
            vfMain.DisplayedChild = ind;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            bpMain?.DisconnectAsync();
        }

        protected override void OnServiceMsg(string deviceId, string msg)
        {
        }

        protected override void OnServiceDisconnected(string deviceId, Constants.ServiceDisconnectReason sdr)
        {
        }

        protected override void OnServiceConnected(string deviceId)
        {
        }

        protected override void OnServiceConnecting(string deviceId)
        {
        }

        protected override void OnArtResult(string deviceId, int status)
        {
            throw new NotImplementedException();
        }
    }
}