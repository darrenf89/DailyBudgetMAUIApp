using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class IncomeEvents : ObservableObject
    {
        [ObservableProperty]
        private int  incomeEventID;
        [ObservableProperty]
        private decimal  incomeAmount;
        [ObservableProperty]
        private string  incomeName  = "";
        [ObservableProperty]
        private DateTime  incomeActiveDate  = DateTime.UtcNow;
        [ObservableProperty]
        private DateTime  dateOfIncomeEvent  = DateTime.UtcNow;
        [ObservableProperty]
        private bool  isRecurringIncome;
        [ObservableProperty]
        private string?  recurringIncomeType;
        [ObservableProperty]
        private int?  recurringIncomeValue;
        [ObservableProperty]
        private string?  recurringIncomeDuration;
        [ObservableProperty]
        private bool  isClosed;
        [ObservableProperty]
        private bool?  isInstantActive;
        [ObservableProperty]
        private bool?  isIncomeAddedToBalance  = false;
        [ObservableProperty]
        public int? accountID;

        public string IncomeTypeConverter { get; set; }
        public string RecurringIncomeDetails { get; set; }

        partial void OnIsRecurringIncomeChanged(bool value)
        {
            UpdateIncomeTypeConverter();
            UpdateRecurringIncomeDetails();
        }

        partial void OnIsInstantActiveChanged(bool? value)
        {
            UpdateIncomeTypeConverter();
        }

        partial void OnRecurringIncomeTypeChanged(string? value)
        {
            UpdateRecurringIncomeDetails();
        }

        partial void OnRecurringIncomeValueChanged(int? value)
        {
            UpdateRecurringIncomeDetails();
        }

        partial void OnRecurringIncomeDurationChanged(string? value)
        {
            UpdateRecurringIncomeDetails();
        }

        private void UpdateIncomeTypeConverter()
        {
            string ReturnValue;

            if (this.IsRecurringIncome)
            {
                ReturnValue = "Recurring ";
            }
            else
            {
                ReturnValue = "One-off ";
            }

            if (this.IsInstantActive ?? false)
            {
                ReturnValue = ReturnValue + "| Instant Active ";
            }
            else
            {
                ReturnValue = ReturnValue + "| On Received";
            }

            IncomeTypeConverter = ReturnValue;
        }

        private void UpdateRecurringIncomeDetails()
        {
            if (this.IsRecurringIncome)
            {
                if (this.RecurringIncomeType == "Everynth")
                {
                    RecurringIncomeDetails = this.RecurringIncomeValue == 1 ? $"Every {this.RecurringIncomeDuration.Replace("s", "")}" : $"Every {this.RecurringIncomeValue} {this.RecurringIncomeDuration}";
                }
                else if (this.RecurringIncomeType == "OfEveryMonth")
                {
                    string DayString;
                    if (this.RecurringIncomeValue == 1)
                    {
                        DayString = $"{this.RecurringIncomeValue}st";
                    }
                    else if (this.RecurringIncomeValue == 2)
                    {
                        DayString = $"{this.RecurringIncomeValue}nd";
                    }
                    else if (this.RecurringIncomeValue == 3)
                    {
                        DayString = $"{this.RecurringIncomeValue}rd";
                    }
                    else
                    {
                        DayString = $"{this.RecurringIncomeValue}th";
                    }

                    RecurringIncomeDetails = $"{DayString} of the month";
                }
                else
                {
                    RecurringIncomeDetails = "";
                }

            }
            else
            {
                RecurringIncomeDetails = "";
            }
        }

    }
}
