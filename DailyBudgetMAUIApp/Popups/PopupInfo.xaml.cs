using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupInfo : Popup
{
    public double ScreenWidth { get; }
    public double ScreenHeight { get; }
    public double PopupWidth { get; }
    public double ButtonOneWidth { get; }
    public PopupInfo(string Title, List<string> SubTitles, List<string> Info)
    {
        InitializeComponent();

        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        PopupWidth = ScreenWidth -  30;
        ButtonOneWidth = ((PopupWidth - 100) / 2);

        lblTitle.Text = Title;

        Application.Current.Resources.TryGetValue("Gray700", out var Gray700);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        var VertLayout = new VerticalStackLayout
        {
            Margin = new Thickness(0, 0, 0, 5),
            WidthRequest = PopupWidth,
            VerticalOptions = LayoutOptions.Center
        };

        int i = 0;
        foreach(string details in Info)
        {
            var VerticalLayout = new VerticalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 20),
                WidthRequest = (PopupWidth - 30),
                VerticalOptions = LayoutOptions.Center
            };

            if (SubTitles[i] != "")
            {
                var Subtitle = new Label
                {
                    Text = SubTitles[i],
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = (Color)Gray900,
                    Margin = new Thickness(0, 0, 0, 10),
                    HorizontalTextAlignment = TextAlignment.Center
                };

                VerticalLayout.Add(Subtitle);
            }

            var Para = new Label
            {
                Text = details,
                FontSize = 12,
                TextColor = (Color)Gray700,
                LineBreakMode = LineBreakMode.WordWrap
            };

            i++;

            VerticalLayout.Add(Para);
            VertLayout.Add(VerticalLayout);

        }

        Parent.Add(VertLayout);

        BindingContext = this;
    }

	private void Close_Popup(object sender, EventArgs e)
	{
        this.Close();
    }
}