using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Bills : ObservableObject
    {
        [ObservableProperty]
        private int  billID;
        [ObservableProperty]
        public string?  billName;
        [ObservableProperty]
        public string?  billType;
        [ObservableProperty]
        public int?  billValue;
        [ObservableProperty]
        public string?  billDuration;
        [ObservableProperty]
        public decimal?  billAmount;
        [ObservableProperty]
        public DateTime?  billDueDate;
        [ObservableProperty]
        public decimal  billCurrentBalance;
        [ObservableProperty]
        public decimal  billBalanceAtLastPayDay;
        [ObservableProperty]
        public bool  isRecuring;
        [ObservableProperty]
        public DateTime  lastUpdatedDate;
        [ObservableProperty]
        public bool  isClosed;
        [ObservableProperty]
        public decimal?  regularBillValue;
        [ObservableProperty]
        public string  billPayee;
        [ObservableProperty]
        public string? category = "";
        [ObservableProperty]
        public int? categoryID = 0;
        [ObservableProperty]
        public int? accountID;
        public string RecurringBillDetails { get; set; }
        public string BillToGlyph { get; set; }

        partial void OnBillValueChanged(int? value)
        {
            UpdateSavingProgressBarMaxString();
        }

        partial void OnBillDurationChanged(string? value)
        {
            UpdateSavingProgressBarMaxString();
        }
        partial void OnBillTypeChanged(string? value)
        {
            UpdateSavingProgressBarMaxString();
        }

        partial void OnIsRecuringChanged(bool value)
        {
            UpdateBillToGlyph();
        }

        private void UpdateSavingProgressBarMaxString()
        {
            if (this.IsRecuring)
            {
                if (this.BillType == "Everynth")
                {
                    RecurringBillDetails =  this.BillValue == 1 ? $"Every {this.BillDuration.Replace("s", "")}" : $"Every {this.BillValue} {this.BillDuration}";
                }
                else if (this.BillType == "OfEveryMonth")
                {
                    string DayString;
                    if (this.BillValue == 1 || this.BillValue == 21 || this.BillValue == 31)
                    {
                        DayString = $"{this.BillValue}st";
                    }
                    else if (this.BillValue == 2 || this.BillValue == 22)
                    {
                        DayString = $"{this.BillValue}nd";
                    }
                    else if (this.BillValue == 3 || this.BillValue == 23)
                    {
                        DayString = $"{this.BillValue}rd";
                    }
                    else
                    {
                        DayString = $"{this.BillValue}th";
                    }

                    RecurringBillDetails = $"{DayString} of the month";
                }
                else if (this.BillType == "WorkingDays")
                {
                    RecurringBillDetails = this.BillValue == 1 ? $"{NumberToWords(this.BillValue.GetValueOrDefault())} working day before the end of month" : $"{NumberToWords(this.BillValue.GetValueOrDefault())} working days before the end of month"; ;
                }
                else if (this.BillType == "LastOfTheMonth")
                {
                    RecurringBillDetails = $"Last {this.BillDuration} of the month";
                }
                else
                {
                    RecurringBillDetails = "";
                }

            }
            else
            {
                RecurringBillDetails = "";
            }
        }

        private void UpdateBillToGlyph()
        {
            if (this.IsRecuring)
            {
                BillToGlyph = "recurring.svg";
            }
            else
            {
                BillToGlyph = "one.svg";
            }
        }

        private string NumberToWords(int number)
        {
            if (number == 0) return "Zero";

            string[] units = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 10) return units[number];
            if (number < 20) return teens[number - 10];
            if (number < 100) return tens[number / 10] + (number % 10 != 0 ? " " + units[number % 10] : "");

            return "Number too large"; // Extend this for larger numbers if needed.
        }

    }
}
