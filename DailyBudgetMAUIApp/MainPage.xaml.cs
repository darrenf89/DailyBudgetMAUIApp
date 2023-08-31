using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using System.Diagnostics;

namespace DailyBudgetMAUIApp;

public partial class MainPage : ContentPage
{
    private readonly IRestDataService _ds;
    int count = 0;

	public MainPage(IRestDataService ds)
	{
		InitializeComponent();
		_ds = ds;

	}

	private async void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		string salt = await _ds.GetUserSaltAsync("Darren.fillis100gmail.com");

		Debug.WriteLine($"----> {salt}");

		CounterBtn.Text = salt;


		SemanticScreenReader.Announce(CounterBtn.Text);
	}

    private async void SignOutClicked(object sender, EventArgs e)
    {
		if(Preferences.ContainsKey(nameof(App.UserDetails)))
		{
			Preferences.Remove(nameof(App.UserDetails));
		}

		await Shell.Current.GoToAsync(nameof(LogonPage));
    }
}

