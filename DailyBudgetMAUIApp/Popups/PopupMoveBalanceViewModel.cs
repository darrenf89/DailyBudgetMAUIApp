using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupMoveBalanceViewModel : BaseViewModel
    {
        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }

        public decimal OriginalAmount { get; set; }
        public DateTime OriginalDate { get; set; }
        [ObservableProperty]
        public decimal amount;
        [ObservableProperty]
        public DateTime date;
        [ObservableProperty]
        public bool fromEnabled;
        [ObservableProperty]
        public bool toEnabled;        
        [ObservableProperty]
        public string fromNewBalanceString;
        [ObservableProperty]
        public string toNewBalanceString;        
        [ObservableProperty]
        public MoveBalanceClass fromSelectedMoveBalance;
        [ObservableProperty]
        public MoveBalanceClass toSelectedMoveBalance;
        [ObservableProperty]
        public Style fromBalanceStyle;
        [ObservableProperty]
        public Style toBalanceStyle;
        [ObservableProperty]
        public Color fromBalanceColor;
        [ObservableProperty]
        public Color toBalanceColor;        
        [ObservableProperty]
        public bool isCoverOverspend;

        [ObservableProperty]
        public Budgets budget;
        [ObservableProperty]
        public ObservableCollection<MoveBalanceClass> moveBalances = new();

        public PopupMoveBalanceViewModel()
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
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

    public class MoveBalanceClass
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
    }
}
