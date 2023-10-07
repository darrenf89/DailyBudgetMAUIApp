using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupInfo : Popup
{
    public double ScreenWidth { get; }
    public double ScreenHeight { get; }
    public double PopupWidth { get; }
    public PopupInfo(string Title, List<string> SubTitles, List<string> Info)
    {
        InitializeComponent();

        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        PopupWidth = ScreenWidth -  0;

        lblTitle.Text = Title;

        Application.Current.Resources.TryGetValue("Gray700", out var Gray700);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        var VertLayout = new VerticalStackLayout
        {
            Margin = new Thickness(0, 0, 0, 5)
        };

        int i = 0;
        foreach(string details in Info)
        {
            var HorizontalLayout = new HorizontalStackLayout
            {
                Margin = new Thickness(5, 0, 5, 5)
            };

            if (SubTitles[i] != "")
            {
                var Subtitle = new Label
                {
                    Text = SubTitles[i] + ":",
                    FontSize = 8,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = (Color)Gray900
                };

                HorizontalLayout.Add(Subtitle);
            }

            var Para = new Label
            {
                Text = details,
                FontSize = 8,
                TextColor = (Color)Gray700
            };

            i++;

            HorizontalLayout.Add(Para);
            VertLayout.Add(HorizontalLayout);

        }

        Parent.Add(VertLayout);

        BindingContext = this;
    }

	private void Close_Popup(object sender, EventArgs e)
	{
        this.Close();
    }
}