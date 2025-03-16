using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Converters;
using System.ComponentModel;
using System.Globalization;
using static Android.Telecom.Call;



namespace DailyBudgetMAUIApp.Models
{
    public partial class Savings : ObservableObject
    {
        [ObservableProperty]
        private string? savingsType;
        [ObservableProperty]
        private string? savingsName;
        [ObservableProperty]
        private decimal? currentBalance = 0;
        [ObservableProperty]
        private DateTime lastUpdatedDate = DateTime.UtcNow;
        [ObservableProperty]
        private DateTime? goalDate = null;
        [ObservableProperty]
        private decimal? lastUpdatedValue;
        [ObservableProperty]
        private bool isSavingsClosed = false;
        [ObservableProperty]
        private decimal? savingsGoal = 0;
        [ObservableProperty]
        private bool canExceedGoal;
        [ObservableProperty]
        private bool isDailySaving;
        [ObservableProperty]
        private bool isRegularSaving;
        [ObservableProperty]
        private decimal? regularSavingValue;
        [ObservableProperty]
        private decimal? periodSavingValue;
        [ObservableProperty]
        private bool isAutoComplete;
        [ObservableProperty]
        private bool isTopUp;
        [ObservableProperty]
        private string ddlSavingsPeriod;
        [ObservableProperty]
        private int savingID = 0;

        public decimal? SavingProgressBarMax { get; set; }
        public string SavingProgressBarMaxString { get; set; }
        public string RegularSavingValueString { get; set; }
        public string SavingGoalDateString { get; set; }
        public bool IsSavingsBuilder { get; set; }
        public bool IsSavingsTargetAmount { get; set; }
        public bool IsSavingsTargetDate { get; set; }
        public string SavingToGlyph { get; set; }

        partial void OnSavingsTypeChanged(string value)
        {
            UpdateSavingProgressBarMaxString();
            UpdateSavingProgressBarMax();
            UpdateSavingGoalDateString();
            UpdateIsSavingsBuilder();
            UpdateIsSavingsTargetAmount();
            UpdateIsSavingsTargetDate();
            UpdateSavingToGlyph();
        }

        partial void OnCurrentBalanceChanged(decimal? value)
        {
            UpdateSavingProgressBarMaxString();
            UpdateSavingProgressBarMax();
        }

        partial void OnSavingsGoalChanged(decimal? value)
        {
            UpdateSavingProgressBarMaxString();
            UpdateSavingProgressBarMax();
        }

        partial void OnRegularSavingValueChanged(decimal? value)
        {
            UpdateRegularSavingValueString();
        }

        partial void OnIsSavingsClosedChanged(bool value)
        {
            UpdateRegularSavingValueString();
            UpdateSavingGoalDateString();
        }

        partial void OnGoalDateChanged(DateTime? value)
        {
            UpdateSavingGoalDateString();
        }

        private void UpdateSavingProgressBarMaxString()
        {
            if (this.SavingsType == "SavingsBuilder")
            {
                SavingProgressBarMaxString = this.CurrentBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }
            else
            {
                SavingProgressBarMaxString = this.SavingsGoal.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }
        }

        private void UpdateSavingProgressBarMax()
        {
            if (this.SavingsType == "SavingsBuilder")
            {
                SavingProgressBarMax = this.CurrentBalance;
            }
            else
            {
                SavingProgressBarMax = this.SavingsGoal;
            }
        }
        private void UpdateRegularSavingValueString()
        {
            if (this.IsSavingsClosed)
            {
                RegularSavingValueString = "Saving Closed";
            }
            else
            {
                RegularSavingValueString = this.RegularSavingValue.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }
        }
        private void UpdateSavingGoalDateString()
        {
            if (this.IsSavingsClosed)
            {
                SavingGoalDateString = "Saving Closed";
            }
            else
            {
                if (this.SavingsType == "SavingsBuilder")
                {
                    SavingGoalDateString = "Continuous Saving";
                }
                else
                {
                    SavingGoalDateString =  this.GoalDate.GetValueOrDefault().ToString("dd MMM yy");
                }

            }
        }
        private void UpdateIsSavingsBuilder()
        {
            if (this.SavingsType == "SavingsBuilder")
            {
                IsSavingsBuilder =  true;
            }
            else
            {
                IsSavingsBuilder = false;
            }
        }
        private void UpdateIsSavingsTargetAmount()
        {
            if (this.SavingsType == "TargetAmount")
            {
                IsSavingsTargetAmount =  true;
            }
            else
            {
                IsSavingsTargetAmount = false;
            }
        }
        private void UpdateIsSavingsTargetDate()
        {
            if (this.SavingsType == "TargetDate")
            {
                IsSavingsTargetDate =  true;
            }
            else
            {
                IsSavingsTargetDate = false;
            }
        }

        private void UpdateSavingToGlyph()
        {
            if (this.SavingsType == "TargetDate")
            {
                SavingToGlyph = "date.svg";
            }
            else if (this.SavingsType == "SavingsBuilder")
            {
                SavingToGlyph = "builder.svg";
            }
            else if (this.SavingsType == "TargetAmount")
            {
                SavingToGlyph = "dollar.svg";
            }
            else
            {
                SavingToGlyph = "income.svg";
            }
        }
    }

}
