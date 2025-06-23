using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpPageVariableInputViewModel : BaseViewModel, IQueryAttributable
    {
        [ObservableProperty]
        public partial bool ReturnDataError { get; set; }

        [ObservableProperty]
        public partial bool IsSubDesc { get; set; }

        [ObservableProperty]
        public partial DateTime DateTimeInput { get; set; }

        [ObservableProperty]
        public partial decimal DecimalInput { get; set; }

        [ObservableProperty]
        public partial decimal StringInput { get; set; }

        [ObservableProperty]
        public partial string Type { get; set; }
        [ObservableProperty]
        public partial string TitleText { get; set; }
        [ObservableProperty]
        public partial string Description { get; set; }
        [ObservableProperty]
        public partial string DescriptionSub { get; set; }
        [ObservableProperty]
        public partial string Placeholder { get; set; }
        [ObservableProperty]
        public partial object Input { get; set; }

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }

        public PopUpPageVariableInputViewModel()
        {
            ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.8;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(nameof(TitleText), out var titleText) && titleText is string title)
            {
                TitleText = title;
            }

            if (query.TryGetValue(nameof(Description), out var description) && description is string desc)
            {
                Description = desc;
            }

            if (query.TryGetValue(nameof(DescriptionSub), out var descriptionSub) && descriptionSub is string descSub)
            {
                DescriptionSub = descSub;
            }

            if (query.TryGetValue(nameof(Placeholder), out var placeholder) && placeholder is string ph)
            {
                Placeholder = ph;
            }

            if (query.TryGetValue(nameof(Input), out var input) && input is string val)
            {
                Input = val;
            }

            if (query.TryGetValue(nameof(Type), out var type) && type is string t)
            {
                Type = t;
            }
        }
    }
}
