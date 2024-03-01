namespace DailyBudgetMAUIApp.Popups
{
    public class LoadingPageTwo : ContentPage
    {
        private ActivityIndicator _indicator;
        private VerticalStackLayout _stackLayout;

        public LoadingPageTwo()
        {
            double ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;

            BackgroundColor = new Color(0, 0, 0, 1);

            Application.Current.Resources.TryGetValue("Primary", out var Primary);
            Application.Current.Resources.TryGetValue("Light", out var Light);
            Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

            _indicator = new ActivityIndicator
            {
                IsRunning = true,
                Color = (Color)Primary,
                Scale = 1,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 400, 0, 0),
                Opacity = 1
            };

            var loadingLabel = new Label()
            {
                Text = "Loading...",
                TextColor = (Color)Primary,
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 12,
                Margin = new Thickness(0, 20, 0, 0),
                FontAttributes = FontAttributes.Bold,
                Opacity = 1
            };

            _stackLayout = new VerticalStackLayout
            {
                WidthRequest = ScreenWidth,
                HeightRequest = ScreenHeight,
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                Opacity = 1,
                Children =
            {
                _indicator,
                loadingLabel
            }
            };

            Content = _stackLayout;
        }

        public async Task Dismiss()
        {
            await Navigation.PopModalAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _indicator.IsRunning = false;
            _stackLayout.IsVisible = false;
        }

        protected override bool OnBackButtonPressed()
        {
            // Return true to prevent the back button from closing the page
            return true;
        }
    }
}
