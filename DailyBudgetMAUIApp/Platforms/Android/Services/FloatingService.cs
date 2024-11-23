using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DailyBudgetMAUIApp;
using DailyBudgetMAUIApp.DataServices;
using System.Globalization;
using static Android.Views.View;
using Button = Android.Widget.Button;
using Color = Android.Graphics.Color;
using Resource = Microsoft.Maui.Controls.Resource;
using View = Android.Views.View;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.Messaging;
using static DailyBudgetMAUIApp.Pages.ViewAccounts;


namespace MAUISample.Platforms.Android
{
    [Service]
    public class FloatingService : Service, IOnTouchListener
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        public bool TextIsUpdating;
        string previousText;

        WindowManagerLayoutParams layoutParams;
        IWindowManager windowManager;
        View floatView;

        public FloatingService()
        {
            _pt = MauiApplication.Current.Services.GetService<IProductTools>()
                          ?? throw new InvalidOperationException("IProductTools is not registered.");
            _ds = MauiApplication.Current.Services.GetService<IRestDataService>()
                          ?? throw new InvalidOperationException("IRestDataService is not registered.");
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }


        public override bool OnUnbind(Intent? intent)
        {
            return base.OnUnbind(intent);
        }


        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            showFloatingWindow();
            return StartCommandResult.NotSticky;
        }

        private void TextChangedEvent(object sender, TextChangedEventArgs e)
        {

        }


        private void showFloatingWindow()
        {
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            LayoutInflater mLayoutInflater = LayoutInflater.From(ApplicationContext);
            floatView = mLayoutInflater.Inflate(Resource.Layout.floatview, null);
            floatView.SetBackgroundResource(Resource.Drawable.rounded_background);
            floatView.SetOnTouchListener(this);
            floatView.Elevation = 10;
            TextView titleText = floatView.FindViewById<TextView>(Resource.Id.titleText);
            if(App.DefaultBudget != null)
            {
                titleText.Text = App.DefaultBudget.BudgetName;
            }
            else
            {
                titleText.Text = "";
            }
            Button closeButton = floatView.FindViewById<Button>(Resource.Id.closeButton);
            closeButton.Click += delegate 
            {
                OnDestroy();
            };
            Button submitButton = floatView.FindViewById<Button>(Resource.Id.submitButton);
            submitButton.Click += delegate 
            {
                EditText transactionAmount = floatView.FindViewById<EditText>(Resource.Id.inputField);
                decimal TransactionAmount = (decimal)_pt.FormatCurrencyNumber(transactionAmount.Text);
                if(TransactionAmount > 0)
                {
                    if (floatView != null)
                    {
                        Toast.MakeText(ApplicationContext, "Processing transaction", ToastLength.Short).Show();
                        windowManager.RemoveView(floatView);
                    }

                    Transactions T = new Transactions
                    {
                        TransactionAmount = TransactionAmount,
                        IsSpendFromSavings = false,
                        SavingID = null,
                        SavingName = null,
                        TransactionDate = DateTime.UtcNow,
                        WhenAdded = DateTime.UtcNow,
                        IsIncome = false,
                        Category = null,
                        Payee = null,
                        Notes = null,
                        CategoryID = null,
                        IsTransacted = true,
                        SavingsSpendType = null,
                        EventType = "Transaction"
                    };

                    Task.Run(async () =>
                    {
                        await _ds.SaveNewTransaction(T, App.DefaultBudget.BudgetID);
                        WeakReferenceMessenger.Default.Send(new UpdateViewAccount(true, true));                   

                    });

                }
            };
            EditText transactionAmount = floatView.FindViewById<EditText>(Resource.Id.inputField);
            transactionAmount.Text = 0.ToString("c", CultureInfo.CurrentCulture);
            previousText = transactionAmount.Text;
            transactionAmount.TextChanged +=  delegate
            {
                if(!TextIsUpdating)
                {
                    TextIsUpdating = true;

                    int CurrentPosition = transactionAmount.SelectionStart;
                    string Value = transactionAmount.Text;
                    if(string.IsNullOrWhiteSpace(Value))
                    {
                        Value = "0";
                    }
                    decimal TransactionAmount = (decimal)_pt.FormatCurrencyNumber(Value);
                    transactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);
                    int position = Value.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
                    if (!string.IsNullOrEmpty(previousText) && (previousText.Length - position) == 2 && CurrentPosition > position)
                    {
                        transactionAmount.SetSelection(position);
                    }
                    else
                    { 
                        if (CurrentPosition == (previousText.Length + 1))
                        {
                            transactionAmount.SetSelection(transactionAmount.Text.Length);
                        }
                        else
                        {
                            transactionAmount.SetSelection(CurrentPosition);
                        }
                        
                    }

                    previousText = transactionAmount.Text;
                    TextIsUpdating = false;
                }
            };




            // set LayoutParam
            layoutParams = new WindowManagerLayoutParams();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                layoutParams.Type = WindowManagerTypes.Phone;
            }
            layoutParams.Flags = WindowManagerFlags.NotTouchModal;
            layoutParams.Flags = WindowManagerFlags.WatchOutsideTouch;
            layoutParams.Flags = WindowManagerFlags.LayoutNoLimits;

            int screenWidth = Resources.DisplayMetrics.WidthPixels;
            int screenHeight = Resources.DisplayMetrics.HeightPixels;

            layoutParams.Width = screenWidth - 100;
            layoutParams.Height = 300;

            layoutParams.X = 0;
            layoutParams.Y = 400;
            windowManager.AddView(floatView, layoutParams);


        }


        private int x;
        private int y;
        public bool OnTouch(global::Android.Views.View? v, MotionEvent? e)
        {
            try
            {
                switch (e?.Action)
                {
                    case MotionEventActions.Down:
                        x = (int)e.RawX;
                        y = (int)e.RawY;
                        break;

                    case MotionEventActions.Move:
                        int nowX = (int)e.RawX;
                        int nowY = (int)e.RawY;
                        int movedX = nowX - x;
                        int movedY = nowY - y;
                        x = nowX;
                        y = nowY;

                        layoutParams.X += movedX;
                        layoutParams.Y += movedY;


                        int screenWidth = Resources.DisplayMetrics.WidthPixels;
                        int screenHeight = Resources.DisplayMetrics.HeightPixels;

                        int viewHeight = floatView.MeasuredHeight;
                        int viewWidth = floatView.MeasuredWidth;

                        if (Math.Abs(layoutParams.Y) > screenHeight / 2)
                        {
                            StopSelf();
                            return true;
                        }

                        if (Math.Abs(layoutParams.X) > viewWidth / 2)
                        {
                            StopSelf();
                            return true;
                        }

                        // Update the floating view's layout
                        windowManager.UpdateViewLayout(floatView, layoutParams);
                        break;
                }
            }
            catch 
            {

            }

            return true; // Consume the touch event
        }

        public override IBinder? OnBind(Intent? intent)
        {
            throw new NotImplementedException();
        }

        public override void OnDestroy()
        {

            try
            {
                base.OnDestroy();
                if (floatView != null)
                {
                    Toast.MakeText(ApplicationContext, "Quick transaction closed", ToastLength.Short).Show();

                    windowManager.RemoveView(floatView);

                }
            }
            catch 
            {

            }
        }

    }
}