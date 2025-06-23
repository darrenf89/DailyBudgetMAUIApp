using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupMoveAccountBalanceViewModel : BaseViewModel, IQueryAttributable
    {
        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }

        public decimal OriginalAmount { get; set; }
        public DateTime OriginalDate { get; set; }
        [ObservableProperty]
        public partial decimal Amount { get; set; }

        [ObservableProperty]
        public partial DateTime Date { get; set; }

        [ObservableProperty]
        public partial bool FromEnabled { get; set; }

        [ObservableProperty]
        public partial bool ToEnabled { get; set; }

        [ObservableProperty]
        public partial string FromNewBalanceString { get; set; }

        [ObservableProperty]
        public partial string ToNewBalanceString { get; set; }

        [ObservableProperty]
        public partial MoveBalanceClass FromSelectedMoveBalance { get; set; }

        [ObservableProperty]
        public partial MoveBalanceClass ToSelectedMoveBalance { get; set; }

        [ObservableProperty]
        public partial Style FromBalanceStyle { get; set; }

        [ObservableProperty]
        public partial Style ToBalanceStyle { get; set; }

        [ObservableProperty]
        public partial Color FromBalanceColor { get; set; }

        [ObservableProperty]
        public partial Color ToBalanceColor { get; set; }

        [ObservableProperty]
        public partial bool IsCoverOverspend { get; set; }

        [ObservableProperty]
        public partial Budgets Budget { get; set; }

        [ObservableProperty]
        public partial BankAccounts ToAccount { get; set; }

        [ObservableProperty]
        public partial List<BankAccounts> FromAccounts { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<MoveBalanceClass> MoveBalances { get; set; } = new();


        public PopupMoveAccountBalanceViewModel()
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(nameof(FromAccounts), out var fromAccounts) && fromAccounts is List<BankAccounts> fromList)
            {
                FromAccounts = fromList;
            }

            if (query.TryGetValue(nameof(ToAccount), out var toAccount) && toAccount is BankAccounts to)
            {
                ToAccount = to;
            }
        }

        partial void OnFromSelectedMoveBalanceChanged(MoveBalanceClass oldValue, MoveBalanceClass newValue)
        {
            RecalculatedBalances();
        }

        partial void OnToSelectedMoveBalanceChanged(MoveBalanceClass oldValue, MoveBalanceClass newValue)
        {
            RecalculatedBalances();
        }

        partial void OnAmountChanged(decimal oldValue, decimal newValue)
        {
            RecalculatedBalances();
        }

        public void RecalculatedBalances() 
        {
            if (!(FromSelectedMoveBalance is null || ToSelectedMoveBalance is null))
            {
                Application.Current.Resources.TryGetValue("Success", out var Success);
                Application.Current.Resources.TryGetValue("Danger", out var Danger);
                Application.Current.Resources.TryGetValue("pillSuccess", out var pillSuccess);
                Application.Current.Resources.TryGetValue("pillDanger", out var pillDanger);

                decimal FromAmount = FromSelectedMoveBalance.Balance;
                decimal ToAmount = ToSelectedMoveBalance.Balance;

                FromAmount -= Amount;
                ToAmount += Amount;

                if (FromAmount < 0)
                {
                    FromBalanceColor = (Color)Danger;
                    FromBalanceStyle = (Style)pillDanger;
                }
                else
                {
                    FromBalanceColor = (Color)Success;
                    FromBalanceStyle = (Style)pillSuccess;
                }

                if (ToAmount < 0)
                {
                    ToBalanceColor = (Color)Danger;
                    ToBalanceStyle = (Style)pillDanger;
                }
                else
                {
                    ToBalanceColor = (Color)Success;
                    ToBalanceStyle = (Style)pillSuccess;
                }

                if (ToSelectedMoveBalance is null)
                {
                    ToNewBalanceString = "-";
                }
                else
                {
                    ToNewBalanceString = ToAmount.ToString("c", CultureInfo.CurrentCulture);
                }

                if (FromSelectedMoveBalance is null)
                {
                    FromNewBalanceString = "-";
                }
                else
                {
                    FromNewBalanceString = FromAmount.ToString("c", CultureInfo.CurrentCulture);
                }

            }
            else
            {          
                if (ToSelectedMoveBalance is null)
                {
                    ToNewBalanceString = "-";
                }
                else
                {
                    decimal ToAmount = ToSelectedMoveBalance.Balance;
                    ToNewBalanceString = ToAmount.ToString("c", CultureInfo.CurrentCulture);
                }

                if (FromSelectedMoveBalance is null)
                {
                    FromNewBalanceString = "-";
                }
                else
                {
                    decimal FromAmount = FromSelectedMoveBalance.Balance;
                    FromNewBalanceString = FromAmount.ToString("c", CultureInfo.CurrentCulture);
                }
            }

        }


    }
}
